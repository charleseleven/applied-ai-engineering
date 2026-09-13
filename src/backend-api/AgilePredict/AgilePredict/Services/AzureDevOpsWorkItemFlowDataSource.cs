using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgilePredict.Models.Configuration;
using AgilePredict.Models.Flow;
using AgilePredict.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace AgilePredict.Services
{
    /// <summary>
    /// Coleta Work Items e seu histórico de transição de status via API REST do Azure DevOps
    /// (Task #212). Autentica com Personal Access Token (Basic Auth) e normaliza a resposta
    /// para o contrato <see cref="WorkItemFlowSnapshot"/> consumido pelo motor estatístico (Task #213).
    /// </summary>
    public class AzureDevOpsWorkItemFlowDataSource : IWorkItemFlowDataSource
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly HttpClient _httpClient;
        private readonly AzureDevOpsConfiguration _configuration;
        private readonly FlowAnalyticsConfiguration _flowConfiguration;
        private readonly ILogger<AzureDevOpsWorkItemFlowDataSource> _logger;

        private static readonly string[] TrackedFields =
        {
            "System.Id",
            "System.Title",
            "System.State",
            "System.AssignedTo",
            "System.CreatedDate",
            "System.IterationPath"
        };

        public AzureDevOpsWorkItemFlowDataSource(
            HttpClient httpClient,
            IOptions<AzureDevOpsConfiguration> configuration,
            IOptions<FlowAnalyticsConfiguration> flowConfiguration,
            ILogger<AzureDevOpsWorkItemFlowDataSource> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _configuration = configuration?.Value ?? throw new ArgumentNullException(nameof(configuration));
            _flowConfiguration = flowConfiguration?.Value ?? throw new ArgumentNullException(nameof(flowConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var token = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{_configuration.PersonalAccessToken}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<IReadOnlyList<WorkItemFlowSnapshot>> GetActiveWorkItemsAsync(
            string iterationPath,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(iterationPath))
            {
                throw new ArgumentException("O caminho da iteração (sprint) é obrigatório", nameof(iterationPath));
            }

            var doneStatuses = string.Join(", ", _flowConfiguration.DoneStatuses.Select(Quote));
            var wiql = $@"SELECT [System.Id] FROM WorkItems
                          WHERE [System.TeamProject] = @project
                          AND [System.IterationPath] = '{Escape(iterationPath)}'
                          AND [System.State] NOT IN ({doneStatuses})
                          AND [System.State] <> 'Removed'";

            var ids = await RunWiqlAsync(wiql, cancellationToken);
            return await LoadSnapshotsAsync(ids, cancellationToken);
        }

        public async Task<IReadOnlyList<WorkItemFlowSnapshot>> GetCompletedWorkItemsSinceAsync(
            DateTime sinceUtc,
            CancellationToken cancellationToken = default)
        {
            var doneStatuses = string.Join(", ", _flowConfiguration.DoneStatuses.Select(Quote));
            var wiql = $@"SELECT [System.Id] FROM WorkItems
                          WHERE [System.TeamProject] = @project
                          AND [System.State] IN ({doneStatuses})
                          AND [System.ChangedDate] >= '{sinceUtc:yyyy-MM-dd}'";

            var ids = await RunWiqlAsync(wiql, cancellationToken);
            return await LoadSnapshotsAsync(ids, cancellationToken);
        }

        private async Task<List<int>> RunWiqlAsync(string wiql, CancellationToken cancellationToken)
        {
            var requestUri = $"{_configuration.Project}/_apis/wit/wiql?api-version={_configuration.ApiVersion}";
            var payload = JsonSerializer.Serialize(new { query = wiql });
            using var content = new StringContent(payload, Encoding.UTF8, "application/json");

            using var response = await _httpClient.PostAsync(requestUri, content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<WiqlResult>(body, JsonOptions);

            return result?.WorkItems?.Select(w => w.Id).ToList() ?? new List<int>();
        }

        private async Task<IReadOnlyList<WorkItemFlowSnapshot>> LoadSnapshotsAsync(
            IReadOnlyList<int> ids,
            CancellationToken cancellationToken)
        {
            if (ids.Count == 0)
            {
                return Array.Empty<WorkItemFlowSnapshot>();
            }

            var details = await GetWorkItemDetailsAsync(ids, cancellationToken);
            var snapshots = new List<WorkItemFlowSnapshot>(details.Count);

            foreach (var detail in details)
            {
                var history = await GetStatusHistoryAsync(detail.Id, detail.CreatedAtUtc, detail.CurrentStatus, cancellationToken);
                snapshots.Add(new WorkItemFlowSnapshot
                {
                    ExternalId = detail.Id,
                    Title = detail.Title,
                    AssignedTo = detail.AssignedTo,
                    CreatedAtUtc = detail.CreatedAtUtc,
                    CurrentStatus = detail.CurrentStatus,
                    IsDone = _flowConfiguration.DoneStatuses.Contains(detail.CurrentStatus, StringComparer.OrdinalIgnoreCase),
                    StatusHistory = history
                });
            }

            return snapshots;
        }

        private async Task<List<WorkItemDetail>> GetWorkItemDetailsAsync(IReadOnlyList<int> ids, CancellationToken cancellationToken)
        {
            var requestUri = $"{_configuration.Project}/_apis/wit/workitemsbatch?api-version={_configuration.ApiVersion}";
            var payload = JsonSerializer.Serialize(new { ids, fields = TrackedFields });
            using var content = new StringContent(payload, Encoding.UTF8, "application/json");

            using var response = await _httpClient.PostAsync(requestUri, content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(body);

            var details = new List<WorkItemDetail>();
            if (!document.RootElement.TryGetProperty("value", out var values))
            {
                return details;
            }

            foreach (var item in values.EnumerateArray())
            {
                var id = item.GetProperty("id").GetInt32();
                var fields = item.GetProperty("fields");

                details.Add(new WorkItemDetail
                {
                    Id = id,
                    Title = GetString(fields, "System.Title") ?? $"Work Item #{id}",
                    CurrentStatus = GetString(fields, "System.State") ?? "New",
                    AssignedTo = GetAssignedTo(fields),
                    CreatedAtUtc = GetDate(fields, "System.CreatedDate") ?? DateTime.UtcNow
                });
            }

            return details;
        }

        private async Task<List<WorkItemStatusPeriod>> GetStatusHistoryAsync(
            int workItemId,
            DateTime createdAtUtc,
            string currentStatus,
            CancellationToken cancellationToken)
        {
            var requestUri = $"{_configuration.Project}/_apis/wit/workitems/{workItemId}/updates?api-version={_configuration.ApiVersion}";
            using var response = await _httpClient.GetAsync(requestUri, cancellationToken);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(body);

            var periods = new List<WorkItemStatusPeriod>();

            if (!document.RootElement.TryGetProperty("value", out var updates))
            {
                periods.Add(new WorkItemStatusPeriod { Status = currentStatus, EnteredAtUtc = createdAtUtc });
                return periods;
            }

            foreach (var update in updates.EnumerateArray())
            {
                if (!update.TryGetProperty("fields", out var fields) ||
                    !fields.TryGetProperty("System.State", out var stateChange))
                {
                    continue;
                }

                if (!stateChange.TryGetProperty("newValue", out var newValueElement))
                {
                    continue;
                }

                var newStatus = newValueElement.GetString() ?? currentStatus;
                var revisedAt = GetDate(update, "revisedDate") ?? createdAtUtc;

                if (periods.Count > 0)
                {
                    periods[^1].ExitedAtUtc = revisedAt;
                }

                periods.Add(new WorkItemStatusPeriod { Status = newStatus, EnteredAtUtc = revisedAt });
            }

            if (periods.Count == 0)
            {
                periods.Add(new WorkItemStatusPeriod { Status = currentStatus, EnteredAtUtc = createdAtUtc });
            }
            else
            {
                periods[0].EnteredAtUtc = createdAtUtc;
            }

            return periods;
        }

        private static string? GetString(JsonElement fields, string fieldName)
        {
            return fields.TryGetProperty(fieldName, out var value) && value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;
        }

        private static DateTime? GetDate(JsonElement container, string fieldName)
        {
            if (!container.TryGetProperty(fieldName, out var value))
            {
                return null;
            }

            return value.ValueKind switch
            {
                JsonValueKind.String when DateTime.TryParse(
                    value.GetString(),
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.AdjustToUniversal | System.Globalization.DateTimeStyles.AssumeUniversal,
                    out var parsed) => parsed,
                _ => null
            };
        }

        private static string? GetAssignedTo(JsonElement fields)
        {
            if (!fields.TryGetProperty("System.AssignedTo", out var value))
            {
                return null;
            }

            if (value.ValueKind == JsonValueKind.String)
            {
                return value.GetString();
            }

            if (value.ValueKind == JsonValueKind.Object && value.TryGetProperty("displayName", out var displayName))
            {
                return displayName.GetString();
            }

            return null;
        }

        private static string Quote(string value) => $"'{Escape(value)}'";

        private static string Escape(string value) => value.Replace("'", "''");

        #region DTOs internos de deserialização

        private sealed class WorkItemDetail
        {
            public int Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public string CurrentStatus { get; set; } = string.Empty;
            public string? AssignedTo { get; set; }
            public DateTime CreatedAtUtc { get; set; }
        }

        private sealed class WiqlResult
        {
            [JsonPropertyName("workItems")]
            public List<WiqlWorkItemRef>? WorkItems { get; set; }
        }

        private sealed class WiqlWorkItemRef
        {
            public int Id { get; set; }
        }

        #endregion
    }
}

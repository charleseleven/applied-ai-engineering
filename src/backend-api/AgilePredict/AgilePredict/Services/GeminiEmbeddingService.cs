using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgilePredict.Models.Configuration;
using AgilePredict.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace AgilePredict.Services
{
    /// <summary>
    /// Implementação do serviço de embeddings via Gemini API
    /// </summary>
    public class GeminiEmbeddingService : IEmbeddingService
    {
        private readonly HttpClient _httpClient;
        private readonly EmbeddingConfiguration _configuration;
        private readonly ILogger<GeminiEmbeddingService> _logger;

        public GeminiEmbeddingService(
            HttpClient httpClient,
            IOptions<EmbeddingConfiguration> configuration,
            ILogger<GeminiEmbeddingService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _configuration = configuration?.Value ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _httpClient.DefaultRequestHeaders.Add("x-goog-api-key", _configuration.ApiKey);
        }

        public async Task<float[]?> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                _logger.LogWarning("Tentativa de gerar embedding para texto vazio");
                return null;
            }

            try
            {
                var payload = new GeminiEmbedRequest
                {
                    Model = $"models/{_configuration.Model}",
                    Content = new GeminiContent
                    {
                        Parts = [new GeminiPart { Text = text }]
                    },
                    OutputDimensionality = _configuration.OutputDimensionality
                };

                var jsonContent = JsonSerializer.Serialize(payload);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    $"models/{_configuration.Model}:embedContent", httpContent, cancellationToken);

                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "Falha ao gerar embedding via Gemini. Status: {Status}. Corpo: {Body}",
                        response.StatusCode, responseBody);
                    return null;
                }

                var apiResponse = JsonSerializer.Deserialize<GeminiEmbedResponse>(
                    responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // A API pode responder no formato em lote (embeddings[]) ou no formato singular (embedding)
                var values = apiResponse?.Embeddings?.FirstOrDefault()?.Values
                             ?? apiResponse?.Embedding?.Values;

                if (values == null || values.Length == 0)
                {
                    _logger.LogError("Resposta da API de embeddings sem vetor válido: {Body}", responseBody);
                    return null;
                }

                _logger.LogInformation("Embedding gerado com sucesso ({Dimensions} dimensões)", values.Length);
                return values;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Timeout ao gerar embedding via Gemini");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao gerar embedding via Gemini");
                return null;
            }
        }

        #region Classes auxiliares para (de)serialização

        private class GeminiEmbedRequest
        {
            [JsonPropertyName("model")]
            public string Model { get; set; } = string.Empty;

            [JsonPropertyName("content")]
            public GeminiContent Content { get; set; } = null!;

            [JsonPropertyName("output_dimensionality")]
            public int OutputDimensionality { get; set; }
        }

        private class GeminiContent
        {
            [JsonPropertyName("parts")]
            public GeminiPart[] Parts { get; set; } = [];
        }

        private class GeminiPart
        {
            [JsonPropertyName("text")]
            public string Text { get; set; } = string.Empty;
        }

        private class GeminiEmbedResponse
        {
            public GeminiEmbeddingValues[]? Embeddings { get; set; }
            public GeminiEmbeddingValues? Embedding { get; set; }
        }

        private class GeminiEmbeddingValues
        {
            public float[]? Values { get; set; }
        }

        #endregion
    }
}

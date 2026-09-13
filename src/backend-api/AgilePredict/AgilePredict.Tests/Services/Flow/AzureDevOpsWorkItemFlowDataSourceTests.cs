using System.Net;
using System.Net.Http.Headers;
using AgilePredict.Models.Configuration;
using AgilePredict.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace AgilePredict.Tests.Services.Flow
{
    public class AzureDevOpsWorkItemFlowDataSourceTests
    {
        private readonly Mock<ILogger<AzureDevOpsWorkItemFlowDataSource>> _loggerMock = new();

        private readonly AzureDevOpsConfiguration _config = new()
        {
            ApiUrl = "https://dev.azure.com/",
            Organization = "org",
            Project = "proj",
            PersonalAccessToken = "pat123",
            ApiVersion = "7.1"
        };

        private readonly FlowAnalyticsConfiguration _flowConfig = new()
        {
            DoneStatuses = new List<string> { "Done" }
        };

        private AzureDevOpsWorkItemFlowDataSource CreateDataSource(Mock<HttpMessageHandler> handlerMock)
        {
            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri($"{_config.ApiUrl}{_config.Organization}/")
            };

            return new AzureDevOpsWorkItemFlowDataSource(
                httpClient,
                Options.Create(_config),
                Options.Create(_flowConfig),
                _loggerMock.Object);
        }

        private static HttpResponseMessage JsonResponse(string json) => new(HttpStatusCode.OK)
        {
            Content = new StringContent(json)
        };

        [Fact]
        public async Task GetActiveWorkItemsAsync_BuildsSnapshotWithStatusHistoryAndAssignee()
        {
            var handlerMock = new Mock<HttpMessageHandler>();

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.AbsolutePath.EndsWith("/wiql")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(JsonResponse("{\"workItems\":[{\"id\":101}]}"));

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.AbsolutePath.EndsWith("/workitemsbatch")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(JsonResponse(@"{
                    ""value"": [{
                        ""id"": 101,
                        ""fields"": {
                            ""System.Title"": ""Card Test"",
                            ""System.State"": ""In Progress"",
                            ""System.AssignedTo"": { ""displayName"": ""Jane Doe"" },
                            ""System.CreatedDate"": ""2026-01-01T00:00:00Z""
                        }
                    }]
                }"));

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.AbsolutePath.EndsWith("/updates")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(JsonResponse(@"{
                    ""value"": [
                        { ""rev"": 1, ""revisedDate"": ""2026-01-01T00:00:00Z"", ""fields"": { ""System.State"": { ""newValue"": ""To Do"" } } },
                        { ""rev"": 2, ""revisedDate"": ""2026-01-02T00:00:00Z"", ""fields"": { ""System.State"": { ""oldValue"": ""To Do"", ""newValue"": ""In Progress"" } } }
                    ]
                }"));

            var dataSource = CreateDataSource(handlerMock);

            var result = await dataSource.GetActiveWorkItemsAsync("proj\\Sprint 1");

            var snapshot = Assert.Single(result);
            Assert.Equal(101, snapshot.ExternalId);
            Assert.Equal("Card Test", snapshot.Title);
            Assert.Equal("Jane Doe", snapshot.AssignedTo);
            Assert.Equal("In Progress", snapshot.CurrentStatus);
            Assert.False(snapshot.IsDone);

            Assert.Equal(2, snapshot.StatusHistory.Count);
            Assert.Equal("To Do", snapshot.StatusHistory[0].Status);
            Assert.Equal(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), snapshot.StatusHistory[0].EnteredAtUtc);
            Assert.Equal(new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc), snapshot.StatusHistory[0].ExitedAtUtc);

            Assert.Equal("In Progress", snapshot.StatusHistory[1].Status);
            Assert.Null(snapshot.StatusHistory[1].ExitedAtUtc);
        }

        [Fact]
        public async Task GetActiveWorkItemsAsync_WhenNoWorkItemsMatch_ReturnsEmptyWithoutFurtherCalls()
        {
            var handlerMock = new Mock<HttpMessageHandler>();

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(JsonResponse("{\"workItems\":[]}"));

            var dataSource = CreateDataSource(handlerMock);
            var result = await dataSource.GetActiveWorkItemsAsync("proj\\Sprint 1");

            Assert.Empty(result);
        }

        [Fact]
        public void Constructor_SetsBasicAuthorizationHeaderWithPersonalAccessToken()
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("https://dev.azure.com/org/") };

            _ = new AzureDevOpsWorkItemFlowDataSource(
                httpClient,
                Options.Create(_config),
                Options.Create(_flowConfig),
                _loggerMock.Object);

            var expectedToken = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($":{_config.PersonalAccessToken}"));
            Assert.Equal(new AuthenticationHeaderValue("Basic", expectedToken), httpClient.DefaultRequestHeaders.Authorization);
        }
    }
}

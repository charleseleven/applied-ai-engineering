using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AgilePredict.Services;
using AgilePredict.Services.Interfaces;
using AgilePredict.Models.Configuration;
using AgilePredict.Models.DTOs;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq.Protected;

namespace AgilePredict.Tests.Services
{
    public class LlmIntegrationServiceTests
    {
        private readonly Mock<ILogger<LlmIntegrationService>> _loggerMock;
        private readonly Mock<IOptions<LlmConfiguration>> _configMock;
        private readonly LlmConfiguration _config;

        public LlmIntegrationServiceTests()
        {
            _loggerMock = new Mock<ILogger<LlmIntegrationService>>();
            _configMock = new Mock<IOptions<LlmConfiguration>>();

            _config = new LlmConfiguration
            {
                ApiUrl = "https://api.openai.com",
                ApiKey = "test-api-key",
                DefaultModel = "gpt-3.5-turbo",
                TimeoutSeconds = 30,
                MaxRetries = 3
            };

            _configMock.Setup(c => c.Value).Returns(_config);
        }

        [Fact]
        public async Task SendPromptAsync_WithValidPrompt_ReturnsSuccessResponse()
        {
            // Arrange
            var httpClientMock = CreateMockHttpClient(
                HttpStatusCode.OK,
                "{\"model\":\"gpt-3.5-turbo\",\"choices\":[{\"message\":{\"content\":\"Test response\"}}],\"usage\":{\"total_tokens\":10}}"
            );

            var service = new LlmIntegrationService(
                httpClientMock,
                _configMock.Object,
                _loggerMock.Object
            );

            // Act
            var result = await service.SendPromptAsync("Test prompt");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Test response", result.Content);
            Assert.Equal(10, result.TokensUsed);
        }

        [Fact]
        public async Task SendPromptAsync_WithEmptyPrompt_ReturnsErrorResponse()
        {
            // Arrange
            var httpClientMock = CreateMockHttpClient(HttpStatusCode.OK, "{}");
            var service = new LlmIntegrationService(
                httpClientMock,
                _configMock.Object,
                _loggerMock.Object
            );

            // Act
            var result = await service.SendPromptAsync("");

            // Assert
            Assert.False(result.Success);
            Assert.Contains("vazio", result.ErrorMessage);
        }

        [Fact]
        public async Task SendPromptAsync_WithApiError_ReturnsErrorResponse()
        {
            // Arrange
            var httpClientMock = CreateMockHttpClient(
                HttpStatusCode.InternalServerError,
                "Error"
            );

            var service = new LlmIntegrationService(
                httpClientMock,
                _configMock.Object,
                _loggerMock.Object
            );

            // Act
            var result = await service.SendPromptAsync("Test prompt");

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.ErrorMessage);
        }

        [Fact]
        public async Task ValidateConnectionAsync_WithSuccessfulConnection_ReturnsTrue()
        {
            // Arrange
            var httpClientMock = CreateMockHttpClient(
                HttpStatusCode.OK,
                "{\"model\":\"gpt-3.5-turbo\",\"choices\":[{\"message\":{\"content\":\"OK\"}}],\"usage\":{\"total_tokens\":5}}"
            );

            var service = new LlmIntegrationService(
                httpClientMock,
                _configMock.Object,
                _loggerMock.Object
            );

            // Act
            var result = await service.ValidateConnectionAsync();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task SendPromptWithOptionsAsync_WithCustomTemperature_UsesProvidedValue()
        {
            // Arrange
            var httpClientMock = CreateMockHttpClient(
                HttpStatusCode.OK,
                "{\"model\":\"gpt-3.5-turbo\",\"choices\":[{\"message\":{\"content\":\"Response\"}}],\"usage\":{\"total_tokens\":10}}"
            );

            var service = new LlmIntegrationService(
                httpClientMock,
                _configMock.Object,
                _loggerMock.Object
            );

            var request = new LlmRequest
            {
                Prompt = "Test",
                Temperature = 0.9,
                MaxTokens = 100
            };

            // Act
            var result = await service.SendPromptWithOptionsAsync(request);

            // Assert
            Assert.True(result.Success);
        }

        // Helper method para criar HttpClient mockado
        private HttpClient CreateMockHttpClient(HttpStatusCode statusCode, string content)
        {
            var handlerMock = new Mock<HttpMessageHandler>();

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(content)
                });

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("https://api.openai.com")
            };

            return httpClient;
        }
    }
}

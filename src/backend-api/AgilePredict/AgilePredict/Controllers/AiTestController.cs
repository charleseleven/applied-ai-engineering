using Microsoft.AspNetCore.Mvc;
using AgilePredict.Models.DTOs;
using AgilePredict.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace AgilePredict.Controllers
{
    /// <summary>
    /// Controller para testes de integração com a API LLM
    /// </summary>
    [ApiController]
    [Route("api/ai")]
    [Produces("application/json")]
    public class AiTestController : ControllerBase
    {
        private readonly ILlmIntegrationService _llmService;
        private readonly ILogger<AiTestController> _logger;

        public AiTestController(
            ILlmIntegrationService llmService,
            ILogger<AiTestController> logger)
        {
            _llmService = llmService ?? throw new ArgumentNullException(nameof(llmService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Endpoint de teste para validar integração com API LLM
        /// Conforme US-21 AC #3: Deve retornar status 200 OK com resposta em texto da IA
        /// </summary>
        /// <param name="request">Request com prompt e configurações</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Resposta da LLM em formato JSON</returns>
        /// <response code="200">Resposta processada com sucesso pela IA</response>
        /// <response code="400">Request inválido (validação falhou)</response>
        /// <response code="500">Erro ao comunicar com a API LLM</response>
        [HttpPost("test")]
        [ProducesResponseType(typeof(LlmResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(LlmResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LlmResponse>> TestLlmConnection(
            [FromBody] LlmRequest request,
            CancellationToken cancellationToken = default)
        {
            // Validação automática via DataAnnotations
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Request inválido para teste de LLM: {Errors}", 
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Iniciando teste de conexão LLM com prompt: {Prompt}", 
                request.Prompt.Length > 50 ? request.Prompt.Substring(0, 50) + "..." : request.Prompt);

            try
            {
                var response = await _llmService.SendPromptWithOptionsAsync(request, cancellationToken);

                if (!response.Success)
                {
                    _logger.LogError("Falha ao processar prompt: {Error}", response.ErrorMessage);
                    return StatusCode(StatusCodes.Status500InternalServerError, response);
                }

                _logger.LogInformation("Teste de LLM concluído com sucesso. Tokens: {Tokens}", 
                    response.TokensUsed);

                // ✅ Critério de Aceitação #3: Retorna 200 OK com resposta em texto da IA
                return Ok(response);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Requisição cancelada pelo cliente");
                return StatusCode(StatusCodes.Status499ClientClosedRequest, new LlmResponse
                {
                    Success = false,
                    ErrorMessage = "Requisição cancelada"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao testar conexão com LLM");
                return StatusCode(StatusCodes.Status500InternalServerError, new LlmResponse
                {
                    Success = false,
                    ErrorMessage = "Erro interno do servidor ao processar requisição"
                });
            }
        }

        /// <summary>
        /// Endpoint simplificado para teste rápido (apenas texto)
        /// </summary>
        /// <param name="prompt">Texto do prompt</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Apenas o texto da resposta</returns>
        /// <response code="200">Texto da resposta da IA</response>
        /// <response code="400">Prompt vazio ou inválido</response>
        [HttpPost("test/simple")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<string>> TestSimple(
            [FromBody][Required] string prompt,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                return BadRequest("O prompt não pode ser vazio");
            }

            var response = await _llmService.SendPromptAsync(prompt, cancellationToken);

            if (!response.Success)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, response.ErrorMessage);
            }

            return Ok(response.Content);
        }

        /// <summary>
        /// Health check da integração com LLM
        /// </summary>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Status da conexão</returns>
        /// <response code="200">Conexão OK</response>
        /// <response code="503">Serviço indisponível</response>
        [HttpGet("health")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult> HealthCheck(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Executando health check da LLM API");

            var isHealthy = await _llmService.ValidateConnectionAsync(cancellationToken);

            if (!isHealthy)
            {
                _logger.LogError("Health check da LLM API falhou");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    status = "Unhealthy",
                    service = "LLM API",
                    timestamp = DateTime.UtcNow
                });
            }

            _logger.LogInformation("Health check da LLM API bem-sucedido");

            return Ok(new
            {
                status = "Healthy",
                service = "LLM API",
                timestamp = DateTime.UtcNow
            });
        }
    }
}

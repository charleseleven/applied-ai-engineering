using System.ComponentModel.DataAnnotations;
using AgilePredict.Models.DTOs;
using AgilePredict.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgilePredict.Controllers
{
    /// <summary>
    /// Controller para o FAQ inteligente (RAG): perguntas respondidas com base nos dados das Tasks
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class FaqController : ControllerBase
    {
        private readonly IRagService _ragService;
        private readonly ILogger<FaqController> _logger;

        public FaqController(IRagService ragService, ILogger<FaqController> logger)
        {
            _ragService = ragService;
            _logger = logger;
        }

        /// <summary>
        /// Executa o pipeline RAG: busca os 3 registros mais similares no banco e responde
        /// estritamente com base nesse contexto, evitando alucinações da IA.
        /// </summary>
        /// <param name="request">Pergunta em linguagem natural</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <response code="200">Resposta gerada com sucesso pelo pipeline RAG</response>
        /// <response code="400">Pergunta inválida</response>
        [HttpPost("ask")]
        [ProducesResponseType(typeof(RagAnswer), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RagAnswer>> Ask(
            [FromBody] AskFaqRequest request,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.Question))
            {
                return BadRequest(new { error = "A pergunta não pode ser vazia." });
            }

            _logger.LogInformation("FAQ RAG: nova pergunta recebida: {Question}", request.Question);

            var result = await _ragService.AskAsync(request.Question, cancellationToken);

            if (!result.Success)
            {
                _logger.LogWarning("FAQ RAG: falha ao responder. Motivo: {Error}", result.ErrorMessage);
            }

            return Ok(result);
        }
    }

    public class AskFaqRequest
    {
        [Required]
        public string Question { get; set; } = string.Empty;
    }
}

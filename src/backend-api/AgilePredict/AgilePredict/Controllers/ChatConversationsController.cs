using AgilePredict.Data;
using AgilePredict.Models;
using AgilePredict.Models.DTOs;
using AgilePredict.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgilePredict.Controllers
{
    /// <summary>
    /// Controller para gerenciamento de conversas do Chat IA (histórico persistido)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ChatConversationsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILlmIntegrationService _llmService;

        public ChatConversationsController(AppDbContext context, ILlmIntegrationService llmService)
        {
            _context = context;
            _llmService = llmService;
        }

        /// <summary>
        /// Retorna o resumo de todas as conversas, mais recente primeiro (sem as mensagens)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ChatConversationSummary>>> GetConversations()
        {
            return await _context.ChatConversations
                .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
                .Select(c => new ChatConversationSummary
                {
                    Id = c.Id,
                    Title = c.Title,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .ToListAsync();
        }

        /// <summary>
        /// Retorna uma conversa específica com todas as suas mensagens
        /// </summary>
        /// <param name="id">ID da conversa</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<ChatConversation>> GetConversation(int id)
        {
            var conversation = await _context.ChatConversations
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (conversation == null)
            {
                return NotFound();
            }

            conversation.Messages = conversation.Messages
                .OrderBy(m => m.CreatedAt)
                .ThenBy(m => m.Id)
                .ToList();

            return conversation;
        }

        /// <summary>
        /// Cria uma nova conversa vazia
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ChatConversation>> PostConversation()
        {
            var conversation = new ChatConversation();

            _context.ChatConversations.Add(conversation);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetConversation), new { id = conversation.Id }, conversation);
        }

        /// <summary>
        /// Envia uma mensagem numa conversa: persiste a pergunta, consulta a LLM e persiste a resposta
        /// </summary>
        /// <param name="id">ID da conversa</param>
        /// <param name="request">Prompt do usuário</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        [HttpPost("{id}/messages")]
        public async Task<ActionResult<ChatExchangeResult>> PostMessage(
            int id,
            [FromBody] SendMessageRequest request,
            CancellationToken cancellationToken = default)
        {
            var conversation = await _context.ChatConversations
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (conversation == null)
            {
                return NotFound();
            }

            var userMessage = new ChatMessage
            {
                ConversationId = conversation.Id,
                Role = "user",
                Content = request.Prompt
            };
            _context.ChatMessages.Add(userMessage);

            var response = await _llmService.SendPromptAsync(request.Prompt, cancellationToken);

            var assistantMessage = new ChatMessage
            {
                ConversationId = conversation.Id,
                Role = "assistant",
                Content = response.Success ? response.Content : (response.ErrorMessage ?? "A IA não conseguiu processar sua solicitação."),
                IsError = !response.Success
            };
            _context.ChatMessages.Add(assistantMessage);

            if (string.IsNullOrEmpty(conversation.Title))
            {
                conversation.Title = Truncate(request.Prompt, 50);
            }
            conversation.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return new ChatExchangeResult
            {
                ConversationId = conversation.Id,
                ConversationTitle = conversation.Title,
                UserMessage = userMessage,
                AssistantMessage = assistantMessage
            };
        }

        /// <summary>
        /// Apaga uma conversa e todas as suas mensagens
        /// </summary>
        /// <param name="id">ID da conversa</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConversation(int id)
        {
            var conversation = await _context.ChatConversations.FindAsync(id);
            if (conversation == null)
            {
                return NotFound();
            }

            _context.ChatConversations.Remove(conversation);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private static string Truncate(string text, int maxLength)
        {
            var trimmed = text.Trim();
            return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength] + "…";
        }
    }

    public class SendMessageRequest
    {
        public string Prompt { get; set; } = string.Empty;
    }
}

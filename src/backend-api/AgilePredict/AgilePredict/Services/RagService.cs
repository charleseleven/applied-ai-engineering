using System.Text;
using System.Text.Json;
using AgilePredict.Data;
using AgilePredict.Models.DTOs;
using AgilePredict.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgilePredict.Services
{
    /// <summary>
    /// Orquestra o pipeline RAG: recuperação por similaridade + injeção de contexto no prompt + chamada à LLM
    /// </summary>
    public class RagService : IRagService
    {
        private const int TopKResults = 3;

        private readonly AppDbContext _context;
        private readonly IEmbeddingService _embeddingService;
        private readonly ISimilarityService _similarityService;
        private readonly ILlmIntegrationService _llmService;
        private readonly ILogger<RagService> _logger;

        public RagService(
            AppDbContext context,
            IEmbeddingService embeddingService,
            ISimilarityService similarityService,
            ILlmIntegrationService llmService,
            ILogger<RagService> logger)
        {
            _context = context;
            _embeddingService = embeddingService;
            _similarityService = similarityService;
            _llmService = llmService;
            _logger = logger;
        }

        public async Task<RagAnswer> AskAsync(string question, CancellationToken cancellationToken = default)
        {
            var trimmedQuestion = question?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(trimmedQuestion))
            {
                return Failure(trimmedQuestion, "A pergunta não pode ser vazia.");
            }

            // 1. Gera o embedding da pergunta
            var questionVector = await _embeddingService.GenerateEmbeddingAsync(trimmedQuestion, cancellationToken);
            if (questionVector == null)
            {
                return Failure(trimmedQuestion, "Não foi possível processar a pergunta (falha ao gerar o embedding).");
            }

            // 2. Busca candidatos com embedding já calculado
            var candidates = await _context.ProjectTasks
                .Where(t => t.Embedding != null)
                .ToListAsync(cancellationToken);

            if (candidates.Count == 0)
            {
                return Failure(trimmedQuestion,
                    "Não há dados suficientes na base para responder (nenhuma Task com embedding disponível ainda).");
            }

            // 3. Ranqueia por similaridade de cosseno e pega os TopK mais similares
            var ranked = candidates
                .Select(task => new
                {
                    Task = task,
                    Similarity = _similarityService.ComputeCosineSimilarity(
                        questionVector,
                        JsonSerializer.Deserialize<float[]>(task.Embedding!) ?? [])
                })
                .OrderByDescending(x => x.Similarity)
                .Take(TopKResults)
                .ToList();

            _logger.LogInformation(
                "RAG: {Count} registros recuperados para a pergunta. Similaridades: {Similarities}",
                ranked.Count, string.Join(", ", ranked.Select(r => r.Similarity.ToString("F3"))));

            // 4. Monta o System Prompt injetando o contexto recuperado
            var systemPrompt = BuildSystemPrompt(ranked.Select(r => r.Task));

            // 5. Consulta a LLM restringida ao contexto
            var llmResponse = await _llmService.SendPromptWithOptionsAsync(new Models.DTOs.LlmRequest
            {
                Prompt = trimmedQuestion,
                SystemPrompt = systemPrompt,
                Temperature = 0.2 // baixa, para respostas mais factuais/aderentes ao contexto
            }, cancellationToken);

            if (!llmResponse.Success)
            {
                return Failure(trimmedQuestion, llmResponse.ErrorMessage ?? "Falha ao consultar a IA.");
            }

            return new RagAnswer
            {
                Question = trimmedQuestion,
                Answer = llmResponse.Content,
                Success = true,
                Sources = ranked.Select(r => new RagSource
                {
                    TaskId = r.Task.Id,
                    Title = r.Task.Title,
                    Similarity = Math.Round(r.Similarity, 4)
                }).ToList()
            };
        }

        private static string BuildSystemPrompt(IEnumerable<Models.ProjectTask> sources)
        {
            var sb = new StringBuilder();
            sb.AppendLine(
                "Você é um assistente que responde perguntas de um Gestor EXCLUSIVAMENTE com base no contexto " +
                "abaixo, extraído do banco de dados de Tasks do projeto. Não utilize nenhum conhecimento externo " +
                "ou suposição além do que está escrito no contexto. Se a resposta não puder ser determinada a " +
                "partir do contexto fornecido, responda exatamente: \"Não há informação suficiente nos dados " +
                "disponíveis para responder a essa pergunta.\" Não invente informações.");
            sb.AppendLine();
            sb.AppendLine("Contexto (registros mais relevantes encontrados no banco de dados):");

            int index = 1;
            foreach (var task in sources)
            {
                sb.AppendLine(
                    $"{index}. Título: {task.Title} | Status: {task.Status} | Prioridade: {task.Priority} | " +
                    $"Story Points: {task.StoryPoints} | Descrição: {task.Description}");
                index++;
            }

            return sb.ToString();
        }

        private static RagAnswer Failure(string question, string errorMessage) => new()
        {
            Question = question,
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}

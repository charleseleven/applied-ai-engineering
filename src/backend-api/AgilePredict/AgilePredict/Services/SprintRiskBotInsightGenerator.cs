using System.Text.Json;
using AgilePredict.Models.DTOs;
using AgilePredict.Models.DTOs.SprintRisk;
using AgilePredict.Services.Interfaces;

namespace AgilePredict.Services
{
    /// <summary>
    /// Implementação da Task #162: constrói o System Prompt dinâmico com os dados em tempo real
    /// calculados pela API (capacidade histórica do time, janela de observação) e traduz a
    /// resposta da LLM em um diagnóstico estruturado (narrativa + recomendações).
    /// </summary>
    public class SprintRiskBotInsightGenerator : ISprintRiskBotInsightGenerator
    {
        private readonly ILlmIntegrationService _llmService;
        private readonly ILogger<SprintRiskBotInsightGenerator> _logger;

        public SprintRiskBotInsightGenerator(ILlmIntegrationService llmService, ILogger<SprintRiskBotInsightGenerator> logger)
        {
            _llmService = llmService ?? throw new ArgumentNullException(nameof(llmService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<SprintRiskInsightResult> GenerateAsync(
            IReadOnlyList<SprintRiskAssessment> assessments,
            double historicalDailyCapacity,
            int historicalSprintsUsed,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(assessments);

            var payloadJson = JsonSerializer.Serialize(assessments, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var request = new LlmRequest
            {
                Prompt = payloadJson,
                SystemPrompt = BuildSystemPrompt(historicalDailyCapacity, historicalSprintsUsed),
                Temperature = 0.3,
                MaxTokens = 700
            };

            var response = await _llmService.SendPromptWithOptionsAsync(request, cancellationToken);

            if (!response.Success || string.IsNullOrWhiteSpace(response.Content))
            {
                _logger.LogWarning("Falha ao gerar insight do bot de risco via LLM: {Error}", response.ErrorMessage);
                return new SprintRiskInsightResult
                {
                    Success = false,
                    ErrorMessage = response.ErrorMessage ?? "A LLM não retornou conteúdo"
                };
            }

            return ParseInsight(response.Content);
        }

        /// <summary>
        /// Template dinâmico (Task #162): capacidade histórica e janela de observação são
        /// calculadas em tempo real a cada requisição, não fixas no texto do prompt.
        /// </summary>
        private static string BuildSystemPrompt(double historicalDailyCapacity, int historicalSprintsUsed)
        {
            // Raw string (não interpolado) com placeholders {0}/{1} em vez de $"""...""": o
            // exemplo de JSON de saída abaixo já usa chaves literais, e combinar interpolação
            // com chaves literais num mesmo raw string exigiria escalonar os delimitadores "$$".
            const string template = """
                Você é um bot especializado em prever gargalos de entrega em Sprints ativas, atuando
                proativamente na saúde de um time ágil para um Scrum Master.

                Contexto calculado em tempo real para esta requisição:
                - Capacidade diária histórica do time: {0:0.0} Story Points/dia, calculada a
                  partir das últimas {1} sprint(s) concluída(s) (ou de um valor padrão quando não
                  há histórico suficiente).

                Você recebe um payload JSON com uma lista de Sprints ativas, cada uma contendo: pontos
                totais, pontos concluídos, pontos restantes, dias restantes, o ritmo diário necessário
                para concluir no prazo (requiredDailyPace), o total projetado de entrega com base na
                capacidade histórica informada acima (projectedTotalCompletion) e o déficit projetado
                (projectedShortfall).

                Regras:
                - Escreva a narrativa e as recomendações em português do Brasil, independente do idioma dos dados recebidos.
                - Avalie o risco comparando requiredDailyPace com a capacidade histórica diária informada.
                - Seja específico: cite o título de cada Sprint em risco e o tamanho do déficit projetado.
                - Seja proativo: sugira ações concretas (redistribuir tarefas, reduzir escopo, reforçar o time).
                - Nunca invente dados que não estejam no payload.

                Responda ESTRITAMENTE em JSON, sem markdown e sem texto fora do JSON, no formato:
                {{"narrative": "<diagnóstico geral em até 4 frases>", "recommendations": ["<sugestão 1>", "<sugestão 2>"]}}
                """;

            return string.Format(template, historicalDailyCapacity, historicalSprintsUsed);
        }

        private SprintRiskInsightResult ParseInsight(string content)
        {
            var jsonPayload = ExtractJsonObject(content);

            try
            {
                var parsed = JsonSerializer.Deserialize<LlmInsightPayload>(
                    jsonPayload,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (parsed is not null && !string.IsNullOrWhiteSpace(parsed.Narrative))
                {
                    return new SprintRiskInsightResult
                    {
                        Success = true,
                        Narrative = parsed.Narrative,
                        Recommendations = parsed.Recommendations ?? new List<string>()
                    };
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Resposta da LLM não estava no formato JSON esperado; usando fallback textual");
            }

            // Fallback: a LLM não seguiu o formato JSON solicitado. Trata o texto bruto como
            // narrativa e tenta extrair recomendações de linhas em formato de lista.
            var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var recommendations = lines
                .Where(l => l.StartsWith('-') || l.StartsWith('*') || System.Text.RegularExpressions.Regex.IsMatch(l, @"^\d+[\.\)]"))
                .Select(l => l.TrimStart('-', '*', ' ').Trim())
                .Where(l => l.Length > 0)
                .ToList();

            return new SprintRiskInsightResult
            {
                Success = true,
                Narrative = content.Trim(),
                Recommendations = recommendations
            };
        }

        private static string ExtractJsonObject(string content)
        {
            var start = content.IndexOf('{');
            var end = content.LastIndexOf('}');
            return start >= 0 && end > start ? content[start..(end + 1)] : content;
        }

        private sealed class LlmInsightPayload
        {
            public string? Narrative { get; set; }
            public List<string>? Recommendations { get; set; }
        }
    }
}

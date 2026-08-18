using AgilePredict.Data;
using AgilePredict.Models.Configuration;
using AgilePredict.Services;
using AgilePredict.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Extensions.Http;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configuração para evitar ciclos em referências circulares
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Configuração do Entity Framework Core com SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ===== HEALTH CHECKS =====
// Adiciona health checks para monitorar a saúde da API
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>(
        name: "database",
        tags: ["db", "sql"])
    .AddUrlGroup(
        uri: new Uri(builder.Configuration["LlmConfiguration:ApiUrl"] ?? "https://api.groq.com"),
        name: "llm-api",
        tags: ["external", "llm"]);

// ===== CONFIGURAÇÃO LLM (GROQ + LLAMA-4) =====
// Bind and validate configuration
builder.Services.Configure<LlmConfiguration>(
    builder.Configuration.GetSection(LlmConfiguration.SectionName));

builder.Services.AddOptions<LlmConfiguration>()
    .Bind(builder.Configuration.GetSection(LlmConfiguration.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Register HttpClient with Polly resilience policies
builder.Services.AddHttpClient<ILlmIntegrationService, LlmIntegrationService>((serviceProvider, client) =>
    {
        var config = serviceProvider
            .GetRequiredService<Microsoft.Extensions.Options.IOptions<LlmConfiguration>>()
            .Value;

        client.BaseAddress = new Uri(config.ApiUrl);
        client.Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds);
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.ApiKey}");
        client.DefaultRequestHeaders.Add("User-Agent", "AgilePredict/1.0");
    })
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy())
    .SetHandlerLifetime(TimeSpan.FromMinutes(5));

// ===== CONFIGURAÇÃO DE EMBEDDINGS (GEMINI) =====
// Serviço separado do LLM de chat (Groq) — a Groq não tem endpoint de embeddings.
builder.Services.Configure<EmbeddingConfiguration>(
    builder.Configuration.GetSection(EmbeddingConfiguration.SectionName));

builder.Services.AddOptions<EmbeddingConfiguration>()
    .Bind(builder.Configuration.GetSection(EmbeddingConfiguration.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddHttpClient<IEmbeddingService, GeminiEmbeddingService>((serviceProvider, client) =>
{
    var config = serviceProvider
        .GetRequiredService<Microsoft.Extensions.Options.IOptions<EmbeddingConfiguration>>()
        .Value;

    client.BaseAddress = new Uri(config.ApiUrl);
    client.Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds);
});

// ===== FILA DE TRABALHOS EM BACKGROUND =====
// Usada para gerar embeddings de forma assíncrona após uma Task ser criada, sem bloquear a resposta HTTP.
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddHostedService<QueuedHostedService>();

// ===== PIPELINE RAG (FAQ INTELIGENTE) =====
builder.Services.AddSingleton<ISimilarityService, CosineSimilarityService>();
builder.Services.AddScoped<IRagService, RagService>();

// ===== CORS (dev only) =====
// Libera o Nuxt dev server (frontend-app) para consumir a API em desenvolvimento.
// Qualquer porta em localhost/127.0.0.1 é aceita porque o Nuxt troca de porta
// automaticamente (3000, 3001, ...) quando a porta padrão já está em uso.
const string DevCorsPolicy = "NuxtDevClient";
builder.Services.AddCors(options =>
{
    options.AddPolicy(DevCorsPolicy, policy =>
    {
        policy.SetIsOriginAllowed(origin =>
              {
                  if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri)) return false;
                  return uri.Scheme == "http" && (uri.Host == "localhost" || uri.Host == "127.0.0.1");
              })
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new()
        {
            Title = "AgilePredict API",
            Version = "v1",
            Description = "API para gerenciamento de projetos ágeis com previsões de IA",
            Contact = new()
            {
                Name = "AgilePredict Team"
            }
        };
        return Task.CompletedTask;
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Adiciona interface interativa do Scalar para documentação da API
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("AgilePredict API")
            .WithTheme(ScalarTheme.Purple)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseCors(DevCorsPolicy);
}

app.UseAuthorization();

// ===== HEALTH CHECK ENDPOINTS =====
// Endpoint básico: verifica se a API está rodando
app.MapHealthChecks("/health");

// Endpoint detalhado: verifica API + Database + LLM API
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds,
                tags = e.Value.Tags
            }),
            totalDuration = report.TotalDuration.TotalMilliseconds
        };
        await context.Response.WriteAsJsonAsync(response);
    }
});

app.MapControllers();

app.Run();

// ===== POLLY RESILIENCE POLICIES =====

/// <summary>
/// Retry Policy: Tenta novamente em caso de falhas transientes
/// - 3 tentativas com backoff exponencial (2^n segundos)
/// - Loga cada tentativa para observabilidade
/// </summary>
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryCount, context) =>
            {
                var message = outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString();
                Console.WriteLine($"[Retry] Tentativa {retryCount} de 3 após {timespan.TotalSeconds}s. Razão: {message}");
            });
}

/// <summary>
/// Circuit Breaker Policy: Protege o sistema de falhas em cascata
/// - Abre o circuito após 5 falhas consecutivas
/// - Permanece aberto por 30 segundos antes de tentar novamente
/// - Loga mudanças de estado do circuito
/// </summary>
static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5,
            durationOfBreak: TimeSpan.FromSeconds(30),
            onBreak: (outcome, duration) =>
            {
                var message = outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString();
                Console.WriteLine($"[Circuit Breaker] ⚠️  Circuito ABERTO por {duration.TotalSeconds}s. Razão: {message}");
            },
            onReset: () =>
            {
                Console.WriteLine("[Circuit Breaker] ✅ Circuito FECHADO. Requisições normalizadas.");
            },
            onHalfOpen: () =>
            {
                Console.WriteLine("[Circuit Breaker] 🔄 Circuito MEIO-ABERTO. Testando recuperação...");
            });
}

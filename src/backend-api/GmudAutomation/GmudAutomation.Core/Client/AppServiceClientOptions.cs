namespace GmudAutomation.Core.Client;

/// <summary>
/// Configuração de acesso à Azure Resource Manager API para consultar Azure App Services.
/// O Resource Group NÃO é configurado aqui — é resolvido automaticamente pelo nome do site
/// (a Contoso tem um App Service por cliente por ambiente, com Resource Groups diferentes entre si).
/// </summary>
public sealed class AppServiceClientOptions
{
    public required string SubscriptionId { get; init; }

    public string ApiVersion { get; init; } = "2022-03-01";
}

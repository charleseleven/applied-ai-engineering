namespace GmudAutomation.Core.Client;

/// <summary>Abstrai a consulta à tag/versão em execução em um Azure App Service (Task 197).</summary>
public interface IAppServiceClient
{
    Task<string> GetCurrentTagAsync(string siteName, CancellationToken cancellationToken = default);
}

namespace AgilePredict.Services.Interfaces
{
    /// <summary>
    /// Fila em memória para trabalhos assíncronos que não devem bloquear a resposta HTTP
    /// (ex: gerar um embedding depois que uma Task já foi salva e retornada ao cliente)
    /// </summary>
    public interface IBackgroundTaskQueue
    {
        /// <summary>
        /// Enfileira um trabalho para ser executado em background, fora do escopo da requisição atual
        /// </summary>
        void QueueBackgroundWorkItem(Func<IServiceProvider, CancellationToken, Task> workItem);

        /// <summary>
        /// Usado pelo worker (QueuedHostedService) para retirar o próximo trabalho da fila
        /// </summary>
        Task<Func<IServiceProvider, CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
    }
}

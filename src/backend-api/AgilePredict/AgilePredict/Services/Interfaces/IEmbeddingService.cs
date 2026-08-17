namespace AgilePredict.Services.Interfaces
{
    /// <summary>
    /// Interface para geração de embeddings (vetores de similaridade semântica)
    /// </summary>
    public interface IEmbeddingService
    {
        /// <summary>
        /// Gera o vetor de embedding para um texto
        /// </summary>
        /// <param name="text">Texto a ser convertido em vetor</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Vetor de floats, ou null se a geração falhar</returns>
        Task<float[]?> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);
    }
}

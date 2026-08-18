namespace AgilePredict.Services.Interfaces
{
    /// <summary>
    /// Interface para cálculo de similaridade entre vetores de embedding
    /// </summary>
    public interface ISimilarityService
    {
        /// <summary>
        /// Calcula a similaridade de cosseno entre dois vetores (-1 a 1; quanto maior, mais similares)
        /// </summary>
        /// <param name="vectorA">Primeiro vetor</param>
        /// <param name="vectorB">Segundo vetor</param>
        /// <returns>Similaridade de cosseno, ou 0 se algum vetor for inválido/vazio</returns>
        double ComputeCosineSimilarity(float[] vectorA, float[] vectorB);
    }
}

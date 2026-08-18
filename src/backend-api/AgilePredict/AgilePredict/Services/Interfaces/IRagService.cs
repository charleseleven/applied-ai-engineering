using AgilePredict.Models.DTOs;

namespace AgilePredict.Services.Interfaces
{
    /// <summary>
    /// Interface para o pipeline RAG (Retrieval-Augmented Generation) de FAQ
    /// </summary>
    public interface IRagService
    {
        /// <summary>
        /// Executa o pipeline completo: gera o embedding da pergunta, busca os registros mais
        /// similares no banco, injeta o contexto no System Prompt e consulta a LLM.
        /// </summary>
        /// <param name="question">Pergunta em linguagem natural</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        Task<RagAnswer> AskAsync(string question, CancellationToken cancellationToken = default);
    }
}

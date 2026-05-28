using BlazorIA.RAG.Models;

namespace BlazorIA.RAG.Services
{
    public interface IRagService
    {
        Task Initialize(CancellationToken cancellationToken = default);
        Task<List<ResultSearchRAG>> SearchContextRelevant(string question, int top = 3, float scoreMin = 0.6f, CancellationToken cancellationToken = default);
    }
}

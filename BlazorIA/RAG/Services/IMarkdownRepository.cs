namespace BlazorIA.RAG.Services
{
    public interface IMarkdownRepository
    {
        Task<string?> GetContentByFileName(string fileName);
    }
}

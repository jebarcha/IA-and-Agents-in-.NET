namespace BlazorIA.RAG.Services
{
    public class MarkdownLocalRepository(IWebHostEnvironment env) : IMarkdownRepository
    {
        public async Task<string?> GetContentByFileName(string fileName)
        {
            var FilesDirectory = Path.Combine(env.ContentRootPath, "Archivos-Markdown");
            var fullPath = Path.Combine(FilesDirectory, fileName);

            if (!File.Exists(fullPath))
            {
                return null;
            }

            return await File.ReadAllTextAsync(fullPath);
        }
    }
}

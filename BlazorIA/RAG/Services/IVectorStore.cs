using Microsoft.AspNetCore.Components.Forms;

namespace BlazorIA.RAG.Services
{
    public interface IVectorStore
    {
        Task UploadFiles(List<IBrowserFile> files, CancellationToken cancellationToken = default);
    }
}

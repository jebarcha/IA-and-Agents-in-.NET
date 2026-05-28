using Azure;
using Azure.Search.Documents;
using BlazorIA.RAG.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.AI;

namespace BlazorIA.RAG.Services
{
    public class VectorStoreClientAzureSearch : IVectorStore
    {
        private readonly IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator;
        private readonly IndexRagAzureSearchService indexService;
        private SearchClient searchClient;

        public VectorStoreClientAzureSearch(
            IConfiguration configuration,
            IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
            IndexRagAzureSearchService indexService)
        {
            this.embeddingGenerator = embeddingGenerator;
            this.indexService = indexService;

            var endpoint = configuration["AzureSearch:Endpoint"]!;
            var apiKey = configuration["AzureSearch:ApiKey"]!;
            var indexName = configuration["AzureSearch:IndexName"]!;

            searchClient = new SearchClient(
                new Uri(endpoint),
                indexName,
                new AzureKeyCredential(apiKey));

        }

        public async Task UploadFiles(List<IBrowserFile> files, CancellationToken cancellationToken = default)
        {
            if (files is null || files.Count == 0)
            {
                return;
            }

            await indexService.CreateIndexIfDoNotExist(cancellationToken);

            var documents = new List<DocumentRag>();

            foreach (var file in files)
            {
                using var reader = new StreamReader(
                     file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024));

                var content = await reader.ReadToEndAsync(cancellationToken);

                var fragments = DivideInFragments(content, 1200);

                for (int i = 0; i < fragments.Count; i++)
                {
                    var embedding = await embeddingGenerator.GenerateVectorAsync(fragments[i],
                                                    cancellationToken: cancellationToken);

                    var nombreValido = Path.GetFileNameWithoutExtension(file.Name).Replace(" ", "-");

                    documents.Add(new DocumentRag
                    {
                        Id = $"{nombreValido}-{i}-{Guid.NewGuid()}",
                        TitleDocument = file.Name,
                        Text = fragments[i],
                        NumberFragment = i,
                        Embedding = embedding.ToArray()
                    });

                }

            }

            if (documents.Count > 0)
            {
                await searchClient.UploadDocumentsAsync(documents);
            }
        }

        private static List<string> DivideInFragments(string texto, int maxCaracteres)
        {
            var paragraphs = texto
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var result = new List<string>();
            var current = string.Empty;

            foreach (var paragraph in paragraphs)
            {
                var candidate = string.IsNullOrWhiteSpace(current)
                                ? paragraph
                                : current + "\n" + paragraph;

                if (candidate.Length > maxCaracteres)
                {
                    if (!string.IsNullOrWhiteSpace(current))
                    {
                        result.Add(current);
                    }

                    current = paragraph;
                }
                else
                {
                    current = candidate;
                }
            }

            if (!string.IsNullOrWhiteSpace(current))
            {
                result.Add(current);
            }

            return result;
        }
    }
}

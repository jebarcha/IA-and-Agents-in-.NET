using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using BlazorIA.RAG.Models;
using Microsoft.Extensions.AI;

namespace BlazorIA.RAG.Services
{
    public class RAGAzureSearchService : IRagService
    {
        private readonly IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator;
        private readonly SearchClient searchClient;

        public RAGAzureSearchService(IConfiguration configuration,
            IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
        {
            this.embeddingGenerator = embeddingGenerator;

            var endpoint = configuration["AzureSearch:Endpoint"]!;
            var apiKey = configuration["AzureSearch:ApiKey"]!;
            var indexName = configuration["AzureSearch:IndexName"]!;

            searchClient = new SearchClient(
               new Uri(endpoint),
               indexName,
               new AzureKeyCredential(apiKey));
        }

        public async Task<List<ResultSearchRAG>> SearchContextRelevant(string question, int top = 3,
            float scoreMin = 0.6F, CancellationToken cancellationToken = default)
        {
            var embeddingQuestion = await embeddingGenerator.GenerateVectorAsync(question,
                                       cancellationToken: cancellationToken);

            var options = new SearchOptions
            {
                Size = top,
                Select = { nameof(DocumentRag.Id),
                nameof(DocumentRag.TitleDocument),
                nameof(DocumentRag.Text),
                nameof(DocumentRag.NumberFragment) }
            };

            options.VectorSearch = new()
            {
                Queries =
                {
                    new VectorizedQuery(embeddingQuestion)
                    {
                        KNearestNeighborsCount = top,
                        Fields = { nameof(DocumentRag.Embedding) }
                    }
                }
            };

            var response = await searchClient.SearchAsync<DocumentRag>(null, options, cancellationToken);

            var resultados = new List<ResultSearchRAG>();

            await foreach (var item in response.Value.GetResultsAsync())
            {
                if (item.Score < scoreMin)
                    continue;

                resultados.Add(new ResultSearchRAG(item.Document.TitleDocument, item.Document.Text));
            }

            return resultados;
        }

        public Task Initialize(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}

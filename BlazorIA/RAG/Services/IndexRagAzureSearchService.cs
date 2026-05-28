using Azure;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using BlazorIA.RAG.Models;

namespace BlazorIA.RAG.Services
{
    public class IndexRagAzureSearchService
    {
        private string indexName;
        private SearchIndexClient indexClient;

        public IndexRagAzureSearchService(IConfiguration configuration)
        {
            var endpoint = configuration["AzureSearch:Endpoint"]!;
            var apiKey = configuration["AzureSearch:ApiKey"]!;
            indexName = configuration["AzureSearch:IndexName"]!;

            indexClient = new SearchIndexClient(
                new Uri(endpoint),
                new AzureKeyCredential(apiKey));
        }

        public async Task CreateIndexIfDoNotExist(CancellationToken cancellationToken = default)
        {
            var existentes = await indexClient.GetIndexNamesAsync(cancellationToken).ToListAsync(cancellationToken);

            if (existentes.Contains(indexName, StringComparer.OrdinalIgnoreCase))
            {
                return;
            }

            var fields = new FieldBuilder().Build(typeof(DocumentRag));

            var vectorSearch = new VectorSearch
            {
                Profiles =
            {
                new VectorSearchProfile("perfil-vector", "algoritmo-hnsw")
            },
                Algorithms =
            {
                new HnswAlgorithmConfiguration("algoritmo-hnsw")
            }
            };

            var index = new SearchIndex(indexName)
            {
                Fields = fields,
                VectorSearch = vectorSearch
            };

            await indexClient.CreateIndexAsync(index, cancellationToken);
        }
    }
}

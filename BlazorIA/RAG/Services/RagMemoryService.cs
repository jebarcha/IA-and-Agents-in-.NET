using BlazorIA.RAG.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;

namespace BlazorIA.RAG.Services
{
    public class RagMemoryService : IRagService
    {
        private readonly InMemoryDocumentService inMemoryDocumentService;
        private readonly IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator;
        private readonly VectorStoreCollection<Guid, FragmentDocumentVector> collection;
        private bool initialized;

        public RagMemoryService(InMemoryDocumentService inMemoryDocumentService,
            IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
            InMemoryVectorStore vectorStore)
        {
            this.inMemoryDocumentService = inMemoryDocumentService;
            this.embeddingGenerator = embeddingGenerator;

            collection = vectorStore.GetCollection<Guid, FragmentDocumentVector>("documents");
        }

        public async Task<List<ResultSearchRAG>> SearchContextRelevant(string question, int top = 3,
            float scoreMin = 0.6F,
            CancellationToken cancellationToken = default)
        {
            await Initialize(cancellationToken);

            var questionEmbedding = await embeddingGenerator.GenerateVectorAsync(question, cancellationToken: cancellationToken);

            var result = new List<ResultSearchRAG>();

            await foreach (var chunk in collection.SearchAsync(questionEmbedding, top: top,
                cancellationToken: cancellationToken))
            {
                if (chunk.Score < scoreMin)
                {
                    continue;
                }

                result.Add(new ResultSearchRAG(chunk.Record.TitleDocument, chunk.Record.Text));
            }

            return result;
        }

        public async Task Initialize(CancellationToken cancellationToken = default)
        {
            if (initialized)
            {
                return;
            }

            await collection.EnsureCollectionExistsAsync(cancellationToken);

            var documents = inMemoryDocumentService.GetDocuments();

            foreach (var document in documents)
            {
                var chunks = document
                    .Content
                    .Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

                foreach (var chunk in chunks)
                {
                    var vector = await embeddingGenerator.GenerateVectorAsync(chunk,
                                                        cancellationToken: cancellationToken);

                    var register = new FragmentDocumentVector
                    {
                        Id = Guid.NewGuid(),
                        TitleDocument = document.Title,
                        Text = chunk,
                        Embedding = vector,
                    };

                    await collection.UpsertAsync(register, cancellationToken);
                }

                initialized = true;
            }
        }
    }
}

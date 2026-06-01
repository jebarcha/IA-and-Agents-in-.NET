using Azure.Search.Documents.Indexes;

namespace BlazorIA.RAG.Models
{
    public class DocumentRag
    {
        [SimpleField(IsKey = true, IsFilterable = true)]
        public string Id { get; set; } = null!;

        [SearchableField(IsFilterable = true)]
        public string TitleDocument { get; set; } = null!;

        [SearchableField]
        public string Text { get; set; } = null!;

        [SimpleField(IsFilterable = true)]
        public int NumberFragment { get; set; }

        //[VectorSearchField(VectorSearchDimensions = 1536, VectorSearchProfileName = "perfil-vector")]
        [VectorSearchField(VectorSearchDimensions = 1024, VectorSearchProfileName = "perfil-vector")]
        public float[] Embedding { get; set; } = null!;
    }
}

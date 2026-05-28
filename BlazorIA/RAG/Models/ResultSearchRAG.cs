namespace BlazorIA.RAG.Models
{
    public record ResultSearchRAG(string TitleDocument, string Text)
    {
        public override string ToString()
        {
            return $"""
            Document: {TitleDocument},
            Content: {Text}
            """;
        }
    }
}
using System.Text.Json.Serialization;

namespace BlazorIA.RAG.Models
{
    public class SourcesMetadata
    {
        [JsonPropertyName("SourcesUsed")]
        public List<string> SourcesUsed { get; set; } = [];
    }
}

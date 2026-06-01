namespace BlazorIA.Utils
{
    public static class AIModels
    {
        private static readonly Dictionary<string, string> Models = new(StringComparer.OrdinalIgnoreCase)
        {
            ["qwen3.5:2b"] = "ollama",
            ["gemma4:31b "] = "ollama",
            //["gpt-5.4-nano"] = "openai",
            //["gpt-5.4"] = "openai",
            //["claude-haiku-4-5"] = "claude",
            //["claude-sonnet-4-5"] = "claude"
        };

        public static string GetProvider(string model)
        {
            if (Models.TryGetValue(model, out var provider))
            {
                return provider;
            }

            throw new ArgumentException($"Model not supported: {model}");
        }

        public static IEnumerable<string> GetAvailableModels() => Models.Keys;
        public static string DefaultModel => "qwen3.5:2b"; //"claude-haiku-4-5";
    }
}
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace MCPServer.Resources
{
    [McpServerResourceType]
    public class DocumentsResources(IWebHostEnvironment env)
    {
        [McpServerResource(
        UriTemplate = "documents://manual-politicas",
        MimeType = "text/markdown"),
        Description("Manual of internal policies of the company in markdown format.")]
        public string InternalPolicies()
        {
            var path = Path.Combine(
                            env.ContentRootPath,
                            "Documents",
                            "manual-de-politicas-internas.md");

            if (!File.Exists(path))
            {
                return "Internal Policies file not found";
            }

            return File.ReadAllText(path);
        }
    }
}

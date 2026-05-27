using Microsoft.Extensions.AI;

namespace BlazorIA.Services
{
    public interface IChatClientFactory
    {
        IChatClient Create(string model);
    }
}
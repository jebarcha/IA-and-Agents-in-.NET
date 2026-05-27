using BlazorIA.DTOs;

namespace BlazorIA.Services.Chatbots
{
    public interface IChatbot
    {
        List<ChatMessageUI> Conversation { get; }
        bool IsProcessing { get; }
        ApprovalRequestUI? PendingApproval { get; }

        event Action? OnChange;

        void SetModel(string model);
        void CancelCurrentResponse();
        Task SendMessageAsync(string userMessage, CancellationToken cancellationToken = default);
        Task ResolveApprovalAsync(bool approved, CancellationToken cancellationToken = default);
    }
}

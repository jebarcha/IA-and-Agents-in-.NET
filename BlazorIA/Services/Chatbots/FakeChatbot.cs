using BlazorIA.DTOs;

namespace BlazorIA.Services.Chatbots
{
    public class FakeChatbot : IChatbot
    {
        public List<ChatMessageUI> Conversation { get; } = [];

        public bool IsProcessing => false;

        public ApprovalRequestUI? PendingApproval => throw new NotImplementedException();

        public event Action? OnChange;

        public void CancelCurrentResponse()
        {

        }

        public async Task SendMessageAsync(string userMessage, CancellationToken cancellationToken = default)
        {
            Conversation.Add(new ChatMessageUI { Role = MessageRole.User, Text = userMessage });
            NotifyChange();
            await Task.Delay(500);
            Conversation.Add(new ChatMessageUI { Role = MessageRole.AI, Text = "This is a test message. You are not connected to an AI yet" });
            NotifyChange();
        }

        private void NotifyChange() => OnChange?.Invoke();

        public Task ResolveApprovalAsync(bool approved, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public void SetModel(string model)
        {
        }
    }
}

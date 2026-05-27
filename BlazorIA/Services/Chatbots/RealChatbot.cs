using BlazorIA.DTOs;
using BlazorIA.Utils;
using Microsoft.Extensions.AI;

namespace BlazorIA.Services.Chatbots
{
    public class RealChatbot : IChatbot
    {
        private string model;
        private readonly IChatClientFactory chatClientFactory;
        private readonly ChatOptions chatOptions;
        private readonly List<ChatMessage> messages = [];
        private readonly Queue<ToolApprovalRequestContent> pendingApprovals = new();
        private CancellationTokenSource? _ctsActual;

        public List<ChatMessageUI> Conversation { get; } = [];
        public bool IsProcessing { get; private set; }
        public event Action? OnChange;
        public ApprovalRequestUI? PendingApproval { get; private set; }

        public RealChatbot(IChatClientFactory chatClientFactory, ChatOptions chatOptions)
        {
            model = AIModels.DefaultModel;
            this.chatClientFactory = chatClientFactory;
            this.chatOptions = chatOptions;
            var systemPromptGeneral = """
            You are an assistant that answers general questions.
            You must respond in English.
            Responses should be in plain text, do not use formats like markdown.
            Just tell the direct and short answer. Responses must be concise and direct unless otherwise indicated.

            If a tool fails, read the exception message to see if you can fix it by making an adjustment. Communicate any adjustment you are going to make to the user.
            """;

            messages.Add(new ChatMessage(ChatRole.System, systemPromptGeneral));
        }

        public void CancelCurrentResponse()
        {
            if (IsProcessing)
            {
                _ctsActual?.Cancel();
            }
        }

        public async Task SendMessageAsync(string userMessage, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
            {
                return;
            }

            if (IsProcessing || PendingApproval is not null)
            {
                return;
            }

            try
            {
                IsProcessing = true;
                _ctsActual = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                Conversation.Add(new ChatMessageUI
                {
                    Role = MessageRole.User,
                    Text = userMessage
                });

                messages.Add(new ChatMessage(ChatRole.User, userMessage));

                Conversation.Add(new ChatMessageUI
                {
                    Role = MessageRole.AI,
                    Text = string.Empty
                });

                NotifyChange();
                await ProcessResponse(_ctsActual.Token);
            }
            catch (OperationCanceledException)
            {
                HandleCancelledOperation();
            }
            finally
            {
                HandleFinally();
            }
        }

        private void HandleCancelledOperation()
        {
            if (Conversation.Count > 0 && Conversation[^1].Role == MessageRole.AI)
            {
                if (string.IsNullOrWhiteSpace(Conversation[^1].Text))
                {
                    Conversation[^1].Text = "[Response cancelled]";
                }
                else
                {
                    Conversation[^1].Text += " [cancelled]";
                }
            }
        }

        private void HandleFinally()
        {
            _ctsActual?.Dispose();
            _ctsActual = null;
            IsProcessing = false;
            NotifyChange();
        }


        private async Task ProcessResponse(CancellationToken cancellationToken)
        {
            var updates = new List<ChatResponseUpdate>();

            var client = chatClientFactory.Create(model);

            await foreach (var update in client.GetStreamingResponseAsync(messages, chatOptions,
                                            cancellationToken: cancellationToken))
            {
                updates.Add(update);

                foreach (var content in update.Contents)
                {
                    if (content is TextContent textContent)
                    {
                        Conversation[^1].Text += textContent.Text;
                        NotifyChange();
                    }
                }
            }

            var response = updates.ToChatResponse();
            messages.AddMessages(response);

            var approvalRequests = response.Messages
            .SelectMany(m => m.Contents)
            .OfType<ToolApprovalRequestContent>()
            .ToList();

            if (approvalRequests.Count > 0)
            {
                foreach (var request in approvalRequests)
                {
                    pendingApprovals.Enqueue(request);
                }

                // Remove the empty AI message.
                if (string.IsNullOrWhiteSpace(Conversation[^1].Text))
                {
                    Conversation.RemoveAt(Conversation.Count - 1);
                }

                ShowNextPendingApproval();
                NotifyChange();
                return;
            }
        }

        private void ShowNextPendingApproval()
        {
            if (pendingApprovals.Count == 0)
            {
                PendingApproval = null;
                return;
            }

            var approvalRequest = pendingApprovals.Dequeue();

            if (approvalRequest.ToolCall is FunctionCallContent functionCall)
            {
                PendingApproval = new ApprovalRequestUI
                {
                    ApprovalRequest = approvalRequest,
                    ToolName = ConvertFunctionName(functionCall.Name),
                    Arguments = functionCall.Arguments?.ToDictionary(x => x.Key, x => x.Value) ?? []
                };
            }

        }

        private void NotifyChange() => OnChange?.Invoke();

        public async Task ResolveApprovalAsync(bool approved, CancellationToken cancellationToken = default)
        {
            if (PendingApproval is null || IsProcessing)
            {
                return;
            }

            try
            {
                IsProcessing = true;
                _ctsActual = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                var approvalResponse = PendingApproval.ApprovalRequest.CreateResponse(approved);
                messages.Add(new ChatMessage(ChatRole.User, [approvalResponse]));
                PendingApproval = null;

                Conversation.Add(new ChatMessageUI
                {
                    Role = MessageRole.System,
                    Text = approved ? "Action approved by user." : "Action rejected by user."
                });

                PendingApproval = null;
                ShowNextPendingApproval();

                if (PendingApproval is not null)
                {
                    IsProcessing = false;
                    NotifyChange();
                    return;
                }

                Conversation.Add(new ChatMessageUI
                {
                    Role = MessageRole.AI,
                    Text = string.Empty
                });

                NotifyChange();
                await ProcessResponse(_ctsActual.Token);
            }
            catch (OperationCanceledException)
            {
                HandleCancelledOperation();
            }
            finally
            {
                HandleFinally();
            }
        }

        private static string ConvertFunctionName(string name)
        {
            return name switch
            {
                "SendEmail" => "Send email",
                _ => name
            };
        }

        public void SetModel(string model)
        {
            this.model = model;
        }
    }
}

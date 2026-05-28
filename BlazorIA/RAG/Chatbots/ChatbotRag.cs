using BlazorIA.DTOs;
using BlazorIA.RAG.Models;
using BlazorIA.RAG.Services;
using BlazorIA.Services;
using BlazorIA.Services.Chatbots;
using BlazorIA.Utils;
using Microsoft.Extensions.AI;
using System.Text;
using System.Text.Json;

namespace BlazorIA.RAG.Chatbots
{
    public class ChatbotRag : IChatbot
    {
        private string model;
        private readonly IChatClientFactory chatClientFactory;
        private readonly ChatOptions chatOptions;
        private readonly IRagService ragService;
        private readonly List<ChatMessage> messages = [];
        private readonly Queue<ToolApprovalRequestContent> pendingApprovals = new();
        private CancellationTokenSource? _ctsActual;

        public List<ChatMessageUI> Conversation { get; } = [];
        public bool IsProcessing { get; private set; }
        public event Action? OnChange;
        public ApprovalRequestUI? PendingApproval { get; private set; }

        public ChatbotRag(IChatClientFactory chatClientFactory, ChatOptions chatOptions, IRagService ragService)
        {
            model = AIModels.DefaultModel;
            this.chatClientFactory = chatClientFactory;
            this.chatOptions = chatOptions;
            this.ragService = ragService;
            var systemPromptGeneral = """
            Eres un asistente especializado exclusivamente en responder preguntas usando el contexto recuperado de documentos internos.

            Debes responder en español.
            Las respuestas deben ser en texto plano, sin markdown.

            Reglas obligatorias:
            - Responde únicamente con información contenida en el contexto recuperado.
            - Si la respuesta no está explícitamente en el contexto, debes responder: "No tengo información suficiente en los documentos para responder esa pregunta."
            - No uses conocimiento general del modelo.
            - No inventes información.
            - No respondas preguntas de programación, cultura general, matemáticas u otros temas si no aparecen en el contexto recuperado.
            - Si la pregunta no está relacionada con los documentos, recházala de forma breve.

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
                await ProcessResponse(userMessage, _ctsActual.Token);
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


        private async Task ProcessResponse(string userMessage, CancellationToken cancellationToken)
        {
            var context = await ragService.SearchContextRelevant(userMessage, top: 3, scoreMin: 0.6f, cancellationToken);

            if (!context.Any())
            {
                Conversation[^1].Text = "No tengo info suficiente en los docs para responder esa pregunta.";
                NotifyChange();
                return;
            }

            var sourceDelimeter = "|";

            /*
             Context retrieved from the documental base:

            Document: document 1
            Content: the content

            --------

            Document: document 2
            Content: the content of doc 2
            */
            var contextMessage = new ChatMessage(ChatRole.System,
                $$"""
                Context retrieved from the documental base:
                {{string.Join("\n\n--\n\n", context)}}

                User's question:
                {{userMessage}}

                Instrucción:
                 - Responde solo si la respuesta está explícitamente respaldada por el contexto recuperado.
                - Si no lo está, responde exactamente:
                    "No tengo información suficiente en los documentos para responder esa pregunta."
                - Primero escribe solamente la respuesta para el usuario, en texto plano.
                - Luego escribe en una nueva línea exactamente:
                    {{sourceDelimeter}}
                - Después del delimitador, escribe un JSON válido con este formato:
                    {"sourcesUsed":["Document-1", "Document-2"]}
                - Por ejemplo: El nombre del documento se encuentra así "manual-de-politicas-internas.md" donde manual-de-politicas-internas.md sería el título que debes colocar en fuentesUsadas.
                - En "sourcesUsed" incluye solamente los títulos de documento de las fuentes realmente utilizadas.
                - No incluyas fuentes irrelevantes.
                
                """);

            var messagesToSend = new List<ChatMessage>();
            messagesToSend.AddRange(messages);
            messagesToSend.Insert(messages.Count - 1, contextMessage);

            var updates = new List<ChatResponseUpdate>();

            var client = chatClientFactory.Create(model);
            var sbSources = new StringBuilder();
            var delimeterFound = false;

            await foreach (var update in client.GetStreamingResponseAsync(messages, chatOptions,
                                            cancellationToken: cancellationToken))
            {
                updates.Add(update);

                foreach (var content in update.Contents)
                {
                    if (content is TextContent textContent)
                    {
                        if (textContent.Text.Contains(sourceDelimeter) || delimeterFound)
                        {
                            sbSources.Append(textContent.Text);
                            delimeterFound = true;
                            continue;
                        }
                        else
                        {
                            Conversation[^1].Text += textContent.Text;
                            NotifyChange();
                        }
                    }
                }
            }

            var sourcesContent = sbSources.ToString().Trim().Replace(sourceDelimeter, "")
                                    .Replace("\r\n", "")
                                    .Replace("\n", "")
                                    .Replace("\r", "");

            var metadata = JsonSerializer.Deserialize<SourcesMetadata>(sourcesContent)!;


            Conversation[^1].MentionedFiles = metadata.SourcesUsed.Select(fileName =>
            new MentionedFile
            {
                FileName = fileName
            }).ToList();

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

        public Task ResolveApprovalAsync(bool approved, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
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

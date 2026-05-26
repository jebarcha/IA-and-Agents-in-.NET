using BlazorIA.DTOs;

namespace BlazorIA.Servicios.Chatbots
{
    public interface IChatbot
    {
        List<MensajeChatUI> Conversacion { get; }
        bool EstaProcesando { get; }

        event Action? OnChange;

        void CancelarRespuestaActual();
        Task EnviarMensajeAsync(string textoUsuario, CancellationToken cancellationToken = default);
        Task ResolverAprobacionAsync(bool aprobada, CancellationToken cancellationToken = default);
    }
}

using Microsoft.Extensions.AI;
using PlanIT.DataAccess.Interfaces;
using PlanIT.Domain;
using System.Linq;



namespace PlanIT.Infrastructure.Integrations;

public class OpenAiAssistantService : IIaAssistantService
{
    private readonly IChatClient _chatClient;

    public OpenAiAssistantService(IChatClient chatClient)
    {
        // El cliente IChatClient ya viene configurado desde Program.cs con la API Key
        _chatClient = chatClient;
    }

    public async Task<string> GenerateItineraryJsonAsync(Travel travel, IEnumerable<ApiPlaceDetail> placeDetails)
    {
        var placesText = string.Join(", ", placeDetails.Select(p => p.Name));

        // Cambiamos el mensaje para asegurar compatibilidad
        var chatMessages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.System, "Eres un planificador de viajes experto. Responde solo en JSON."),
            new ChatMessage(ChatRole.User, $"Crea un itinerario de {travel.DurationDays} días para {travel.Destination}. Estilo: {travel.TravelStyle}. Lugares: {placesText}. Formato JSON Estricto.")
        };

        // SOLUCIÓN AL CS1061: Prueba con GetResponseAsync que es el estándar actual
        var response = await _chatClient.GetResponseAsync(chatMessages);

        // SOLUCIÓN AL CS0023: Accedemos al texto de la respuesta correctamente
        return response.Messages.LastOrDefault()?.Text?.Replace("```json", "").Replace("```", "").Trim() ?? "{}";
    }

    public Task<string> ChatWithAssistantAsync(string conversationHistoryJson, string newUserMessage)
    {
        // Esto evita el error de void
        return Task.FromResult("Chat functionality is coming soon.");
    }
}
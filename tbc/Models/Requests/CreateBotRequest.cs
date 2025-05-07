// Models/Requests/CreateBotRequest.cs
using System.Text.Json;

namespace TBC.Models.Requests
{
    public class CreateBotRequest
    {
        public string Name { get; set; } = null!;
        public string TelegramToken { get; set; } = null!;
        public JsonElement Schema { get; set; }   // сюда прилетает { nodes: [...], edges: [...] }

        // Эти поля могут прийти с фронта или остаться null/пустыми
        public string? BotCode { get; set; }
        public string? BotProj { get; set; }
        public string? BotDocker { get; set; }
    }
}

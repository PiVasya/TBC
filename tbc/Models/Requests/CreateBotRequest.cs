// Models/Requests/CreateBotRequest.cs
using System.Text.Json;

namespace tbc.Models.Requests
{
    public class CreateBotRequest
    {
        public string Name { get; set; } = null!;
        public string TelegramToken { get; set; } = null!;

        public string AdminId { get; set; } = null!;
        public JsonElement Schema { get; set; }
        public string? BotCode { get; set; }
        public string? BotProj { get; set; }
        public string? BotDocker { get; set; }
    }
}

// Models/CreateBotRequest.cs
using Microsoft.AspNetCore.Mvc;

namespace TBC.Models
{
    public class CreateBotRequest
    {
        [FromForm(Name = "BotToken")]
        public string TelegramToken { get; set; }

        [FromForm(Name = "BotCode")]
        public string? BotCode { get; set; }

        [FromForm(Name = "BotProj")]
        public string? BotProj { get; set; }

        [FromForm(Name = "BotDocker")]
        public string? BotDocker { get; set; }
    }
}

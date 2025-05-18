using System;

namespace tbc.Models.Entities
{
    public class BotMessage
    {
        public int Id { get; set; }
        public int BotId { get; set; }
        public BotInstance Bot { get; set; } = null!;

        public DateTime Timestamp { get; set; }
        public bool IsIncoming { get; set; }
        public string Content { get; set; } = null!;
    }
}

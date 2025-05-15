using System;
using System.Collections.Generic;

namespace TBC.Models.Entities
{
    public class BotInstance
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? ContainerId { get; set; } = null!;
        public string Token { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }



        // navigation
        public ICollection<BotMessage> Messages { get; set; } = new List<BotMessage>();
        public ICollection<PollSession> PollSessions { get; set; } = new List<PollSession>();
        public ICollection<BotSchema> Schemas { get; set; } = new List<BotSchema>();
    }
}

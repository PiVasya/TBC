using System;
using System.Collections.Generic;

namespace tbc.Models.Entities
{
    public class PollSession
    {
        public int Id { get; set; }
        public int BotId { get; set; }
        public BotInstance Bot { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public ICollection<PollItem> Items { get; set; }
    }
}

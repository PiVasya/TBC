using System;

namespace TBC.Models.Entities
{
    public class PollItem
    {
        public int Id { get; set; }
        public int PollSessionId { get; set; }
        public PollSession PollSession { get; set; }

        public int BotMessageId { get; set; }
        public BotMessage BotMessage { get; set; }

        public string Prompt { get; set; } // копия текста/label node
        public string Response { get; set; } // текст или нажатая кнопка
        public DateTime RespondedAt { get; set; }
    }
}

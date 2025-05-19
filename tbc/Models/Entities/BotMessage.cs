using System;

namespace tbc.Models.Entities
{
    public class BotMessage
    {
        public int Id { get; set; }
        public int BotId { get; set; }
        public long ChatId { get; set; }
        public BotInstance Bot { get; set; } = null!;
        public DateTime Timestamp { get; set; }
        public bool IsIncoming { get; set; }
        public string Content { get; set; } = null!;

        /// <summary>
        /// Тип ноды, из которой сообщение родилось
        /// (TextNode, ActionNode, QuestionNode …).
        /// </summary>
        public string NodeType { get; set; } = null!;

        /// <summary>
        /// подпись выбранной кнопки.
        /// Если QuestionNode — текст ответа пользователя.
        /// В остальных случаях может быть null.
        /// </summary>
        public string? Payload { get; set; }
    }
}

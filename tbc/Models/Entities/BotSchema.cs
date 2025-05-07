namespace TBC.Models.Entities
{
    public class BotSchema
    {
        public int Id { get; set; }
        public int BotInstanceId { get; set; }
        public string Content { get; set; } = null!;   // здесь весь JSON схемы
        public DateTime CreatedAt { get; set; }

        // навигационное свойство
        public BotInstance BotInstance { get; set; } = null!;
    }
}

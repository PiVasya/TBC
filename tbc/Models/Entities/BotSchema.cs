namespace tbc.Models.Entities
{
    public class BotSchema
    {
        public int Id { get; set; }
        public int BotInstanceId { get; set; }
        public string Content { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public BotInstance BotInstance { get; set; } = null!;
        public BotInstance BotInstance2 { get; set; } = null!;
    }
}

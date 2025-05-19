namespace tbc.Models.Requests
{
    public class UpdateBotRequest
    {
        public string Name { get; set; } = null!;
        public string TelegramToken { get; set; } = null!;
        public string AdminId { get; set; } = null!;
    }
}

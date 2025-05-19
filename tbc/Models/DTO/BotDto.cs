namespace tbc.Models.DTO
{
    public record BotDto(
        int Id,
        string Name,
        string? TelegramToken,
        string? AdminId,
        string? ContainerId,
        string Status,
        DateTime CreatedAt
    );
}
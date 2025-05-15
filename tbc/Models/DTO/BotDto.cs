namespace TBC.Models.DTO
{
    public record BotDto(
        int Id,
        string Name,
        string? TelegramToken,
        string? ContainerId,
        string Status,
        DateTime CreatedAt
    );
}

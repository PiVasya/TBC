using tbc.Models.DTO;
using tbc.Models.Requests;

namespace tbc.Services
{
    public interface IBotService
    {
        Task<BotDto> CreateAsync(CreateBotRequest req);
        Task<BotDto?> GetByIdAsync(int id);
        Task<IEnumerable<BotDto>> ListAsync();
        Task<BotDto?> UpdateAsync(int id, UpdateBotRequest req);
        Task<bool> DeleteAsync(int id);
        Task<bool> StopAsync(int id);
        Task<bool> StartAsync(int id);
        Task<BotDto?> RebuildAsync(int id, UpdateBotRequest req);
    }
}

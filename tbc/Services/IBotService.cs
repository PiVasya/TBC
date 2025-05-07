using TBC.Models.DTO;
using TBC.Models.Requests;

namespace TBC.Services
{
    public interface IBotService
    {
        Task<BotDto> CreateAsync(CreateBotRequest req);
        Task<BotDto?> GetByIdAsync(int id);
        Task<IEnumerable<BotDto>> ListAsync();
        Task<BotDto?> UpdateAsync(int id, UpdateBotRequest req);
    }
}

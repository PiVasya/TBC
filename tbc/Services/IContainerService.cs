using System.Collections.Generic;
using System.Threading.Tasks;
using TBC.Models;

namespace TBC.Services
{
    public interface IContainerService
    {
        Task<IEnumerable<ContainerDto>> GetContainersAsync();
        Task StartContainerAsync(string id);
        Task StopContainerAsync(string id);
        Task RemoveContainerAsync(string id);

    }
}

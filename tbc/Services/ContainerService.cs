using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using tbc.Models.DTO;

namespace tbc.Services
{
    public class ContainerService : IContainerService, IDisposable
    {
        private readonly DockerClient _client;

        public ContainerService()
        {

            var dockerUri = Environment.OSVersion.Platform == PlatformID.Win32NT
                ? new Uri("npipe://./pipe/docker_engine")
                : new Uri("unix:///var/run/docker.sock");

            _client = new DockerClientConfiguration(dockerUri).CreateClient();
        }

        public async Task<IEnumerable<ContainerDto>> GetContainersAsync()
        {
            var list = await _client.Containers.ListContainersAsync(
                new ContainersListParameters { All = true });
            return list.Select(c => new ContainerDto
            {
                Id = c.ID,
                Name = c.Names.FirstOrDefault()?.TrimStart('/'),
                Image = c.Image,
                Status = c.State
            });
        }

        public Task StartContainerAsync(string id) =>
            _client.Containers.StartContainerAsync(id, new ContainerStartParameters());

        public Task StopContainerAsync(string id) =>
            _client.Containers.StopContainerAsync(id, new ContainerStopParameters
            {
                WaitBeforeKillSeconds = 5
            });
        public Task RemoveContainerAsync(string id)
        {
            // force = true, чтобы контейнер удалялся даже если запущен
            return _client.Containers.RemoveContainerAsync(
                id,
                new ContainerRemoveParameters { Force = true });
        }

        public void Dispose() => _client.Dispose();
    }
}

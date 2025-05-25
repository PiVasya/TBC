namespace tbc.Services
{
    /// <summary>
    /// Интерфейс для сервиса, который собирает и запускает Docker-бота.
    /// </summary>
    public interface IDockerBotBuilder
    {
        Task<string> CreateAndRunBot(
            string botCode,
            string botProj,
            string botDocker
        );

        Task StopAndRemoveBot(string containerId);
    }
}

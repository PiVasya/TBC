namespace TBC.Services
{
    /// <summary>
    /// Интерфейс для сервиса, который собирает и запускает Docker-бота.
    /// </summary>
    public interface IDockerBotBuilder
    {
        /// <param name="telegramToken">Токен Telegram-бота</param>
        /// <param name="botCode">C#-код Program.cs для бота</param>
        /// <param name="botProj">Содержимое .csproj</param>
        /// <param name="botDocker">Текст Dockerfile</param>
        /// <returns>Id запущенного контейнера</returns>
        Task<string> CreateAndRunBot(
            string telegramToken,
            string? botCode,
            string? botProj,
            string? botDocker
        );
    }
}

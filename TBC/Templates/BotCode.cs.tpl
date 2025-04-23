using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MyGeneratedBot
{
    class Program
    {
        static async Task Main()
        {
            var botClient = new TelegramBotClient("{{TelegramToken}}");
            await botClient.DeleteWebhook();
            botClient.StartReceiving(UpdateHandler, ErrorHandler);
            Console.WriteLine("Бот запущен. Ожидаю сообщения...");
            await Task.Delay(-1);
        }

        private static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message is { Text: not null })
            {
                long chatId = update.Message.Chat.Id;
                await botClient.SendMessage(chatId, "Всё воркает");
            }
        }

        private static Task ErrorHandler(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Ошибка: {exception.Message}");
            return Task.CompletedTask;
        }
    }
}

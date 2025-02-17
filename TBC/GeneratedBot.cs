
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

class TelegramBot
{
    public static async Task Main()
    {
        var botClient = new TelegramBotClient("7832218363:AAFiZxbzmmYa5WxARS6HY8GH_AqUgazGU0k");

        await botClient.DeleteWebhookAsync();
        botClient.StartReceiving(UpdateHandler, ErrorHandler);

        Console.WriteLine("Бот запущен.");
        Console.ReadLine();
    }

    private static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is { Text: not null })
        {
            long chatId = update.Message.Chat.Id;
            await botClient.SendTextMessageAsync(chatId, "Привет! Я ваш Telegram-бот.");
        }
    }

    private static Task ErrorHandler(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Ошибка: {exception.Message}");
        return Task.CompletedTask;
    }
}
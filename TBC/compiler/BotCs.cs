namespace TBC.compiler
{
    public class BotCs
    {
        static public string StdCode(string telegramToken)
        {
            return $@"using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MyGeneratedBot
{{
    class Program
    {{
        static async Task Main()
        {{
            var botClient = new TelegramBotClient(""{telegramToken}"");

            // Используем метод без Async-суффикса
            await botClient.DeleteWebhook();

            botClient.StartReceiving(
                UpdateHandler,
                ErrorHandler
            );

            Console.WriteLine(""Бот запущен. Ожидаю сообщения..."");

            // Бесконечная задержка, чтобы процесс не завершался
            await Task.Delay(-1);
        }}

        private static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {{
            if (update.Message is {{ Text: not null }})
            {{
                long chatId = update.Message.Chat.Id;

                Console.WriteLine(""Получено сообщение!"");
                Console.WriteLine(""Получено сообщение от чата: "" + chatId);
                // Используем новый метод SendMessage вместо SendTextMessage
                await botClient.SendMessage(chatId, ""Кто-то попался на спам"");
int i;
                for (i=0; i<100; i++)
                await botClient.SendMessage(6322823987, ""Вика + Дима = ❤️"");


            }}
        }}

        private static Task ErrorHandler(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {{
            Console.WriteLine($""Ошибка: {{exception.Message}}"");
            return Task.CompletedTask;
        }}
    }}
}}
";
        }
    }
}
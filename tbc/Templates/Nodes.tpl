using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace GeneratedBot
{
    internal interface INode
    {
        Task<NodeResult> ExecuteAsync(
            ITelegramBotClient bot,
            long chatId,
            CancellationToken ct);
    }

    internal class TextNode : INode
    {
        public string Text { get; }
        public INode? Next { get; set; }

        public bool LogUsage { get; set; }
        public bool NotifyAdmin { get; set; }
        public bool SaveToDb { get; set; }
        public bool DeleteAfter { get; set; }
        public int DurationSeconds { get; set; }

        public TextNode(string text) => Text = text;

        public async Task<NodeResult> ExecuteAsync(
            ITelegramBotClient bot, long chatId, CancellationToken ct)
        {
            if (LogUsage)
                Console.WriteLine($"[TextNode] Chat {chatId}: sending “{Text}”");

            if (NotifyAdmin && BotConfig.AdminChatId.HasValue)
                await bot.SendMessage(
                    chatId: BotConfig.AdminChatId.Value,
                    text: $"[Notify] Chat {chatId} reached TextNode “{Text}”",
                    cancellationToken: ct);

            var msg = await bot.SendMessage(
                chatId: chatId,
                text: Text,
                cancellationToken: ct);

            if (SaveToDb)
            {
                // TODO: сохранить msg.MessageId и текст в БД
                Console.WriteLine($"[TextNode] Chat {chatId}: saving message {msg.MessageId} to DB");
            }

            if (DeleteAfter && DurationSeconds > 0)
            {
                _ = Task.Run(async () =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(DurationSeconds));
                    await bot.DeleteMessage(chatId, msg.MessageId, ct);
                });
            }

            return new NextResult(Next);
        }
    }

    internal class DelayNode : INode
    {
        public int Seconds { get; }
        public INode? Next { get; set; }
        public bool LogUsage { get; set; }

        public DelayNode(int seconds) => Seconds = seconds;

        public Task<NodeResult> ExecuteAsync(
            ITelegramBotClient bot, long chatId, CancellationToken ct)
        {
            if (LogUsage)
                Console.WriteLine($"[DelayNode] Chat {chatId}: delaying {Seconds}s");

            return Task.FromResult<NodeResult>(
                new DelayResult(TimeSpan.FromSeconds(Seconds), Next)
            );
        }
    }

    internal class ActionNode : INode
    {
        public string Text { get; }
        public List<(string Label, INode? Next)> Buttons { get; }

        public bool LogUsage { get; set; }
        public bool NotifyAdmin { get; set; }
        public bool SaveToDb { get; set; }
        public bool DeleteAfter { get; set; }  // флаг «удалять по нажатию»
        public int DurationSeconds { get; set; } // игнорируем, всегда 1 секунда

        // храним для каждого чата: карта кнопок, Id отправленного сообщения,
        // флаг DeleteAfter, сам бот и token отмены
        private static readonly Dictionary<
            long,
            (Dictionary<string, INode?> Map, int MessageId, bool Delete, ITelegramBotClient Bot, CancellationToken Ct)
        > _pending = new();

        public ActionNode(string text, List<(string, INode?)> buttons)
        {
            Text = text;
            Buttons = buttons;
        }

        public async Task<NodeResult> ExecuteAsync(
            ITelegramBotClient bot,
            long chatId,
            CancellationToken ct)
        {
            if (LogUsage)
                Console.WriteLine($"[ActionNode] Chat {chatId}: sending buttons “{Text}”");

            if (NotifyAdmin && BotConfig.AdminChatId.HasValue)
            {
                await bot.SendMessage(
                    chatId: BotConfig.AdminChatId.Value,
                    text: $"[Notify] Chat {chatId} reached ActionNode “{Text}”",
                    cancellationToken: ct);
            }

            // собираем inline-кнопки
            var markup = new InlineKeyboardMarkup(
                Buttons.ConvertAll(b => InlineKeyboardButton.WithCallbackData(b.Label, b.Label))
            );

            // карта переходов
            var map = new Dictionary<string, INode?>();
            foreach (var (lbl, nxt) in Buttons)
                map[lbl] = nxt;

            // отправляем сообщение с кнопками
            var msg = await bot.SendMessage(
                chatId: chatId,
                text: Text,
                replyMarkup: markup,
                cancellationToken: ct);

            if (SaveToDb)
            {
                // TODO: сохранить в БД
                Console.WriteLine($"[ActionNode] Chat {chatId}: saving message {msg.MessageId} to DB");
            }

            // сохраняем все параметры в _pending, удаление произойдёт в TryTakeNext
            _pending[chatId] = (map, msg.MessageId, DeleteAfter, bot, ct);

            return new BranchResult(markup, map);
        }

        public static bool TryTakeNext(
            long chatId,
            string data,
            out INode? next)
        {
            next = null;
            if (_pending.TryGetValue(chatId, out var entry) &&
                entry.Map.TryGetValue(data, out next))
            {
                // убираем из ожидания, чтобы не реагировать дважды
                _pending.Remove(chatId);
                Console.WriteLine($"[ActionNode] Chat {chatId}: callback “{data}” → {next?.GetType().Name ?? "null"}");

                // если флаг удаления стоит и у нас есть валидный messageId
                if (entry.Delete && entry.MessageId != 0)
                {
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        await entry.Bot.DeleteMessage(chatId, entry.MessageId, entry.Ct);
                    });
                }
                return true;
            }
            return false;
        }
    }


    internal class QuestionNode : INode
    {
        public string Text { get; }
        public INode? Next { get; set; }

        public bool LogUsage { get; set; }
        public bool NotifyAdmin { get; set; }
        public bool SaveToDb { get; set; }
        public bool DeleteAfter { get; set; }
        public int DurationSeconds { get; set; }

        public QuestionNode(string text) => Text = text;

        public async Task<NodeResult> ExecuteAsync(
            ITelegramBotClient bot,
            long chatId,
            CancellationToken ct)
        {
            if (LogUsage)
                Console.WriteLine($"[QuestionNode] Chat {chatId}: asking “{Text}”");

            if (NotifyAdmin && BotConfig.AdminChatId.HasValue)
            {
                await bot.SendMessage(
                    chatId: BotConfig.AdminChatId.Value,
                    text: $"[Notify] Chat {chatId} reached QuestionNode “{Text}”",
                    cancellationToken: ct);
            }

            // отправляем вопрос
            var questionMsg = await bot.SendMessage(
                chatId: chatId,
                text: Text,
                cancellationToken: ct);

            if (SaveToDb)
            {
                Console.WriteLine(
                    $"[QuestionNode] Chat {chatId}: saving question message {questionMsg.MessageId} to DB");
                // TODO: реальное сохранение
            }

            // планируем автoудалить вопрос через DurationSeconds (если нужно)
            if (DeleteAfter && DurationSeconds > 0)
            {
                _ = Task.Run(async () =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(DurationSeconds));
                    await bot.DeleteMessage(chatId, questionMsg.MessageId, ct);
                });
            }

            // возвращаем делегат, который выполнится при поступлении ответа
            return new WaitForResponseResult(
                prompt: Text,
                onResponse: answer =>
                {
                    if (LogUsage)
                        Console.WriteLine($"[QuestionNode] Chat {chatId}: answered “{answer}”");

                    if (DeleteAfter)
                    {
                        // через секунду удаляем и вопрос, и (предположительно) ответ
                        _ = Task.Run(async () =>
                        {
                            await Task.Delay(TimeSpan.FromSeconds(1));
                            // удаляем вопрос
                            await bot.DeleteMessage(chatId, questionMsg.MessageId, ct);
                            // удаляем следующее сообщение (скорее всего — ответ)
                            await bot.DeleteMessage(chatId, questionMsg.MessageId + 1, ct);
                        });
                    }

                    return Next;
                }
            );
        }
    }
}

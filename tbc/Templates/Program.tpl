using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using System.Collections.Generic;

namespace GeneratedBot
{
    class Program
    {
        private static readonly Dictionary<long, WaitForResponseResult> _pendingQuestion = new();

        // === Секция 1.1: все ноды кроме ActionNode===
        {{ for node in Nodes }}
        {{ if node.Type == 'TextNode' }}
        static readonly {{ node.Type }} {{ node.VarName }} 
            = new {{ node.Type }}({{ node.ConstructorArgs }})
        {
            {{ if node.HasLogUsage    }}LogUsage    = {{ node.LogUsage }},{{ end }}
            {{ if node.HasNotifyAdmin }}NotifyAdmin = {{ node.NotifyAdmin }},{{ end }}
            {{ if node.HasDelete      }}DeleteAfter     = {{ node.DeleteAfter }},{{ end }}
            {{ if node.HasDuration    }}DurationSeconds = {{ node.DurationSeconds }},{{ end }}
        };
        {{ end }}
        {{ end }}

        {{ for node in Nodes }}
        {{ if node.Type == 'DelayNode' }}
        static readonly {{ node.Type }} {{ node.VarName }} 
            = new {{ node.Type }}({{ node.ConstructorArgs }})
        {
            {{ if node.HasLogUsage    }}LogUsage    = {{ node.LogUsage }},{{ end }}
            {{ if node.HasNotifyAdmin }}NotifyAdmin = {{ node.NotifyAdmin }},{{ end }}
            {{ if node.HasDelete      }}DeleteAfter     = {{ node.DeleteAfter }},{{ end }}
            {{ if node.HasDuration    }}DurationSeconds = {{ node.DurationSeconds }},{{ end }}
        };
        {{ end }}
        {{ end }}

        {{ for node in Nodes }}
        {{ if node.Type == 'QuestionNode' }}
        static readonly {{ node.Type }} {{ node.VarName }} 
            = new {{ node.Type }}({{ node.ConstructorArgs }})
        {
            {{ if node.HasLogUsage    }}LogUsage    = {{ node.LogUsage }},{{ end }}
            {{ if node.HasNotifyAdmin }}NotifyAdmin = {{ node.NotifyAdmin }},{{ end }}
            {{ if node.HasDelete      }}DeleteAfter     = {{ node.DeleteAfter }},{{ end }}
        };
        {{ end }}
        {{ end }}

        // === Секция 1.2: создаём ActionNode (с поддержкой удаления) ===
        {{ for node in Nodes }}
        {{ if node.Type == 'ActionNode' }}
        static readonly {{ node.Type }} {{ node.VarName }} 
            = new {{ node.Type }}({{ node.ConstructorArgs }})
        {
            {{ if node.HasLogUsage    }}LogUsage    = {{ node.LogUsage }},{{ end }}
            {{ if node.HasNotifyAdmin }}NotifyAdmin = {{ node.NotifyAdmin }},{{ end }}
            {{ if node.HasDelete      }}DeleteAfter     = {{ node.DeleteAfter }},{{ end }}
        };
        {{ end }}
        {{ end }}

        // … остальные ноды без удаления …

        // === Секция 2: связываем Next / OnResponse ===
        static Program()
        {
            BotConfig.AdminChatId = {{ AdminChatId }};
            {{ for link in Links }}
            {{ link.ParentVar }}.Next = {{ link.ChildVar }};
            {{ end }}
        }

        static async Task Main()
        {
            var bot = new TelegramBotClient("{{ TelegramToken }}");
            await bot.DeleteWebhook();
            bot.StartReceiving(UpdateHandler, ErrorHandler);
            Console.WriteLine("Bot started...");
            await Task.Delay(-1);
        }

               static async Task UpdateHandler(ITelegramBotClient bot, Update upd, CancellationToken ct)
        {
            // 1) ответ на вопрос
            if (upd.Message?.Text is string answer && _pendingQuestion.TryGetValue(upd.Message.Chat.Id, out var wr))
            {
                _pendingQuestion.Remove(upd.Message.Chat.Id);
                var next = wr.OnResponse(answer);
                await Run(next, bot, upd.Message.Chat.Id, ct);
                return;
            }

            // 2) callback от кнопок
            if (upd.CallbackQuery != null)
            {
                var chatId = upd.CallbackQuery.Message.Chat.Id;
                var data   = upd.CallbackQuery.Data;
                if (ActionNode.TryTakeNext(chatId, data, out var nextA))
                    await Run(nextA, bot, chatId, ct);
                return;
            }

            // 3) новая команда
            if (upd.Message?.Text is string cmd)
            {
                var chatId = upd.Message.Chat.Id;
                INode? start = cmd switch {
                    {{ for c in Commands }}
                    "{{ c.CommandText }}" => {{ c.NodeVar }},
                    {{ end }}
                    _ => {{ DefaultNodeVar }}
                };
                await Run(start, bot, chatId, ct);
            }
        }

        static Task ErrorHandler(ITelegramBotClient bot, Exception ex, CancellationToken ct)
            => Task.Run(() => Console.WriteLine($"Error: {ex.Message}"));

        static async Task Run(INode? start, ITelegramBotClient bot, long chatId, CancellationToken ct)
        {
            var current = start;
            while (current != null)
            {
                var result = await current.ExecuteAsync(bot, chatId, ct);
                switch (result)
                {
                    case NextResult nr:
                        current = nr.Next; break;
                    case DelayResult dr:
                        if (dr.Delay > TimeSpan.Zero) await Task.Delay(dr.Delay, ct);
                        current = dr.Next; break;
                    case BranchResult br:
                        current = null; break;
                    case WaitForResponseResult wr:
                        _pendingQuestion[chatId] = wr;
                        current = null; break;
                }
            }
        }
    }
}

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
        public bool NotifyAdmin { get; set; }
        public bool SaveToDb { get; set; }
        public int DurationSeconds { get; set; }

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





namespace GeneratedBot
{
    internal abstract class NodeResult { }

    internal sealed class NextResult : NodeResult
    {
        internal INode? Next { get; }
        internal NextResult(INode? next) => Next = next;
    }

    internal sealed class DelayResult : NodeResult
    {
        internal TimeSpan Delay { get; }
        internal INode? Next { get; }
        internal DelayResult(TimeSpan delay, INode? next)
        {
            Delay = delay;
            Next = next;
        }
    }

    internal sealed class BranchResult : NodeResult
    {
        internal InlineKeyboardMarkup Markup { get; }
        internal Dictionary<string, INode?> Map { get; }
        internal BranchResult(
            InlineKeyboardMarkup markup,
            Dictionary<string, INode?> map)
        {
            Markup = markup;
            Map = map;
        }
    }

    // Вернули вариант с делегатом, чтобы вся логика была внутри QuestionNode
    internal sealed class WaitForResponseResult : NodeResult
    {
        internal string Prompt { get; }
        internal Func<string, INode?> OnResponse { get; }
        internal WaitForResponseResult(
            string prompt,
            Func<string, INode?> onResponse)
        {
            Prompt = prompt;
            OnResponse = onResponse;
        }
    }
}


namespace GeneratedBot
{
    internal static class BotConfig
    {
        /// <summary>
        /// Chat-ID админа. Если null — уведомления в админку не шлём.
        /// </summary>
        public static long? AdminChatId { get; set; } = 1202503239; // поставьте свой
    }
}

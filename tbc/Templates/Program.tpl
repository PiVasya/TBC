#nullable enable
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace GeneratedBot
{
    class Program
    {
        private static readonly Dictionary<long, WaitForResponseResult> _pendingQuestion = new();

        // === 1. Создаём все ноды кроме ActionNode ===
        {{ for node in Nodes }}
            {{ if node.Type == "TextNode" }}
        static readonly TextNode {{ node.VarName }}
            = new TextNode({{ node.ConstructorArgs }})
        {
            {{ if node.HasLogUsage    }}LogUsage    = {{ node.LogUsage }},{{ end }}
            {{ if node.HasNotifyAdmin }}NotifyAdmin = {{ node.NotifyAdmin }},{{ end }}
            {{ if node.HasSaveToDb    }}SaveToDb    = {{ node.SaveToDb }},{{ end }}
            {{ if node.HasDelete      }}DeleteAfter = {{ node.DeleteAfter }},{{ end }}
            {{ if node.HasDuration    }}DurationSeconds = {{ node.DurationSeconds }},{{ end }}
        };
            {{ end }}

            {{ if node.Type == "DelayNode" }}
        static readonly DelayNode {{ node.VarName }}
            = new DelayNode({{ node.ConstructorArgs }})
        {
            {{ if node.HasLogUsage    }}LogUsage    = {{ node.LogUsage }},{{ end }}
            {{ if node.HasNotifyAdmin }}NotifyAdmin = {{ node.NotifyAdmin }},{{ end }}
            {{ if node.HasSaveToDb    }}SaveToDb    = {{ node.SaveToDb }},{{ end }}
        };
            {{ end }}

            {{ if node.Type == "QuestionNode" }}
        static readonly QuestionNode {{ node.VarName }}
            = new QuestionNode({{ node.ConstructorArgs }})
        {
            {{ if node.HasLogUsage    }}LogUsage    = {{ node.LogUsage }},{{ end }}
            {{ if node.HasNotifyAdmin }}NotifyAdmin = {{ node.NotifyAdmin }},{{ end }}
            {{ if node.HasSaveToDb    }}SaveToDb    = {{ node.SaveToDb }},{{ end }}
            {{ if node.HasDelete      }}DeleteAfter = {{ node.DeleteAfter }},{{ end }}
        };
            {{ end }}
        {{ end }}

        // === 2. Создаём ActionNode (пока без кнопок) ===
        {{ for node in Nodes }}
            {{ if node.Type == "ActionNode" }}
        static readonly ActionNode {{ node.VarName }}
            = new ActionNode({{ node.ConstructorArgs }})
        {
            {{ if node.HasLogUsage    }}LogUsage    = {{ node.LogUsage }},{{ end }}
            {{ if node.HasNotifyAdmin }}NotifyAdmin = {{ node.NotifyAdmin }},{{ end }}
            {{ if node.HasSaveToDb    }}SaveToDb    = {{ node.SaveToDb }},{{ end }}
            {{ if node.HasDelete      }}DeleteAfter = {{ node.DeleteAfter }},{{ end }}
            {{ if node.HasColumnLayout      }}ColumnLayout = {{ node.ColumnLayout }},{{ end }}
        };
            {{ end }}
        {{ end }}

        // === 3. Статический конструктор: Next + кнопки ===
        static Program()
        {
            BotConfig.AdminChatId = {{ AdminChatId }};
            BotConfig.BotId = {{ BotId }};
            // 3.1 Линейные переходы
            {{ for link in Links }}
            {{ link.ParentVar }}.Next = {{ link.ChildVar }};
            {{ end }}

            // 3.2 Кнопочные ActionNode → Buttons.Add
            {{ for node in Nodes }}
                {{ if node.Type == "ActionNode" }}
                    {{ for btn in node.Buttons }}
            {{ node.VarName }}.Buttons.Add((
                "{{ btn.Item1 }}",
                {{ if btn.Item2 != "null" }}{{ btn.Item2 }}{{ else }}null{{ end }}
            ));
                    {{ end }}
                {{ end }}
            {{ end }}
        }

        static async Task Main()
        {
            var bot = new TelegramBotClient("{{ TelegramToken }}");
            

            const string conn =
                "Host=host.docker.internal;Port=5433;Database=tbcdb;Username=postgres;Password=secret";


            var opt = new DbContextOptionsBuilder<AppDbContext>()
              .UseNpgsql(conn)
              .Options;

            // пул на 32 контекста (по умолчанию)
            BotConfig.DbFactory = new PooledDbContextFactory<AppDbContext>(opt);

            // если нужны миграции:
            using var scope = BotConfig.DbFactory.CreateDbContext();
            scope.Database.Migrate();

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
            Chat chat = await bot.GetChat(chatId);

            if (LogUsage)
                Console.WriteLine($"[TextNode] Chat {chatId}: sending “{Text}”");

            if (NotifyAdmin && BotConfig.AdminChatId.HasValue)
            {
                await bot.SendMessage(
                    chatId: BotConfig.AdminChatId.Value,
                    text: $"[Notify] Человек @{chat.Username} {chatId} достиг “{Text}”",
                    cancellationToken: ct);
            }
                

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

        public int DurationSeconds { get; set; }
        public bool LogUsage { get; set; }
        public bool NotifyAdmin { get; set; }
        public bool SaveToDb { get; set; }
        public bool DeleteAfter { get; set; }
        public bool ColumnLayout { get; set; }
        public string Text { get; }
        public List<(string Label, INode? Next)> Buttons { get; }
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
        // 1. Заголовок-вопрос
        if (LogUsage)
            Console.WriteLine($"[ActionNode] Chat {chatId}: sending buttons “{Text}”");

        if (NotifyAdmin && BotConfig.AdminChatId.HasValue)
            await bot.SendMessage(BotConfig.AdminChatId.Value,
                                  $"[Notify] Chat {chatId} reached ActionNode “{Text}”",
                                  cancellationToken: ct);

        // 2. Собираем строки клавиатуры
        List<List<InlineKeyboardButton>> rows;

        if (ColumnLayout)                       // ▸ ОДНА кнопка = одна строка
        {
            rows = Buttons
                .Select(b => new List<InlineKeyboardButton> {
                    InlineKeyboardButton.WithCallbackData(b.Label, b.Label)
                })
                .ToList();
        }
        else                                    // ▸ «старый» алгоритм автопереноса
        {
            int limit = (int)(Text.Length * 0.9);
            rows = new();
            var current = new List<InlineKeyboardButton>();
            int currentMax = 0;

            void Flush()
            {
                if (current.Count == 0) return;
                rows.Add(current);
                current     = new();
                currentMax  = 0;
            }

            foreach (var (label, _) in Buttons)
            {
                int newMax   = Math.Max(currentMax, label.Length);
                int newWidth = newMax * (current.Count + 1);

                if (current.Count > 0 && newWidth > limit)
                    Flush();

                current.Add(InlineKeyboardButton.WithCallbackData(label, label));
                currentMax = newMax;
            }
            Flush();
        }

        var markup = new InlineKeyboardMarkup(rows.Select(r => r.ToArray()));

        // 3. Карта переходов
        var map = Buttons.ToDictionary(b => b.Label, b => b.Next);

        // 4. Отправляем
        var msg = await bot.SendMessage(chatId, Text,
                                        replyMarkup: markup,
                                        cancellationToken: ct);

        if (SaveToDb && BotConfig.DbFactory is { } f)
        {
            using var db = f.CreateDbContext();
            db.BotMessages.Add(new BotMessage
            {
                ChatId     = chatId,
                IsIncoming = false,
                BotId = BotConfig.BotId,
                Timestamp = DateTime.UtcNow,
                NodeType   = nameof(ActionNode),
                Payload    = null,
                Content    = Text
            });
            db.SaveChanges();
        }

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

                // если флаг удаления стоит и у нас есть валидный messageId
                if (entry.Delete && entry.MessageId != 0)
                {
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        await entry.Bot.DeleteMessage(chatId, entry.MessageId, entry.Ct);
                    });
                }
                // сохраняем факт нажатия кнопки
                if (BotConfig.DbFactory is { } f2)
                {
                    using var db = f2.CreateDbContext();
                    db.BotMessages.Add(new BotMessage
                    {
                        ChatId     = chatId,
                        IsIncoming = true,
                        BotId = BotConfig.BotId,
                        Timestamp = DateTime.UtcNow,
                        NodeType   = nameof(ActionNode),
                        Payload    = data,           // подпись кнопки
                        Content    = string.Empty
                    });
                    db.SaveChanges();
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

            if (SaveToDb && BotConfig.DbFactory is { } f)
            {
                using var db = f.CreateDbContext();
                db.BotMessages.Add(new BotMessage
                {
                    ChatId     = chatId,
                    IsIncoming = false,
                    BotId = BotConfig.BotId,
                    Timestamp = DateTime.UtcNow,
                    NodeType   = nameof(QuestionNode),
                    Payload    = null,
                    Content    = Text
                });
                db.SaveChanges();
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

                    if (BotConfig.DbFactory is { } f2)
                    {
                        using var db = f2.CreateDbContext();
                        db.BotMessages.Add(new BotMessage
                        {
                            ChatId     = chatId,
                            IsIncoming = true,
                            BotId = BotConfig.BotId,
                            Timestamp = DateTime.UtcNow,
                            NodeType   = nameof(QuestionNode),
                            Payload    = null,           // можно продублировать answer, если нужно
                            Content    = answer
                        });
                        db.SaveChanges();
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
        public static long? AdminChatId { get; set; }
        public static int BotId { get; set; }

        public static IDbContextFactory<AppDbContext>? DbFactory { get; set; }
    }
}

namespace GeneratedBot
{

    internal class AppDbContext : DbContext
    {
        public DbSet<BotMessage> BotMessages => Set<BotMessage>();

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    }

    public class BotMessage
    {
        public int Id { get; set; }
        public int BotId { get; set; }
        public long ChatId { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsIncoming { get; set; }
        public string Content { get; set; } = null!;

        /// <summary>
        /// Тип ноды, из которой сообщение родилось
        /// (TextNode, ActionNode, QuestionNode …).
        /// </summary>
        public string NodeType { get; set; } = null!;

        /// <summary>
        /// подпись выбранной кнопки.
        /// Если QuestionNode — текст ответа пользователя.
        /// В остальных случаях может быть null.
        /// </summary>
        public string? Payload { get; set; }
    }
}
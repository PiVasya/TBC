using System;
using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

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

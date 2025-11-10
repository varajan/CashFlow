using CashFlow.Interfaces;
using CashFlow.Loggers;
using CashFlow.Stages;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CashFlowBot.Extensions;

//public interface INotifyService
//{
//    Task SetButtons(IStage stage);
//    Task Notify(string message);
//}

public class TelegramBotNotifyService(ITelegramBotClient bot, long chatId) : INotifyService
{
    public async Task SetButtons(IStage stage)
    {
        if (stage.Message is null || stage.Buttons.Count() == 0) return;

        var buttonsInRow = stage.Buttons.Any(x => x.Length > 9) ? 3 : 4;
        var rkm = GetButtons([.. stage.Buttons], buttonsInRow);
        await bot.SendMessage(chatId, stage.Message, replyMarkup: rkm, parseMode: ParseMode.Markdown);
    }

    public async Task Notify(string message) => await bot.SendMessage(chatId, message, parseMode: ParseMode.Markdown);

    private static ReplyKeyboardMarkup GetButtons(string[] buttons, int inRow)
    {
        var rkm = new ReplyKeyboardMarkup { Keyboard = [] };
        var last = buttons.Last();
        buttons = buttons.Take(buttons.Length - 1).ToArray();

        while (buttons.Any())
        {
            var x = buttons.Take(inRow).ToList();
            buttons = buttons.Skip(inRow).ToArray();

            if (x.Count == 4) { rkm.Keyboard = rkm.Keyboard.Append([x[0], x[1], x[2], x[3]]); continue; }
            if (x.Count == 3) { rkm.Keyboard = rkm.Keyboard.Append([x[0], x[1], x[2]]); continue; }
            if (x.Count == 2) { rkm.Keyboard = rkm.Keyboard.Append([x[0], x[1]]); continue; }
            if (x.Count == 1) { rkm.Keyboard = rkm.Keyboard.Append([x[0]]); }
        }

        rkm.Keyboard = rkm.Keyboard.Append(new KeyboardButton[] { last });

        return rkm;
    }
}

public static class BotExtensions
{
    public static async void SetButtons(this TelegramBotClient bot, long userId, string message, IEnumerable<string> buttons, ParseMode parseMode = ParseMode.Markdown)
    {
        var buttonsInRow = buttons.Any(x => x.Length > 9) ? 3 : 4;
        var rkm = GetButtons(buttons.ToArray(), buttonsInRow);
        await bot.SendMessage(userId, message, replyMarkup: rkm, parseMode: parseMode);
    }

    public static void SetButtons(this TelegramBotClient bot, long userId, string message, params string[] buttons) =>
        bot.SetButtons(userId, message, buttons.ToList());

    private static ReplyKeyboardMarkup GetButtons(string[] buttons, int inRow)
    {
        var rkm = new ReplyKeyboardMarkup { Keyboard = new List<IEnumerable<KeyboardButton>>() };
        var last = buttons.Last();
        buttons = buttons.Take(buttons.Length - 1).ToArray();

        while (buttons.Any())
        {
            var x = buttons.Take(inRow).ToList();
            buttons = buttons.Skip(inRow).ToArray();

            if (x.Count == 4) { rkm.Keyboard = rkm.Keyboard.Append([x[0], x[1], x[2], x[3]]); continue; }
            if (x.Count == 3) { rkm.Keyboard = rkm.Keyboard.Append([x[0], x[1], x[2]]); continue; }
            if (x.Count == 2) { rkm.Keyboard = rkm.Keyboard.Append([x[0], x[1]]); continue; }
            if (x.Count == 1) { rkm.Keyboard = rkm.Keyboard.Append([x[0]]); }
        }

        rkm.Keyboard = rkm.Keyboard.Append(new KeyboardButton[] { last });

        return rkm;
    }

    public static void SendMessage(this TelegramBotClient bot, long userId, string message, ParseMode parseMode = ParseMode.Markdown)
    {
        try
        {
            bot.SendMessage(userId, message, parseMode);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            // ISSUE
            ILogger _logger = new FileLogger();
            _logger.Log(exception);
        }
    }
}
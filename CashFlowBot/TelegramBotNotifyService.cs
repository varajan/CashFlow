using CashFlow.Interfaces;
using CashFlow.Stages;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CashFlowBot;

public class TelegramBotNotifyService(ITelegramBotClient bot) : INotifyService
{
    public async Task SetButtons(long userId, IStage stage)
    {
        if (stage.Message is null || stage.Buttons.Count() == 0) return;

        var buttonsInRow = stage.Buttons.Any(x => x.Length > 9) ? 3 : 4;
        var rkm = GetButtons([.. stage.Buttons], buttonsInRow);
        await bot.SendMessage(userId, stage.Message, replyMarkup: rkm, parseMode: ParseMode.Markdown);
    }

    public async Task Notify(long userId, string message) => await bot.SendMessage(userId, message, parseMode: ParseMode.Markdown);

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

        rkm.Keyboard = rkm.Keyboard.Append([last]);

        return rkm;
    }
}

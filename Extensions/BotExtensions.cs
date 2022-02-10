using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CashFlowBot.Extensions
{
    public static class BotExtensions
    {
        public static void SetButtons(this TelegramBotClient bot, long userId, string message, IEnumerable<string> buttons) =>
            bot.SetButtons(userId, message, buttons.ToArray());

        public static async void SetButtons(this TelegramBotClient bot, long userId, string message, params string[] buttons)
        {
            var buttonsInRow = buttons.Any(x => x.Length > 9) ? 3 : 4;
            var rkm = GetButtons(buttons, buttonsInRow);
            await bot.SendTextMessageAsync(userId, message, replyMarkup: rkm, parseMode: ParseMode.Markdown);
        }

        private static ReplyKeyboardMarkup GetButtons(string[] buttons, int inRow)
        {
            var rkm = new ReplyKeyboardMarkup { Keyboard = new List<IEnumerable<KeyboardButton>>() };
            var last = buttons.Last();
            buttons = buttons.Take(buttons.Length - 1).ToArray();

            while (buttons.Any())
            {
                var x = buttons.Take(inRow).ToList();
                buttons = buttons.Skip(inRow).ToArray();

                if (x.Count == 4) { rkm.Keyboard = rkm.Keyboard.Append(new KeyboardButton[] { x[0], x[1], x[2], x[3] }); continue; }
                if (x.Count == 3) { rkm.Keyboard = rkm.Keyboard.Append(new KeyboardButton[] { x[0], x[1], x[2] }); continue; }
                if (x.Count == 2) { rkm.Keyboard = rkm.Keyboard.Append(new KeyboardButton[] { x[0], x[1] }); continue; }
                if (x.Count == 1) { rkm.Keyboard = rkm.Keyboard.Append(new KeyboardButton[] { x[0] }); }
            }

            rkm.Keyboard = rkm.Keyboard.Append(new KeyboardButton[] { last });

            return rkm;
        }

        public static async void SendMessage(this TelegramBotClient bot, long userId, string message, ParseMode parseMode = ParseMode.Markdown)
        {
            try
            {
                await bot.SendTextMessageAsync(userId, message, parseMode);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                Logger.Log(exception);
            }
        }
    }
}

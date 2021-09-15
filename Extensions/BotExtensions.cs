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
            var rkm = new ReplyKeyboardMarkup
            {
                Keyboard = buttons.Select(button => new KeyboardButton[] {button})
            };

            await bot.SendTextMessageAsync(userId, message, replyMarkup: rkm, parseMode: ParseMode.Markdown);
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

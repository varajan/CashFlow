using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CashFlowBot.Extensions
{
    public static class BotExtensions
    {
        public static async void SetButtons(this TelegramBotClient bot, long userId, string message, params string[] buttons)
        {
            var rkm = new ReplyKeyboardMarkup
            {
                Keyboard = buttons.Select(button => new KeyboardButton[] {button})
            };

            await bot.SendTextMessageAsync(userId, message, replyMarkup: rkm, parseMode: ParseMode.Markdown);
        }

        public static async void SendMessage(this TelegramBotClient bot, long userId, string message)
        {
            await bot.SendTextMessageAsync(userId, message, ParseMode.Markdown);
        }
    }
}

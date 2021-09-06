using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace CashFlowBot.Extensions
{
    public static class BotExtensions
    {
        public static async void SendMessage(this TelegramBotClient bot, long userId, string message)
        {
            await bot.SendTextMessageAsync(userId, message, ParseMode.Markdown);
        }
    }
}

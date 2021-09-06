using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace CashFlowBot
{
    public static class BotExtensions
    {
        public static async void SendMessage(this TelegramBotClient bot, int userId, string message)
        {
            await bot.SendTextMessageAsync(userId, message, ParseMode.Markdown);
        }
    }
}

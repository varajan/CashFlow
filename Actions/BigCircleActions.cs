using CashFlowBot.DataBase;
using CashFlowBot.Extensions;
using CashFlowBot.Models;
using Telegram.Bot;

namespace CashFlowBot.Actions
{
    public class BigCircleActions : BaseActions
    {
        public static void Divorce(TelegramBotClient bot, User user)
        {
            bot.SendMessage(user.Id, Terms.Get(72, user, "You've lost {0}.", user.Person.Cash.AsCurrency()));
            user.Person.Cash = 0;
            Cancel(bot, user);
        }

        public static void TaxAudit(TelegramBotClient bot, User user)
        {
            bot.SendMessage(user.Id, Terms.Get(72, user, "You've lost {0}.", (user.Person.Cash / 2).AsCurrency()));
            user.Person.Cash /= 2;
            Cancel(bot, user);
        }

        public static void GoToBigCircle(TelegramBotClient bot, User user)
        {
            user.Person.InitialCashFlow = user.Person.Assets.Income / 10 * 1000;
            user.Person.Cash += user.Person.InitialCashFlow;
            user.Person.BigCircle = true;
            user.Person.Assets.Clear();

            Cancel(bot, user);
        }
    }
}

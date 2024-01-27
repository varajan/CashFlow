using CashFlowBot.DataBase;
using CashFlowBot.Extensions;
using CashFlowBot.Models;
using Telegram.Bot;

namespace CashFlowBot.Actions;

public class BigCircleActions : BaseActions
{
    public static void LostMoney(TelegramBotClient bot, User user, int amount, Data.ActionType action)
    {
        bot.SendMessage(user.Id, Terms.Get(72, user, "You've lost {0}.", amount.AsCurrency()));
        user.Person.Cash -= amount;
        user.History.Add(action, amount);
        Cancel(bot, user);
    }

    public static void GoToBigCircle(TelegramBotClient bot, User user)
    {
        user.Person.InitialCashFlow = user.Person.Assets.Income / 10 * 1000;
        user.Person.Cash += user.Person.InitialCashFlow;
        user.Person.BigCircle = true;

        user.History.Add(Data.ActionType.GoToBigCircle);

        Cancel(bot, user);
    }
}
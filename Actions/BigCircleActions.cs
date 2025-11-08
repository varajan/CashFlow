using CashFlowBot.Data;
using CashFlowBot.Data.Consts;
using CashFlowBot.Data.DataBase;
using CashFlowBot.Data.Users;
using CashFlowBot.Data.Users.UserData.PersonData;
using CashFlowBot.Extensions;
using CashFlowBot.Loggers;
using Telegram.Bot;

namespace CashFlowBot.Actions;

public class BigCircleActions : BaseActions
{
    private static ILogger logger = new FileLogger();
    private static IDataBase dataBase = new SQLiteDataBase(logger);
    private static ITermsService Terms => new TermsService(dataBase);

    public static void LostMoney(TelegramBotClient bot, IUser user, int amount, ActionType action)
    {
        bot.SendMessage(user.Id, Terms.Get(72, user, "You've lost {0}.", amount.AsCurrency()));
        user.Person.Cash -= amount;
        user.History.Add(action, amount);
        Cancel(bot, user);
    }

    public static void GoToBigCircle(TelegramBotClient bot, IUser user)
    {
        user.Person.InitialCashFlow = user.Person.Assets.Income / 10 * 1000;
        user.Person.Cash += user.Person.InitialCashFlow;
        user.Person.Circle = Circle.Big;

        user.History.Add(ActionType.GoToBigCircle);

        Cancel(bot, user);
    }
}
using CashFlowBot.Extensions;
using System.Globalization;
using System;
using System.Linq;
using CashFlowBot.Data.Consts;
using CashFlowBot.Data.DataBase;
using TelegramUser = Telegram.Bot.Types.User;
using CashFlowBot.Data.Users.UserData.HistoryData;
using CashFlowBot.Data.Users.UserData.PersonData;

namespace CashFlowBot.Data.Users;

public class User(IDataBase dataBase, long id) : BaseDataModel(dataBase, id, "Users"), IUser
{
    public IHistory History => new History(DataBase, this);
    public IPerson Person => new Person(DataBase, this);

    public bool Exists => DataBase.GetColumn($"SELECT ID FROM {Table} WHERE ID = {Id}").Any();

    public Stage Stage { get => (Stage)GetInt("Stage"); set => Set("Stage", (int)value); }

    public string Name { get => Get("Name"); private set => Set("Name", value); }

    public void SetName(TelegramUser user = null)
    {
        var name = $"{user?.FirstName} {user?.LastName}".Trim();
        Name = string.IsNullOrEmpty(name) ? user?.Username : name;
    }

    public DateTime FirstLogin => Get("FirstLogin").ToDateTime();

    public DateTime LastActive
    {
        get => Get("LastActive").ToDateTime();
        set => Set("LastActive", value.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture));
    }

    public bool IsAdmin { get => GetInt("Admin") == 1; set => Set("Admin", value ? 1 : 0); }

    public Language Language { get => (Language)GetInt("Language"); set => Set("Language", (int)value); }

    public string Description => Person.Description +
                                 Person.Assets.Description +
                                 Person.Expenses.Description;

    public void GetCredit(int amount)
    {
        Person.Cash += amount;
        Person.Expenses.BankLoan += amount / 10;
        Person.Liabilities.BankLoan += amount;
        History.Add(ActionType.Credit, amount);
    }

    public int PayCredit(int amount, bool regular)
    {
        amount = amount / 1000 * 1000;
        amount = Math.Min(amount, Person.Liabilities.BankLoan);
        var percent = (decimal)1 / 10;
        var expenses = (int)(amount * percent);

        Person.Cash -= amount;
        Person.Expenses.BankLoan -= expenses;
        Person.Liabilities.BankLoan -= amount;
        History.Add(regular ? ActionType.BankLoan : ActionType.BankruptcyBankLoan, amount);

        return amount;
    }

    public void Create()
    {
        DataBase.Execute($"INSERT INTO {Table} " +
                   "(ID, Stage, Admin, Name, Language, LastActive, FirstLogin) " +
                   $"VALUES ({Id}, '', '', '', '', '', '')");

        Set("FirstLogin", DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture));
        LastActive = DateTime.Now;
    }
}

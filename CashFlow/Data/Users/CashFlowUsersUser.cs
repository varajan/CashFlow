using CashFlow.Extensions;
using System.Globalization;
using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.HistoryData;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Stages;
using CashFlow.Interfaces;

namespace CashFlow.Data.Users;

public class CashFlowUsersUser(IDataBase dataBase, INotifyService notifyService, long id) : BaseDataModel(dataBase, id, "Users"), IUser
{
    public IHistory History => new UserData.HistoryData.History(DataBase, this);
    public IPerson Person_OBSOLETE => new Person(DataBase, this);

    public bool Exists => DataBase.GetColumn($"SELECT ID FROM {Table} WHERE ID = {Id}").Any();
    public Stage Stage { get => throw new Exception(); set => throw new Exception(); }
    public string StageName { get => Get("Stage"); set => Set("Stage", value); }
    public string Name { get => Get("Name"); set => Set("Name", value); }
    public bool IsActive => LastActive > DateTime.Now.AddMinutes(-15);

    public DateTime FirstLogin => Get("FirstLogin").ToDateTime();

    public DateTime LastActive
    {
        get => Get("LastActive").ToDateTime();
        set => Set("LastActive", value.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture));
    }

    public bool IsAdmin { get => GetInt("Admin") == 1; set => Set("Admin", value ? 1 : 0); }

    public Language Language { get => (Language)GetInt("Language"); set => Set("Language", (int)value); }

    public string Description => Person_OBSOLETE.Description +
                                 Person_OBSOLETE.Assets.Description +
                                 Person_OBSOLETE.Expenses.Description;

    public void GetCredit(int amount)
    {
        Person_OBSOLETE.Cash += amount;
        Person_OBSOLETE.Expenses.BankLoan += amount / 10;
        Person_OBSOLETE.Liabilities.BankLoan += amount;
        History.Add(ActionType.Credit, amount);
    }

    public int PayCredit(int amount, bool regular)
    {
        amount = amount / 1000 * 1000;
        amount = Math.Min(amount, Person_OBSOLETE.Liabilities.BankLoan);
        var percent = (decimal)1 / 10;
        var expenses = (int)(amount * percent);

        Person_OBSOLETE.Cash -= amount;
        Person_OBSOLETE.Expenses.BankLoan -= expenses;
        Person_OBSOLETE.Liabilities.BankLoan -= amount;
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

    public async Task SetButtons(IStage stage) => await notifyService.SetButtons(stage);
    public async Task Notify(string message) => await notifyService.Notify(message);
}

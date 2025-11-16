using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.HistoryData;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Data.Users.UserData.PersonData;

public class Person(IDataBase dataBase, IUser user) : BaseDataModel(dataBase, user.Id, "Persons"), IPerson
{
    public string Profession
    {
        get
        {
            var profession = Get("Profession");
            return Persons.Get(profession).Profession[User.Language];
        }
        set => Set("Profession", value);
    }

    private IUser User { get; } = user;

    public int Cash { get => GetInt("Cash"); set => Set("Cash", value); }
    public int Salary { get => GetInt("Salary"); set => Set("Salary", value); }
    public int CashFlow => Salary + Assets.Income - Expenses.Total;
    public bool ReadyForBigCircle => User.Person_OBSOLETE.Assets.Income > User.Person_OBSOLETE.Expenses.Total;
    public bool Bankruptcy { get => GetInt("Bankruptcy") == 1; set => Set("Bankruptcy", value ? 1 : 0); }
    public bool CreditsReduced { get => GetInt("CreditsReduced") == 1; set => Set("CreditsReduced", value ? 1 : 0); }
    public Circle Circle { get => BigCircle ? Circle.Big : Circle.Small; set => throw new NotImplementedException(); }
    public bool BigCircle { get => GetInt("BigCircle") == 1; set => Set("BigCircle", value ? 1 : 0); }
    //public bool SmallRealEstate { get => GetInt("SmallRealEstate") == 1; set => Set("SmallRealEstate", value ? 1 : 0); }

    public IExpenses Expenses => new Expenses(DataBase, User);
    public ILiabilities Liabilities => new Liabilities(DataBase, User);
    public IAssets Assets => new Assets(DataBase, User);

    private string ProfessionTerm => Terms.Get(50, User, "Profession");
    private string CashTerm => Terms.Get(51, User, "Cash");
    private string SalaryTerm => Terms.Get(52, User, "Salary");
    private string IncomeTerm => Terms.Get(53, User, "Income");
    private string ExpensesTerm => Terms.Get(54, User, "Expenses");
    private string CashFlowTerm => Terms.Get(55, User, "Cash Flow");
    private string InitialTerm => Terms.Get(65, User, "Initial");
    private string CurrentTerm => Terms.Get(66, User, "Current");
    private string TargetTerm => Terms.Get(67, User, "Target");

    public int InitialCashFlow { get => GetInt("InitialCashFlow"); set => Set("InitialCashFlow", value); }
    public int TargetCashFlow => InitialCashFlow + 50_000;
    public int CurrentCashFlow => InitialCashFlow + Assets.Businesses.Sum(x => x.TotalCashFlow);

    private string SmallCircleDescription =>
        $"*{ProfessionTerm}:* {Profession}{Environment.NewLine}" +
        $"*{CashTerm}:* {Cash.AsCurrency()}{Environment.NewLine}" +
        $"*{SalaryTerm}:* {Salary.AsCurrency()}{Environment.NewLine}" +
        $"*{IncomeTerm}:* {Assets.Income.AsCurrency()}{Environment.NewLine}" +
        $"*{ExpensesTerm}:* {Expenses.Total.AsCurrency()}{Environment.NewLine}" +
        $"*{CashFlowTerm}*: {CashFlow.AsCurrency()}";

    private string BigCircleDescription =>
        $"*{ProfessionTerm}:* {Profession}{Environment.NewLine}" +
        $"*{CashTerm}:* {Cash.AsCurrency()}{Environment.NewLine}" +
        $"{InitialTerm} {CashFlowTerm}: {InitialCashFlow.AsCurrency()}{Environment.NewLine}" +
        $"{CurrentTerm} {CashFlowTerm}: {CurrentCashFlow.AsCurrency()}{Environment.NewLine}" +
        $"{TargetTerm} {CashFlowTerm}: {TargetCashFlow.AsCurrency()}{Environment.NewLine}" +
        $"{Assets.BigCircleDescription}";

    public string Description
    {
        get
        {
            User.LastActive = DateTime.Now;
            return BigCircle ? BigCircleDescription : SmallCircleDescription;
        }
    }

    public bool Exists => DataBase.GetColumn($"SELECT ID FROM {Table} WHERE ID = {Id}").Any();
    public void Clear() => DataBase.Execute($"DELETE FROM {Table} WHERE ID = {Id}");

    public void Create(string profession)
    {
        var defaultProfessionData = Persons.Get(profession);

        Clear();
        DataBase.Execute($"INSERT INTO {Table} " +
                   "(ID, Profession, Salary, Cash, SmallRealEstate, ReadyForBigCircle, BigCircle, InitialCashFlow, Bankruptcy, CreditsReduced) " +
                   $"VALUES ({Id}, '', '', '', '', '', '', '', 0, 0)");

        Assets.Clear();
        Profession = defaultProfessionData.Profession[User.Language];
        Cash = defaultProfessionData.Cash;
        Salary = defaultProfessionData.Salary;

        Expenses.Clear();
        Expenses.Create(defaultProfessionData.Expenses);

        Liabilities.Clear();
        Liabilities.Create(defaultProfessionData.Liabilities);
    }

    public void ReduceCreditsRollback()
    {
        var person = Persons.Get(User.Person_OBSOLETE.Profession);
        var count = User.History.Count(ActionType.BankruptcyDebtRestructuring);

        Expenses.CarLoan = person.Expenses.CarLoan;
        Expenses.CreditCard = person.Expenses.CreditCard;
        Expenses.SmallCredits = person.Expenses.SmallCredits;
        Liabilities.CarLoan = person.Liabilities.CarLoan;
        Liabilities.CreditCard = person.Liabilities.CreditCard;
        Liabilities.SmallCredits = person.Liabilities.SmallCredits;

        for (var i = 0; i < count; i++)
        {
            Expenses.CarLoan /= 2;
            Expenses.CreditCard /= 2;
            Expenses.SmallCredits /= 2;
            Liabilities.CarLoan /= 2;
            Liabilities.CreditCard /= 2;
            Liabilities.SmallCredits /= 2;
        }

        CreditsReduced = false;
        Bankruptcy = CashFlow < 0;
    }

    public void ReduceCredits()
    {
        if (CreditsReduced) return;

        Expenses.CarLoan /= 2;
        Expenses.SmallCredits /= 2;
        Liabilities.CarLoan /= 2;
        Liabilities.CreditCard /= 2;
        Liabilities.SmallCredits /= 2;

        CreditsReduced = true;
        Bankruptcy = CashFlow < 0;

        var history = new History(DataBase, User);
        var count = history.Count(ActionType.BankruptcyDebtRestructuring);
        history.Add(ActionType.BankruptcyDebtRestructuring, count);
    }
}
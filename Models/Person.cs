using CashFlowBot.Data;
using CashFlowBot.DataBase;
using CashFlowBot.Extensions;
using System;
using System.Linq;

namespace CashFlowBot.Models;

public class Person : DataModel
{
    public Person(long userId) : base(userId, "Persons") { }

    public string Profession
    {
        get
        {
            var profession = Get("Profession");
            return Persons.Get(Id, profession).Profession;
        }
        set => Set("Profession", value);
    }

    public int Cash { get => GetInt("Cash"); set => Set("Cash", value); }
    public int Salary { get => GetInt("Salary"); set => Set("Salary", value); }
    public int CashFlow => Salary + Assets.Income - Expenses.Total;
    public bool ReadyForBigCircle { get => GetInt("ReadyForBigCircle") == 1; set => Set("ReadyForBigCircle", value ? 1 : 0); }
    public bool Bankruptcy { get => GetInt("Bankruptcy") == 1; set => Set("Bankruptcy", value ? 1 : 0); }
    public bool CreditsReduced { get => GetInt("CreditsReduced") == 1; set => Set("CreditsReduced", value ? 1 : 0); }
    public bool BigCircle { get => GetInt("BigCircle") == 1; set => Set("BigCircle", value ? 1 : 0); }
    public bool SmallRealEstate { get => GetInt("SmallRealEstate") == 1; set => Set("SmallRealEstate", value ? 1 : 0); }

    public Expenses Expenses => new(Id);
    public Liabilities Liabilities => new(Id);
    public Assets Assets => new(Id);

    private string ProfessionTerm => Terms.Get(50, Id, "Profession");
    private string CashTerm => Terms.Get(51, Id, "Cash");
    private string SalaryTerm => Terms.Get(52, Id, "Salary");
    private string IncomeTerm => Terms.Get(53, Id, "Income");
    private string ExpensesTerm => Terms.Get(54, Id, "Expenses");
    private string CashFlowTerm => Terms.Get(55, Id, "Cash Flow");
    private string InitialTerm => Terms.Get(65, Id, "Initial");
    private string CurrentTerm => Terms.Get(66, Id, "Current");
    private string TargetTerm => Terms.Get(67, Id, "Target");

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
            new User(Id).LastActive = DateTime.Now;
            return BigCircle ? BigCircleDescription : SmallCircleDescription;
        }
    }

    public bool Exists => DB.GetColumn($"SELECT ID FROM {Table} WHERE ID = {Id}").Any();
    public void Clear() => DB.Execute($"DELETE FROM {Table} WHERE ID = {Id}");

    public void Create(string profession)
    {
        var data = Data.Persons.Get(Id).First(x => x.Profession.ToLower() == profession);

        Clear();
        DB.Execute($"INSERT INTO {Table} " +
                   "(ID, Profession, Salary, Cash, SmallRealEstate, ReadyForBigCircle, BigCircle, InitialCashFlow, Bankruptcy, CreditsReduced) " +
                   $"VALUES ({Id}, '', '', '', '', '', '', '', 0, 0)");

        Assets.Clear();
        Profession = data.Profession;
        Cash = data.Cash;
        Salary = data.Salary;

        Expenses.Clear();
        Expenses.Create(data.Expenses);

        Liabilities.Clear();
        Liabilities.Create(data.Liabilities);
    }

    public void ReduceCreditsRollback()
    {
        var user = new User(Id);
        var person = Persons.Get(user.Id, user.Person.Profession);
        var count = user.History.Count(ActionType.BankruptcyDebtRestructuring);

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
        Expenses.CreditCard /= 2;
        Expenses.SmallCredits /= 2;
        Liabilities.CarLoan /= 2;
        Liabilities.CreditCard /= 2;
        Liabilities.SmallCredits /= 2;

        CreditsReduced = true;
        Bankruptcy = CashFlow < 0;

        var history = new User(Id).History;
        var count = history.Count(ActionType.BankruptcyDebtRestructuring);
        history.Add(ActionType.BankruptcyDebtRestructuring, count);
    }
}
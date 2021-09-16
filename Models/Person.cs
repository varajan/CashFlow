using System;
using System.Linq;
using CashFlowBot.DataBase;
using CashFlowBot.Extensions;

namespace CashFlowBot.Models
{
    public class Person : DataModel
    {
        public Person(long userId) : base(userId, DB.Tables.Persons) { }

        public string Profession { get => Get("Profession"); set => Set("Profession", value); }
        public int Cash { get => GetInt("Cash"); set => Set("Cash", value); }
        public int Salary { get => GetInt("Salary"); set => Set("Salary", value); }
        public int CashFlow => Salary + Assets.Income - Expenses.Total;
        public bool BigCircle { get => GetInt("BigCircle") == 1; set => Set("BigCircle", value ? 1 : 0); }

        public Expenses Expenses => new(Id);
        public Liabilities Liabilities => new(Id);
        public Assets Assets => new (Id);

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
        public int CurrentCashFlow => InitialCashFlow + Assets.Income;

        private string SmallCircleDescription => $"*{ProfessionTerm}:* {Profession}{Environment.NewLine}" +
                     $"*{CashTerm}:* {Cash.AsCurrency()}{Environment.NewLine}" +
                     $"*{SalaryTerm}:* {Salary.AsCurrency()}{Environment.NewLine}" +
                     $"*{IncomeTerm}:* {Assets.Income.AsCurrency()}{Environment.NewLine}" +
                     $"*{ExpensesTerm}:* {Expenses.Total.AsCurrency()}{Environment.NewLine}" +
                     $"*{CashFlowTerm}*: {CashFlow.AsCurrency()}";
        public string BigCircleDescription => $"*{CashTerm}:* {Cash.AsCurrency()}{Environment.NewLine}" +
                                              $"{InitialTerm} {CashFlowTerm}:{InitialCashFlow.AsCurrency()}" +
                                              $"{CurrentTerm} {CashFlowTerm}: {CurrentCashFlow.AsCurrency()}" +
                                              $"{TargetTerm} {CashFlowTerm}: {TargetCashFlow.AsCurrency()}" +
                                              $"{Assets.Description}";

        public string Description => BigCircle ? BigCircleDescription : SmallCircleDescription;

        public bool Exists => DB.GetColumn($"SELECT ID FROM {Table} WHERE ID = {Id}").Any();
        public void Clear() => DB.Execute($"DELETE FROM {Table} WHERE ID = {Id}");

        public void Create(string profession)
        {
            var data = Data.Persons.Get(Id).First(x => x.Profession.ToLower() == profession);

            Clear();
            DB.Execute($"INSERT INTO {Table} ({DB.ColumnNames.Persons}) VALUES ({Id}, {DB.DefaultValues.Persons})");

            Assets.Items.ForEach(x => x.Delete());

            Profession = data.Profession;
            Cash = data.Cash;
            Salary = data.Salary;

            Expenses.Clear();
            Expenses.Create(data.Expenses);

            Liabilities.Clear();
            Liabilities.Create(data.Liabilities);
        }
    }
}

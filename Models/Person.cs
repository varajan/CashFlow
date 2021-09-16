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

        public Expenses Expenses => new(Id);
        public Liabilities Liabilities => new(Id);
        public Assets Assets => new (Id);

        private string ProfessionTerm => Terms.Get(50, Id, "Profession");
        private string CashTerm => Terms.Get(51, Id, "Cash");
        private string SalaryTerm => Terms.Get(52, Id, "Salary");
        private string IncomeTerm => Terms.Get(53, Id, "Income");
        private string ExpensesTerm => Terms.Get(54, Id, "Expenses");
        private string CashFlowTerm => Terms.Get(55, Id, "Cash Flow");

        public string Description => $"*{ProfessionTerm}:* {Profession}{Environment.NewLine}" +
                     $"*{CashTerm}:* {Cash.AsCurrency()}{Environment.NewLine}" +
                     $"*{SalaryTerm}:* {Salary.AsCurrency()}{Environment.NewLine}" +
                     $"*{IncomeTerm}:* {Assets.Income.AsCurrency()}{Environment.NewLine}" +
                     $"*{ExpensesTerm}:* {Expenses.Total.AsCurrency()}{Environment.NewLine}" +
                     $"*{CashFlowTerm}*: {CashFlow.AsCurrency()}";

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

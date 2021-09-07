using System;
using System.Linq;
using CashFlowBot.DataBase;

namespace CashFlowBot.Models
{
    public class Person : DataModel
    {
        public Person(long userId) : base(userId, DB.Tables.Persons) { }

        public string Profession { get => Get("Profession"); set => Set("Profession", value); }
        public int Cash { get => GetInt("Cash"); set => Set("Cash", value); }
        public int Salary { get => GetInt("Salary"); set => Set("Salary", value); }
        public int CashFlow => Salary - Expenses.Total;

        public Expenses Expenses => new(Id);
        public Liabilities Liabilities => new(Id);

        public string Description => $"*Profession:* {Profession}{Environment.NewLine}" +
                                     $"*Salary:* ${Salary}{Environment.NewLine}" +
                                     $"*Cash:* ${Cash}{Environment.NewLine}";

        public bool Exists => DB.GetColumn($"SELECT ID FROM {Table} WHERE ID = {Id}").Any();
        public void Clear() => DB.Execute($"DELETE FROM {Table} WHERE ID = {Id}");

        public void Create(string profession)
        {
            var data = Data.Persons.Items.First(x => x.Profession.ToLower() == profession);

            Clear();
            DB.Execute($"INSERT INTO {Table} ({DB.ColumnNames.Persons}) VALUES ({Id}, {DB.DefaultValues.Persons})");

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

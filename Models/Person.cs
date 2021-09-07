using System.Linq;
using CashFlowBot.DataBase;
using CashFlowBot.Extensions;

namespace CashFlowBot.Models
{
    public class Person
    {
        private long Id { get; init; }

        public Person(long userId) => Id = userId;

        public string Profession
        {
            get => DB.GetValue($"SELECT Profession FROM {DB.Tables.Persons} WHERE ID = {Id}");
            set => DB.Execute($"UPDATE {DB.Tables.Persons} SET Profession = '{value}' WHERE ID = {Id}");
        }

        public int Assets
        {
            get => DB.GetValue($"SELECT Assets FROM {DB.Tables.Persons} WHERE ID = {Id}").ToInt();
            set => DB.Execute($"UPDATE {DB.Tables.Persons} SET Assets = {value} WHERE ID = {Id}");
        }

        public int Salary
        {
            get => DB.GetValue($"SELECT Salary FROM {DB.Tables.Persons} WHERE ID = {Id}").ToInt();
            set => DB.Execute($"UPDATE {DB.Tables.Persons} SET Salary = {value} WHERE ID = {Id}");
        }

        public Expenses Expenses { get; set; } = new();
        public Liabilities Liabilities { get; set; } = new();

        public bool Exists => DB.GetColumn($"SELECT ID FROM {DB.Tables.Persons} WHERE ID = {Id}").Any();
        public void Delete() => DB.Execute($"DELETE FROM {DB.Tables.Persons} WHERE ID = {Id}");

        public void Create(string profession)
        {
            var data = Data.Persons.Items.First(x => x.Profession.ToLower() == profession);

            DB.Execute($"INSERT INTO {DB.Tables.Persons} ({DB.ColumnNames.Persons}) VALUES ({DB.DefaultValues.Persons})");

            Profession = data.Profession;
            Assets = data.Assets;
            Salary = data.Salary;
        }
    }

    public class Expenses
    {
        public int Total => Others + Taxes + Mortgage + SchoolLoan + CarLoan + CreditCard + BankLoan + ChildrenExpenses;

        public int Taxes { get; set; }
        public int Mortgage { get; set; }
        public int SchoolLoan { get; set; }
        public int CarLoan { get; set; }
        public int CreditCard { get; set; }
        public int BankLoan { get; set; }
        public int Others { get; set; }

        public int Children { get; set; } = 0;
        public int PerChild { get; set; }
        public int ChildrenExpenses => Children * PerChild;
    }

    public class Liabilities
    {
        public int Mortgage { get; set; }
        public int SchoolLoan { get; set; }
        public int CarLoan { get; set; }
        public int CreditCard { get; set; }
        public int BankLoan { get; set; }
    }
}

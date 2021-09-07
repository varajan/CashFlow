using System.Linq;
using CashFlowBot.DataBase;

namespace CashFlowBot.Models
{
    public class Person : DataModel
    {
        public Person(long userId) : base(userId, DB.Tables.Persons) { }

        public string Profession { get => Get("Profession"); set => Set("Profession", value); }
        public int Assets { get => GetInt("Assets"); set => Set("Assets", value); }
        public int Salary { get => GetInt("Salary"); set => Set("Salary", value); }

        public Expenses Expenses => new(Id);
        public Liabilities Liabilities => new(Id);

        public bool Exists => DB.GetColumn($"SELECT ID FROM {Table} WHERE ID = {Id}").Any();
        public void Delete() => DB.Execute($"DELETE FROM {Table} WHERE ID = {Id}");

        public void Create(string profession)
        {
            var data = Data.Persons.Items.First(x => x.Profession.ToLower() == profession);

            DB.Execute($"INSERT INTO {Table} ({DB.ColumnNames.Persons}) VALUES ({DB.DefaultValues.Persons})");

            Profession = data.Profession;
            Assets = data.Assets;
            Salary = data.Salary;

            Expenses.Clear();
            Expenses.Create(data.Expenses);

            Liabilities.Clear();
            Liabilities.Create(data.Liabilities);
        }
    }
}

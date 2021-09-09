using System;
using System.Linq;
using CashFlowBot.Data;
using CashFlowBot.DataBase;

namespace CashFlowBot.Models
{
    public class User : DataModel
    {
        public User(long id) : base(id, DB.Tables.Users) { }

        public Person Person => new (Id);

        public bool Exists => DB.GetColumn($"SELECT ID FROM {Table} WHERE ID = {Id}").Any();

        public Stage Stage { get => (Stage) GetInt("Stage"); set => Set("Stage", (int)value); }

        public string Description => Person.Description +
                                     Person.Assets.Description +
                                     Person.Expenses.Description;

        public string ShortDescription => $"*Profession:* {Person.Profession}{Environment.NewLine}" +
                             $"*Salary:* ${Person.Salary}{Environment.NewLine}" +
                             $"*Cash:* ${Person.Cash}{Environment.NewLine}" +
                             $"*Income:* $n/a{Environment.NewLine}" +
                             $"*Expenses:* ${Person.Expenses.Total}{Environment.NewLine}" +
                             $"*Cash Flow*: ${Person.CashFlow}";

        public void Create()
        {
            DB.Execute($"INSERT INTO {Table} ({DB.ColumnNames.Users}) VALUES ({Id}, {DB.DefaultValues.Users})");
        }
    }
}

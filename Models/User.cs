using System.Linq;
using CashFlowBot.Data;
using CashFlowBot.DataBase;
using CashFlowBot.Extensions;

namespace CashFlowBot.Models
{
    public class User
    {
        public User(long id) => Id = id;

        public long Id { get; init; }

        public Person Person => new (Id);

        public bool Exists => DB.GetColumn($"SELECT ID FROM {DB.Tables.Users} WHERE ID = {Id}").Any();

        public Stages Stage
        {
            get => (Stages) DB.GetValue($"SELECT Stage FROM {DB.Tables.Users} WHERE ID = {Id}").ToInt();
            set => DB.Execute($"UPDATE {DB.Tables.Users} SET Stage = {(int)value} WHERE ID = {Id}");
        }

        public void DeleteExpenses() => DB.Execute($"DELETE FROM {DB.Tables.Expenses} WHERE ID = {Id}");
    }
}

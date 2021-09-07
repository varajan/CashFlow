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

        public Stages Stage { get => (Stages) GetInt("Stage"); set => Set("Stage", (int)value); }
    }
}

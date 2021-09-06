using System.Linq;

namespace CashFlowBot.DataBase
{
    public static class Users
    {
        private const string Columns = "ID, Name";
        private const string Table = "Users";

        static Users() => DB.Execute($"CREATE TABLE IF NOT EXISTS {Table} (Number ID, Name Text); ");

        public static bool Exists(long id) => DB.GetColumn($"SELECT ID FROM {Table} WHERE ID = {id}").Any();

        //public static User Get(long id)
        //{
        //    if (!Exists(id)) return null;

        //    var data= DB.GetRow($"SELECT {Columns} FROM {Table} WHERE ID = {id}");

        //    return new User { Id = data[0], Name = }
        //}
    }
}

using System.Linq;

namespace CashFlowBot.DataBase
{
    public static class Users
    {
        private const string Columns = "ID, Name";
        private const string Table = "Contacts";

        static Users() => DB.Execute($"CREATE TABLE IF NOT EXISTS {Table} (Number ID, Name Text); ");

        public static bool Exists(long id) => DB.GetColumn($"SELECT ID FROM {Table}").Any();
    }
}

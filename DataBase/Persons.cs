namespace CashFlowBot.DataBase
{
    public static class Persons
    {
        private const string Columns = "ID, Profession, Salary, Assets";
        private const string Table = "Persons";

        static Persons() => DB.Execute($"CREATE TABLE IF NOT EXISTS {Table} (ID Number, Profession Text, Salary Number, Assets Number); ");

        public static void Delete(long id) => DB.Execute($"DELETE FROM {Table} WHERE ID = {id}");
    }
}

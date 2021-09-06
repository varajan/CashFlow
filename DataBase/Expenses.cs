using System.Linq;

namespace CashFlowBot.DataBase
{
    public static class Expenses
    {
        private static readonly string[] ColumnNames = { "ID", "Taxes", "Mortgage", "SchoolLoan", "CarLoan", "CreditCard", "BankLoan", "Others", "Children", "PerChild" };
        private static readonly string Columns = string.Join(", ", ColumnNames.Select(x => $"{x} Number"));
        private const string Table = "Expenses";

        static Expenses() => DB.Execute($"CREATE TABLE IF NOT EXISTS {Table} ({Columns});");

        public static void Delete(long id) => DB.Execute($"DELETE FROM {Table} WHERE ID = {id}");
    }
}

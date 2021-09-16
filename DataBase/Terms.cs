using CashFlowBot.Extensions;
using CashFlowBot.Models;

namespace CashFlowBot.DataBase
{
    public static class Terms
    {
        private static readonly string Table = DB.Tables.Terms;

        static Terms()
        {
            DB.Execute($"DELETE FROM {Table}");
            DB.Execute(Data.Terms.terms_en);
            DB.Execute(Data.Terms.terms_ua);
        }

        public static string Get(int id, long userId, string defaultValue, params object[] args) =>
            Get(id, new User(userId), defaultValue, args);

        public static string Get(int id, User user, string defaultValue, params object[] args)
        {
            var term = DB.GetValue($"SELECT Term FROM {Table} WHERE ID = {id} AND Language = '{user.Language}'").NullIfEmpty() ?? $"#{id}#{defaultValue}#";
            return string.Format(term, args);
        }
    }
}

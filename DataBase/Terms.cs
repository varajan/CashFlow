using System;
using System.Collections.Generic;
using CashFlowBot.Data;
using CashFlowBot.Extensions;
using CashFlowBot.Models;

namespace CashFlowBot.DataBase
{
    public static class Terms
    {
        private static readonly string Table = DB.Tables.Terms;

        static Terms()
        {
            Logger.Log("[constructor] Terms >>>");

            DB.Execute($"DELETE FROM {Table}");
            Logger.Log("Clear table");

            DB.Execute(Data.Terms.terms_en, true);
            Logger.Log("English");

            DB.Execute(Data.Terms.terms_ua, true);
            Logger.Log("Ukrainian");

            DB.Execute(Data.Terms.terms_de, true);
            Logger.Log("Deutsch");

            Logger.Log("[constructor] Terms <<<");
        }

        public static List<string> Get(int id)
        {
            var result = new List<string>();

            foreach (Language language in Enum.GetValues(typeof(Language)))
            {
                var term = DB.GetValue($"SELECT Term FROM {Table} WHERE ID = {id} AND Language = '{language}'");

                result.Add(term);
            }

            return result;
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

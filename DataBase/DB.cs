using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using CashFlowBot.Extensions;

namespace CashFlowBot.DataBase
{
    public static class DB
    {
        public static string DBFileName => $"{AppDomain.CurrentDomain.BaseDirectory}/DB.db";
        private static string Connection => $"Data Source={DBFileName}; Version=3;";

        static DB()
        {
            if (!File.Exists(DBFileName))
            {
                SQLiteConnection.CreateFile(DBFileName);
            }

            Execute($"CREATE TABLE IF NOT EXISTS {Tables.Users} ({CreateColumns.Users}); ");
            Execute($"CREATE TABLE IF NOT EXISTS {Tables.Persons} ({CreateColumns.Persons}); ");
            Execute($"CREATE TABLE IF NOT EXISTS {Tables.Expenses} ({CreateColumns.Expenses});");
            Execute($"CREATE TABLE IF NOT EXISTS {Tables.Liabilities} ({CreateColumns.Liabilities});");
            Execute($"CREATE TABLE IF NOT EXISTS {Tables.Assets} ({CreateColumns.Assets});");
            Execute($"CREATE TABLE IF NOT EXISTS {Tables.AvailableAssets} ({CreateColumns.AvailableAssets});");
    }

    public static void Execute(string sql)
        {
            try
            {
                using var connection = new SQLiteConnection(Connection);
                using var cmd = new SQLiteCommand(sql, connection);

                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception e) { e.Log(sql); }
        }

        public static string GetValue(string sql)
        {
            string result = null;

            try
            {
                using var connection = new SQLiteConnection(Connection);
                using var cmd = new SQLiteCommand(sql, connection);

                cmd.Connection.Open();
                result = (cmd.ExecuteScalar() ?? string.Empty).ToString();
            }
            catch (Exception e) { e.Log(sql); }

            return result;
        }

        public static List<string> GetColumn(string sql)
        {
            var result = new List<string>();

            try
            {
                using var connection = new SQLiteConnection(Connection);
                using var cmd = new SQLiteCommand(sql, connection);
                cmd.Connection.Open();

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    result.Add(reader[Columns(sql).First()].ToString());
                }
            }
            catch (Exception e) { e.Log(sql); }

            return result;
        }

        public static List<List<string>> GetRows(string sql, bool toLoverCase = false)
        {
            var result = new List<List<string>>();

            try
            {
                using var connection = new SQLiteConnection(Connection);
                using var cmd = new SQLiteCommand(sql, connection);
                cmd.Connection.Open();

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var values = Columns(sql).Select(column => reader[column.Trim()].ToString()).ToList();

                    result.Add(toLoverCase ? values.Select(x => x.ToLower()).ToList() : values);
                }
            }
            catch (Exception e) { e.Log(sql); }

            return result;
        }

        public static List<string> GetRow(string sql)
        {
            var result = new List<string>();

            try
            {
                using var connection = new SQLiteConnection(Connection);
                using var cmd = new SQLiteCommand(sql, connection);
                cmd.Connection.Open();

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    result = Columns(sql).Select(column => reader[column.Trim()].ToString()).ToList();
                }
            }
            catch (Exception e) { e.Log(sql); }

            return result;
        }

        private static IEnumerable<string> Columns(string sql) =>
            sql.Replace("DISTINCT", string.Empty).SubString("select", "from").Trim().Split(",");

        private static void Log(this Exception ex, string sql) =>
            Console.WriteLine($"{ex.Message}{Environment.NewLine}{sql}{Environment.NewLine}{ex.StackTrace}");

        public static class Tables
        {
            public static string Users = "Users";
            public static string Persons = "Persons";
            public static string Expenses = "Expenses";
            public static string Liabilities = "Liabilities";
            public static string Assets = "Assets";
            public static string AvailableAssets = "AvailableAssets";
        }

        public static class DefaultValues
        {
            private static string GetDefaults(string query) => string.Join(", ", Enumerable.Repeat("''", query.Count(x => x == ',')));

            public static string Users = GetDefaults(CreateColumns.Users);
            public static string Persons = GetDefaults(CreateColumns.Persons);
            public static string Expenses = GetDefaults(CreateColumns.Expenses);
            public static string Liabilities = GetDefaults(CreateColumns.Liabilities);
            public static string Assets = GetDefaults(CreateColumns.Assets);
            public static string AvailableAssets = GetDefaults(CreateColumns.AvailableAssets);
        }

        public static class ColumnNames
        {
            private static string GetColumns(string query) => string.Join(", ", query.Split(',').Select(x => x.Trim().Split(' ').First()));

            public static string Users = GetColumns(CreateColumns.Users);
            public static string Persons = GetColumns(CreateColumns.Persons);
            public static string Expenses = GetColumns(CreateColumns.Expenses);
            public static string Liabilities = GetColumns(CreateColumns.Liabilities);
            public static string Assets = GetColumns(CreateColumns.Assets);
            public static string AvailableAssets = GetColumns(CreateColumns.AvailableAssets);
        }

        public static class CreateColumns
        {
            private static readonly string[] _users = { "ID", "Stage" };
            public static readonly string Users = string.Join(", ", _users.Select(x => $"{x} Number"));

            public static string Persons = "ID Number, Profession Text, Salary Number, Cash Number";

            private static readonly string[] _expenses = { "ID", "Taxes", "Mortgage", "SchoolLoan", "CarLoan", "CreditCard", "BankLoan", "Others", "Children", "PerChild" };
            public static string Expenses = string.Join(", ", _expenses.Select(x => $"{x} Number"));

            private static readonly string[] _liabilities = { "ID", "Mortgage", "SchoolLoan", "CarLoan", "CreditCard", "BankLoan" };
            public static string Liabilities = string.Join(", ", _liabilities.Select(x => $"{x} Number"));

            public static string Assets = "AssetID Number, UserID Number, Type Number, Title Text, Price Number, Qtty Number, Cost Number, Mortgage Number, CashFlow Number";

            public static string AvailableAssets = "Type Number, Value Text";
        }
    }
}

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
            catch (Exception e) { throw e; }
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
            catch (Exception e) { throw e; }

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
            catch (Exception e) { throw e; }

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
            catch (Exception e) { throw e; }

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
            catch (Exception e) { throw e; }

            return result;
        }

        private static IEnumerable<string> Columns(string sql) =>
            sql.Replace("DISTINCT", string.Empty).SubString("select", "from").Trim().Split(",");
    }

}

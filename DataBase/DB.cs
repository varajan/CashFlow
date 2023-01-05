using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using CashFlowBot.Extensions;
using MoreLinq;

namespace CashFlowBot.DataBase
{
    public static class DB
    {
        private static readonly SQLiteConnection _connection;

        static DB()
        {
            var databaseFileName = $"{AppDomain.CurrentDomain.BaseDirectory}/DB.db";

            if (!File.Exists(databaseFileName))
            {
                SQLiteConnection.CreateFile(databaseFileName);
            }

            _connection = new SQLiteConnection($"Data Source={databaseFileName}; Version=3; Cache=Shared");
            _connection = _connection.OpenAndReturn();

            Directory
                .GetFiles($"{AppDomain.CurrentDomain.BaseDirectory}/SQL")
                .Where(file => file.ToLower().EndsWith(".sql"))
                .Select(File.ReadAllText)
                .ForEach(Execute);
        }

        public static void Execute(string sql)
        {
            var cmd = new SQLiteCommand(sql, _connection);

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                e.Log(sql);
            }
            finally
            {
                cmd.Dispose();
            }
        }

        public static string GetValue(string sql)
        {
            string result = null;
            var cmd = new SQLiteCommand(sql, _connection);

            try
            {
                result = (cmd.ExecuteScalar() ?? string.Empty).ToString();
            }
            catch (Exception e)
            {
                e.Log(sql);
            }
            finally
            {
                cmd.Dispose();
            }

            return result;
        }

        public static List<string> GetColumn(string sql)
        {
            var result = new List<string>();
            var cmd = new SQLiteCommand(sql, _connection);

            try
            {
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    result.Add(reader[Columns(sql).First()].ToString());
                }
            }
            catch (Exception e)
            {
                e.Log(sql);
            }
            finally
            {
                cmd.Dispose();
            }

            return result;
        }

        public static List<List<string>> GetRows(string sql, bool toLoverCase = false)
        {
            var result = new List<List<string>>();
            var cmd = new SQLiteCommand(sql, _connection);

            try
            {
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var values = Columns(sql).Select(column => reader[column.Trim()].ToString()).ToList();

                    result.Add(toLoverCase ? values.Select(x => x.ToLower()).ToList() : values);
                }
            }
            catch (Exception e)
            {
                e.Log(sql);
            }
            finally
            {
                cmd.Dispose();
            }

            return result;
        }

        public static List<string> GetRow(string sql)
        {
            var result = new List<string>();
            var cmd = new SQLiteCommand(sql, _connection);

            try
            {
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    result = Columns(sql).Select(column => reader[column.Trim()].ToString()).ToList();
                }
            }
            catch (Exception e)
            {
                e.Log(sql);
            }
            finally
            {
                cmd.Dispose();
            }

            return result;
        }

        private static IEnumerable<string> Columns(string sql) =>
            sql.Replace("DISTINCT", string.Empty).SubString("select", "from").Trim().Split(",");

        private static void Log(this Exception ex, string sql)
        {
            Console.WriteLine($"{ex.Message}{Environment.NewLine}{sql}{Environment.NewLine}{ex.StackTrace}");
            Logger.Log(sql);
            Logger.Log(ex);
        }
    }
}

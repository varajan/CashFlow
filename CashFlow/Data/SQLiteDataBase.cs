using CashFlow.Extensions;
using CashFlow.Interfaces;
using System.Data.SQLite;

namespace CashFlow.Data;

public class SQLiteDataBase(ILogger logger) : IDataBase
{
    private readonly ILogger _logger = logger;

    private static string DatabaseFileName => $"{AppDomain.CurrentDomain.BaseDirectory}/DB.db";
    private static string ConnectionString => $"Data Source={DatabaseFileName}; Version=3; Cache=Shared";

    private static SQLiteConnection _connection;
    private SQLiteConnection Connection
    {
        get
        {
            if (_connection == null || !IsReady)
            {
                SQLiteConnection.CreateFile(DatabaseFileName);
                _connection = new SQLiteConnection(ConnectionString);
                _connection = _connection.OpenAndReturn();

                Directory
                    .GetFiles($"{AppDomain.CurrentDomain.BaseDirectory}/Data/SQL")
                    .Where(file => file.ToLower().EndsWith(".sql"))
                    .OrderBy(x => x)
                    .Select(File.ReadAllText)
                    .ToList()
                    .ForEach(sql => Execute(sql, _connection));
            }

            return _connection;
        }
    }

    private static bool IsReady => File.Exists(DatabaseFileName);

    public void Execute(string sql) => Execute(sql, Connection);
    private void Execute(string sql, SQLiteConnection connection = null)
    {
        var cmd = new SQLiteCommand(sql, connection ?? Connection);

        try
        {
            cmd.ExecuteNonQuery();
        }
        catch (Exception e)
        {
            Log(e, sql);
        }
        finally
        {
            cmd.Dispose();
        }
    }

    public string GetValue(string sql)
    {
        string result = null;
        var cmd = new SQLiteCommand(sql, Connection);

        try
        {
            result = (cmd.ExecuteScalar() ?? string.Empty).ToString();
        }
        catch (Exception e)
        {
            Log(e, sql);
        }
        finally
        {
            cmd.Dispose();
        }

        return result;
    }

    public IList<string> GetColumn(string sql)
    {
        var result = new List<string>();
        var cmd = new SQLiteCommand(sql, Connection);

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
            Log(e, sql);
        }
        finally
        {
            cmd.Dispose();
        }

        return result;
    }

    public IList<IList<string>> GetRows_OLD(string sql)
    {
        var result = new List<IList<string>>();
        var cmd = new SQLiteCommand(sql, Connection);

        try
        {
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var values = Columns(sql).Select(column => reader[column].ToString()).ToList();
                result.Add(values);
            }
        }
        catch (Exception e)
        {
            Log(e, sql);
        }
        finally
        {
            cmd.Dispose();
        }

        return result;
    }

    public IList<Dictionary<string, string>> GetRows(string sql)
    {
        var result = new List<Dictionary<string, string>>();
        var cmd = new SQLiteCommand(sql, Connection);

        try
        {
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var values = Columns(sql).ToDictionary(column => column, column => reader[column].ToString());
                result.Add(values);
            }
        }
        catch (Exception e)
        {
            Log(e, sql);
        }
        finally
        {
            cmd.Dispose();
        }

        return result;
    }

    public Dictionary<string, string> GetRow(string sql)
    {
        var result = new Dictionary<string, string>();
        var cmd = new SQLiteCommand(sql, Connection);

        try
        {
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result = Columns(sql).ToDictionary(column => column, column => reader[column].ToString());
            }
        }
        catch (Exception e)
        {
            Log(e, sql);
        }
        finally
        {
            cmd.Dispose();
        }

        return result;
    }

    private List<string> Columns(string sql)
    {
        var columns = sql.Replace("DISTINCT", string.Empty).SubString("select", "from").Trim().Split(",").Trim().ToList();

        if (columns.Count == 1 && columns[0] == "*")
        {
            var table = sql.SubString("from").Trim().Split(' ').First();
            columns = [.. GetColumn($"SELECT name FROM pragma_table_info('{table}')")];
        }

        return columns;
    }

    private void Log(Exception ex, string sql)
    {
        Console.WriteLine($"{ex.Message}{Environment.NewLine}{sql}{Environment.NewLine}{ex.StackTrace}");
        _logger.Log(sql);
        _logger.Log(ex);
    }
}
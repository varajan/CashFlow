using CashFlow.Extensions;
using CashFlow.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace CashFlow.Data;

public class SQLiteDataBase(ILogger logger) : IDataBase
{
    private static string DatabaseFileName => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DB.db");
    
    // Microsoft.Data.Sqlite uses a simpler connection string. 
    // 'Version=3' is not supported/needed.
    private static string ConnectionString => $"Data Source={DatabaseFileName};Cache=Shared";

    private static SqliteConnection? _connection;
    private SqliteConnection Connection
    {
        get
        {
            // If the file doesn't exist, we don't need CreateFile (not supported in Microsoft.Data.Sqlite).
            // Simply opening the connection will create the file if it doesn't exist.
            if (_connection == null || !File.Exists(DatabaseFileName))
            {
                _connection = new SqliteConnection(ConnectionString);
                _connection.Open();

                var initTablesCommand = @"
                CREATE TABLE IF NOT EXISTS Users (ID INTEGER, Data TEXT);
                CREATE TABLE IF NOT EXISTS Persons (ID INTEGER, PersonData TEXT);
                CREATE TABLE IF NOT EXISTS History (UserID INTEGER, Id INTEGER, HistoryRecord TEXT);";

                Execute(initTablesCommand, _connection);
            }

            if (_connection.State != System.Data.ConnectionState.Open)
            {
                _connection.Open();
            }

            return _connection;
        }
    }

    public void Execute(string sql) => Execute(sql, Connection);
    
    private void Execute(string sql, SqliteConnection connection)
    {
        using var cmd = new SqliteCommand(sql, connection);
        try
        {
            cmd.ExecuteNonQuery();
        }
        catch (Exception e)
        {
            Log(e, sql);
        }
    }

    public string GetValue(string sql)
    {
        using var cmd = new SqliteCommand(sql, Connection);
        try
        {
            var val = cmd.ExecuteScalar();
            return (val ?? string.Empty).ToString() ?? string.Empty;
        }
        catch (Exception e)
        {
            Log(e, sql);
            return string.Empty;
        }
    }

    public IList<string> GetColumn(string sql)
    {
        var result = new List<string>();
        using var cmd = new SqliteCommand(sql, Connection);
        try
        {
            using var reader = cmd.ExecuteReader();
            var columnNames = Columns(sql);
            if (columnNames.Count == 0) return result;

            while (reader.Read())
            {
                result.Add(reader[columnNames.First()]?.ToString() ?? string.Empty);
            }
        }
        catch (Exception e)
        {
            Log(e, sql);
        }
        return result;
    }

    public IList<Dictionary<string, string>> GetRows(string sql)
    {
        var result = new List<Dictionary<string, string>>();
        using var cmd = new SqliteCommand(sql, Connection);
        try
        {
            using var reader = cmd.ExecuteReader();
            var columnNames = Columns(sql);
            while (reader.Read())
            {
                var row = columnNames.ToDictionary(
                    column => column, 
                    column => reader[column]?.ToString() ?? string.Empty
                );
                result.Add(row);
            }
        }
        catch (Exception e)
        {
            Log(e, sql);
        }
        return result;
    }

    public Dictionary<string, string> GetRow(string sql)
    {
        using var cmd = new SqliteCommand(sql, Connection);
        try
        {
            using var reader = cmd.ExecuteReader();
            var columnNames = Columns(sql);
            if (reader.Read())
            {
                return columnNames.ToDictionary(
                    column => column, 
                    column => reader[column]?.ToString() ?? string.Empty
                );
            }
        }
        catch (Exception e)
        {
            Log(e, sql);
        }
        return new Dictionary<string, string>();
    }

    private List<string> Columns(string sql)
    {
        // Simple parsing logic (assuming standard SELECT ... FROM format)
        // Note: In a production environment, regex or a parser is preferred.
        try 
        {
            var selectPart = sql.SubString("select", "from").Trim();
            if (string.IsNullOrEmpty(selectPart)) return new List<string>();

            var columns = selectPart
                .Replace("DISTINCT", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Split(',')
                .Select(c => c.Trim())
                .ToList();

            if (columns.Count == 1 && columns[0] == "*")
            {
                var tablePart = sql.SubString("from").Trim().Split(' ').First();
                return GetColumn($"SELECT name FROM pragma_table_info('{tablePart}')").ToList();
            }

            return columns;
        }
        catch
        {
            return new List<string>();
        }
    }

    private void Log(Exception ex, string sql)
    {
        Console.WriteLine($"{ex.Message}{Environment.NewLine}{sql}{Environment.NewLine}{ex.StackTrace}");
        // Note: Assuming your ILogger interface or Extension has a Log(string) and Log(Exception) method
        // If it's the standard Microsoft ILogger, use logger.LogError(ex, "SQL Error: {sql}", sql);
        try 
        {
            // Adapting to common custom logger patterns used in your original snippet
            logger.Log(sql); 
            logger.Log(ex);
        }
        catch
        {
            // Fallback for logging
        }
    }
}

using CashFlowBot.DataBase;
using CashFlowBot.Extensions;

namespace CashFlowBot.Models;

public abstract class DataModel
{
    public long Id { get; init; }
    protected string Table;

    protected DataModel(long id, string table) => (Id, Table) = (id, table);

    protected string Get(string column) => DB.GetValue($"SELECT {column} FROM {Table} WHERE ID = {Id}");
    protected int GetInt(string column) => Get(column).ToInt();
    protected void Set(string column, int value) => DB.Execute($"UPDATE {Table} SET {column} = {value} WHERE ID = {Id}");
    protected void Set(string column, string value) => DB.Execute($"UPDATE {Table} SET {column} = '{value}' WHERE ID = {Id}");
}
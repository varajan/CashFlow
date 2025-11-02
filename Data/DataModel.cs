using CashFlowBot.DataBase;
using CashFlowBot.Extensions;

namespace CashFlowBot.Data;

public abstract class DataModel
{
    protected IDataBase DataBase { get; }
    public long Id { get; }
    protected string Table { get; }

    public DataModel(IDataBase dataBase, long id, string table) => (DataBase, Id, Table) = (dataBase, id, table);

    protected Terms Terms => new Terms(DataBase);

    protected string Get(string column) => DataBase.GetValue($"SELECT {column} FROM {Table} WHERE ID = {Id}");
    protected int GetInt(string column) => Get(column).ToInt();
    protected void Set(string column, int value) => DataBase.Execute($"UPDATE {Table} SET {column} = {value} WHERE ID = {Id}");
    protected void Set(string column, string value) => DataBase.Execute($"UPDATE {Table} SET {column} = '{value}' WHERE ID = {Id}");
}

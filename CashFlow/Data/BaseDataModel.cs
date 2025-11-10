using CashFlow.Data.DataBase;
using CashFlow.Extensions;

namespace CashFlow.Data;

public abstract class BaseDataModel
{
    protected IDataBase DataBase { get; }
    public long Id { get; }
    protected string Table { get; }

    public BaseDataModel(IDataBase dataBase, long id, string table) => (DataBase, Id, Table) = (dataBase, id, table);

    protected ITermsService Terms => new TermsService(DataBase);

    protected string Get(string column) => DataBase.GetValue($"SELECT {column} FROM {Table} WHERE ID = {Id}");
    protected int GetInt(string column) => Get(column).ToInt();
    protected void Set(string column, int value) => DataBase.Execute($"UPDATE {Table} SET {column} = {value} WHERE ID = {Id}");
    protected void Set(string column, string value) => DataBase.Execute($"UPDATE {Table} SET {column} = '{value}' WHERE ID = {Id}");
}

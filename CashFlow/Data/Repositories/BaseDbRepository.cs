using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Data.Repositories;

public abstract class BaseDbRepository
{
    protected IDataBase DataBase { get; }
    public long Id { get; }
    protected string Table { get; }

    public BaseDbRepository(IDataBase dataBase, long id, string table) => (DataBase, Id, Table) = (dataBase, id, table);

    protected ITermsRepository Terms => new TermsRepository(DataBase);

    protected string Get(string column) => DataBase.GetValue($"SELECT {column} FROM {Table} WHERE ID = {Id}");
    protected int GetInt(string column) => Get(column).ToInt();
    protected void Set(string column, int value) => DataBase.Execute($"UPDATE {Table} SET {column} = {value} WHERE ID = {Id}");
    protected void Set(string column, string value) => DataBase.Execute($"UPDATE {Table} SET {column} = '{value}' WHERE ID = {Id}");
}

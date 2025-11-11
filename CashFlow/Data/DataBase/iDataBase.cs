namespace CashFlow.Data.DataBase;

public interface IDataBase
{
    void Execute(string sql);
    string GetValue(string sql);
    IList<string> GetColumn(string sql);
    Dictionary<string, string> GetRow(string sql);
    IList<Dictionary<string, string>> GetRows(string sql);
    IList<IList<string>> GetRows_OLD(string sql);
}

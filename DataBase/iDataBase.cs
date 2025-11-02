using System.Collections.Generic;

namespace CashFlowBot.DataBase;

public interface IDataBase
{
    void Execute(string sql);
    string GetValue(string sql);
    IList<string> GetColumn(string sql);
    IList<string> GetRow(string sql);
    IList<IList<string>> GetRows(string sql);
}

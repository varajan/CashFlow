using CashFlow.Data.Consts;
using CashFlow.Data.Users;

namespace CashFlow.Data;

public interface ITermsService
{
    string Get(int id, Language language, string defaultValue = null, params object[] args);
    string Get(int id, IUser user, string defaultValue = null, params object[] args);
}

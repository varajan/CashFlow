using CashFlowBot.Data.Consts;
using CashFlowBot.Data.Users;

namespace CashFlowBot.Data;

public interface ITermsService
{
    string Get(int id, Language language, string defaultValue = null, params object[] args);
    string Get(int id, IUser user, string defaultValue = null, params object[] args);
}

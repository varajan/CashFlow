using CashFlowBot.Data.Users;
using System.Collections.Generic;

namespace CashFlowBot.Data;

public interface ITermsService
{
    IList<string> Get(int id);
    string Get(int id, IUser user, string defaultValue, params object[] args);
}

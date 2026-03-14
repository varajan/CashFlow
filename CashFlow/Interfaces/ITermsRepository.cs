using CashFlow.Data.Consts;

namespace CashFlow.Interfaces;

public interface ITermsRepository
{
    string Get(int id, Language language, string defaultValue = null, params object[] args);
    string Get(int id, ICashFlowUser user, string defaultValue = null, params object[] args);
}

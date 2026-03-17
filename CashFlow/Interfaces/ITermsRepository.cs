using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;

namespace CashFlow.Interfaces;

public interface ITermsRepository
{
    string Get(int id, Language language, string defaultValue = null, params object[] args);
    string Get(int id, UserDto user, string defaultValue = null, params object[] args);
    string Translate(string term, Language language = Language.EN);
}

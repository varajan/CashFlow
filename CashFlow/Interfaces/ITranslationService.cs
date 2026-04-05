using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;

namespace CashFlow.Interfaces;

public interface ITranslationService
{
    string Translate(string key, Language source, Language target, params object[] args);
    string Get(string key, Language language = Language.EN, params object[] args);
    string Get(string key, UserDto user, params object[] args);
}

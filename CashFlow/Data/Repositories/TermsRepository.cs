using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Data.Repositories;

public class TermsRepository(IDataBase dataBase) : ITermsRepository
{
    private readonly IDataBase _dataBase = dataBase;

    public string Get(int id, UserDto user, string defaultValue = null, params object[] args) => Get(id, user.Language, defaultValue, args);

    public string Get(int id, Language language, string defaultValue = null, params object[] args)
    {
        var term = _dataBase.GetValue($"SELECT Term FROM Terms WHERE ID = {id} AND Language = '{language}'").NullIfEmpty() ?? $"#{id}#{defaultValue}#";
        return string.Format(term, args);
    }
}
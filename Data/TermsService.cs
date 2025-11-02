using CashFlowBot.Data.Consts;
using CashFlowBot.Data.DataBase;
using CashFlowBot.Data.Users;
using CashFlowBot.Extensions;
using System;
using System.Collections.Generic;

namespace CashFlowBot.Data;

public class TermsService(IDataBase dataBase) : ITermsService
{
    private readonly IDataBase _dataBase = dataBase;

    public IList<string> Get(int id)
    {
        var result = new List<string>();

        foreach (Language language in Enum.GetValues(typeof(Language)))
        {
            var term = _dataBase.GetValue($"SELECT Term FROM Terms WHERE ID = {id} AND Language = '{language}'");

            result.Add(term);
        }

        return result;
    }

    public string Get(int id, IUser user, string defaultValue, params object[] args)
    {
        var term = _dataBase.GetValue($"SELECT Term FROM Terms WHERE ID = {id} AND Language = '{user.Language}'").NullIfEmpty() ?? $"#{id}#{defaultValue}#";
        return string.Format(term, args);
    }
}
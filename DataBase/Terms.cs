﻿using CashFlowBot.Data;
using CashFlowBot.Extensions;
using CashFlowBot.Models;
using System;
using System.Collections.Generic;

namespace CashFlowBot.DataBase;

public static class Terms
{
    public static List<string> Get(int id)
    {
        var result = new List<string>();

        foreach (Language language in Enum.GetValues(typeof(Language)))
        {
            var term = DB.GetValue($"SELECT Term FROM Terms WHERE ID = {id} AND Language = '{language}'");

            result.Add(term);
        }

        return result;
    }

    public static string Get(int id, long userId, string defaultValue, params object[] args) =>
        Get(id, new User(userId), defaultValue, args);

    public static string Get(int id, User user, string defaultValue, params object[] args)
    {
        var term = DB.GetValue($"SELECT Term FROM Terms WHERE ID = {id} AND Language = '{user.Language}'").NullIfEmpty() ?? $"#{id}#{defaultValue}#";
        return string.Format(term, args);
    }
}
using System.Collections.Generic;
using System.Linq;
using CashFlowBot.Extensions;
using CashFlowBot.Models;

namespace CashFlowBot.DataBase
{
    public class Users
    {
        public static List<User> AllUsers =>
            DB.GetColumn($"SELECT ID FROM {DB.Tables.Users}")
                .ToLong()
                .Select(x => new User(x))
                .ToList();
    }
}

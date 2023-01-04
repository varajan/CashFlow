using System;
using System.Collections.Generic;
using System.Linq;
using CashFlowBot.Extensions;
using CashFlowBot.Models;

namespace CashFlowBot.DataBase
{
    public class Users
    {
        public static List<string> ActiveUsersNames(User currentUser) => ActiveUsers(currentUser).Select(x => x.Name).ToList();

        public static List<User> ActiveUsers(User currentUser) =>
            AllUsers
                .Where(x => x.Id != currentUser.Id)
                .Where(x => x.Person.Exists)
                .Where(x => x.LastActive > DateTime.Now.AddMinutes(-15))
                .OrderBy(x => x.Name)
                .ToList();

        public static List<User> AllUsers =>
            DB.GetColumn("SELECT ID FROM Users")
                .ToLong()
                .Select(x => new User(x))
                .ToList();
    }
}

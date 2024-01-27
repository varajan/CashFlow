using CashFlowBot.Extensions;
using CashFlowBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CashFlowBot.DataBase;

public static class Users
{
    public static List<string> ActiveUsersNames(User currentUser, Circle circle = Circle.Both) =>
        ActiveUsers(currentUser, circle)
            .Select(x => x.Name)
            .ToList();

    public static List<User> ActiveUsers(User currentUser, Circle circle = Circle.Both) =>
        AllUsers
            .Where(x => x.Id != currentUser.Id)
            .Where(x => x.Person.Exists)
            .Where(x => x.LastActive > DateTime.Now.AddMinutes(-15))
            .OrderBy(x => x.Name)
            .Filter(circle)
            .ToList();

    public static List<User> AllUsers =>
        DB.GetColumn("SELECT ID FROM Users")
            .ToLong()
            .Select(x => new User(x))
            .ToList();

    private static IEnumerable<User> Filter(this IEnumerable<User> users, Circle circle)
    {
        return circle switch
        {
            Circle.Small => users.Where(x => !x.Person.BigCircle),
            Circle.Big => users.Where(x => x.Person.BigCircle),
            _ => users
        };
    }
}

public enum Circle
{
    Small,
    Big,
    Both
}
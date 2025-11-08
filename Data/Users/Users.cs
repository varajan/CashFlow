using CashFlowBot.Data.DataBase;
using CashFlowBot.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CashFlowBot.Data.Users;

public class Users(IDataBase dataBase, INotifyService notifyService) : IUsers
{
    private readonly IDataBase _dataBase = dataBase;

    public IList<string> GetActiveUsersNames(IUser currentUser, Circle circle = Circle.Both) =>
        GetActiveUsers(currentUser, circle)
            .Select(x => x.Name)
            .ToList();

    public IList<IUser> GetActiveUsers(IUser currentUser, Circle circle = Circle.Both) =>
        AllUsers
            .Where(x => x.Id != currentUser.Id)
            .Where(x => x.Person.Exists)
            .Where(x => x.LastActive > DateTime.Now.AddMinutes(-15))
            .OrderBy(x => x.Name)
            .Where(x => circle == Circle.Both || x.Person.Circle == circle)
            .ToList();

    public IList<IUser> AllUsers =>
        _dataBase.GetColumn("SELECT ID FROM Users")
            .ToLong()
            .Select(x => (IUser) new User(_dataBase, notifyService, x))
            .ToList();
}

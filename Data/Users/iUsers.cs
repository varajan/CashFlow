using System.Collections.Generic;

namespace CashFlowBot.Data.Users;

public interface IUsers
{
    IList<string> GetActiveUsersNames(IUser currentUser, Circle circle = Circle.Both);
    IList<IUser> GetActiveUsers(IUser currentUser, Circle circle = Circle.Both);
    IList<IUser> AllUsers { get; }
}

public enum Circle
{
    Small,
    Big,
    Both
}
using System;
using CashFlowBot.Data.Consts;
using CashFlowBot.Data.Users.UserData.HistoryData;
using CashFlowBot.Data.Users.UserData.PersonData;

namespace CashFlowBot.Data.Users;

public interface IUser
{
    long Id { get; }
    string Name { get; set; }
    bool IsAdmin { get; set; }
    bool Exists { get; }
    string Description { get; }
    Language Language { get; set; }
    Stage Stage { get; set; }
    string StageName { get; set; }
    IPerson Person { get; }
    IHistory History { get; }
    DateTime FirstLogin { get; }
    DateTime LastActive { get; set; }

    void Create();
    int PayCredit(int amount, bool regular);
    void GetCredit(int amount);
}

using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.HistoryData;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Stages;

namespace CashFlow.Data.Users;

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
    bool IsActive { get; }

    void Create();
    int PayCredit(int amount, bool regular);
    void GetCredit(int amount);

    Task SetButtons(IStage stage);
    Task Notify(string message);
}

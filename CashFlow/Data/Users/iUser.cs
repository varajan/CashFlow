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
    IPerson Person_OBSOLETE { get; }
    IHistory History_OBSOLETE { get; }
    DateTime FirstLogin { get; }
    DateTime LastActive { get; set; }
    bool IsActive { get; }

    void Create();
    void GetCredit_OBSOLETE(int amount);

    Task SetButtons(IStage stage);
    Task Notify(string message);
}

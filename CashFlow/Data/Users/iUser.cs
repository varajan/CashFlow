using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.HistoryData;
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
    IHistory History_OBSOLETE { get; }
    DateTime FirstLogin { get; }
    DateTime LastActive { get; set; }
    bool IsActive { get; }

    void Create();

    Task SetButtons(IStage stage);
    Task Notify(string message);
}

using CashFlow.Data.Consts;
using CashFlow.Stages;

namespace CashFlow.Data.Users;

public interface IUser
{
    long Id { get; }
    string Name { get; set; }
    bool Exists { get; }
    string Description { get; }
    Language Language { get; set; }
    Stage Stage { get; set; }
    string StageName { get; set; }
    bool IsActive { get; }

    void Create();

    Task SetButtons(IStage stage);
    Task Notify(string message);
}

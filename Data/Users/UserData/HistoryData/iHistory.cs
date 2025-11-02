using CashFlowBot.Data.Consts;

namespace CashFlowBot.Data.Users.UserData.HistoryData;

public interface IHistory
{
    string Description { get; }
    bool IsEmpty { get; }
    string TopFive { get; }

    void Clear();
    void Rollback();
    void Add(ActionType action, long value = 0);
    int Count(ActionType type);
}

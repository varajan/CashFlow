using CashFlow.Data.DTOs;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Interfaces;

namespace CashFlow.Stages;

public class History(ITermsService termsService, IHistoryManager historyManager, IPersonManager personManager) : BaseStage(termsService, personManager)
{
    private IHistoryManager HistoryManager { get; } = historyManager;

    public override string Message => Records.Any()
        ? string.Join(Environment.NewLine, Records.Select(x => x.Description))
        : Terms.Get(111, CurrentUser, "No records found.");

    public override IEnumerable<string> Buttons => Records.Any() ? [Rollback, Cancel] : [Cancel];

    private List<HistoryDto> Records => HistoryManager.Read(CurrentUser.Id).OrderByDescending(x => x.Id).ToList();
    private string Rollback => Terms.Get(109, CurrentUser, "Rollback last action");

    public async override Task HandleMessage(string message)
    {
        if (IsCanceled(message))
        {
            NextStage = New<Start>();
            return;
        }

        if (MessageEquals(message, 109, "Rollback last action"))
        {
            HistoryManager.Rollback(Records.First());
        }

        if (Records.Count == 0)
        {
            await CurrentUser.Notify(Message);
            NextStage = New<Start>();
            return;
        }
    }
}

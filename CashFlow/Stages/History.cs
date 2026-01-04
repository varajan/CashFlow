using CashFlow.Data.DTOs;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Interfaces;

namespace CashFlow.Stages;

public class History(ITermsService termsService, IPersonManager personManager) : BaseStage(termsService, personManager)
{
    public override string Message => Records.Any()
        ? string.Join(Environment.NewLine, Records.Select(x => x.Description))
        : Terms.Get(111, CurrentUser, "No records found.");

    public override IEnumerable<string> Buttons => Records.Any() ? [Rollback, Cancel] : [Cancel];

    private List<HistoryDto> Records => PersonManager.ReadHistory(CurrentUser);
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
            PersonManager.RollbackHistory(Records.Last());
        }

        if (Records.Count == 0)
        {
            await CurrentUser.Notify(Message);
            NextStage = New<Start>();
            return;
        }
    }
}

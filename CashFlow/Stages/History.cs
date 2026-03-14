using CashFlow.Data.DTOs;
using CashFlow.Interfaces;

namespace CashFlow.Stages;

public class History(ITermsRepository termsService, IPersonService personManager) : BaseStage(termsService, personManager)
{
    public override string Message => Records.Any()
        ? string.Join(Environment.NewLine, Records.Select(x => x.Description))
        : Terms.Get(111, CurrentUser, "No records found.");

    public override IEnumerable<string> Buttons => Records.Any() ? [Rollback, MainMenu] : [MainMenu];

    private List<HistoryDto> Records => PersonManager.ReadHistory(CurrentUser);
    private string Rollback => Terms.Get(109, CurrentUser, "Rollback last action");
    private string MainMenu => Terms.Get(102, CurrentUser, "Main menu");

    public async override Task HandleMessage(string message)
    {
        if (IsCanceled(message) || MessageEquals(message, 102, "Main menu"))
        {
            NextStage = New<Start>();
            return;
        }

        if (MessageEquals(message, 109, "Rollback last action"))
        {
            var person = PersonManager.Read(CurrentUser);

            PersonManager.RollbackHistory(person, Records.Last());
        }

        if (Records.Count == 0)
        {
            await CurrentUser.Notify(Message);
            NextStage = New<Start>();
            return;
        }
    }
}

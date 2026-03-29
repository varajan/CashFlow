using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages;

public class History(ITranslationService termsService, IPersonService personManager, IUserRepository userRepository) : BaseStage(termsService, personManager, userRepository)
{
    public override string Message => Records.Any()
        ? string.Join(Environment.NewLine, Records.Select(x => x.Description))
        : TranslationService.Get("No records found.", CurrentUser);

    public override IEnumerable<string> Buttons => Records.Any() ? [Rollback, MainMenu] : [MainMenu];

    private List<HistoryDto> Records => PersonService.ReadHistory(CurrentUser);
    private string Rollback => TranslationService.Get("Rollback last action", CurrentUser);
    private string MainMenu => TranslationService.Get("Main menu", CurrentUser);

    public async override Task HandleMessage(string message)
    {
        if (IsCanceled(message) || MessageEquals(message, "Main menu"))
        {
            NextStage = New<Start>();
            return;
        }

        if (MessageEquals(message, "Rollback last action"))
        {
            var person = PersonService.Read(CurrentUser);

            PersonService.RollbackHistory(person, Records.Last());
        }

        if (Records.Count == 0)
        {
            await CurrentUser.Notify(Message);
            NextStage = New<Start>();
            return;
        }
    }
}

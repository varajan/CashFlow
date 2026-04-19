using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Interfaces;

namespace CashFlow.Stages;

public class History(ITranslationService termsService, IUserService userService, IPersonService personManager, IUserRepository userRepository) : BaseStage(termsService, userService, personManager, userRepository)
{
    public override string Message => Records.Any()
        ? string.Join(Environment.NewLine, Records.Select(x => x.Description))
        : TranslationService.Get(Terms.NoRecords, CurrentUser);

    public override IEnumerable<string> Buttons => Records.Any() ? [Rollback, MainMenu] : [MainMenu];

    private List<HistoryDto> Records => PersonService.ReadHistory(CurrentUser);
    private string Rollback => TranslationService.Get(Terms.Rollback, CurrentUser);
    private string MainMenu => TranslationService.Get(Terms.MainMenu, CurrentUser);

    public async override Task HandleMessage(string message)
    {
        if (IsCanceled(message) || MessageEquals(message, Terms.MainMenu))
        {
            NextStage = New<Start>();
            return;
        }

        if (MessageEquals(message, Terms.Rollback))
        {
            var person = PersonService.Read(CurrentUser);

            PersonService.RollbackHistory(person, Records.Last());
        }

        if (Records.Count == 0)
        {
            await UserService.Notify(CurrentUser, Message);
            NextStage = New<Start>();
            return;
        }
    }
}

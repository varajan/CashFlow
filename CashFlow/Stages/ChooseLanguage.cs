using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Data;
using CashFlow.Extensions;

namespace CashFlow.Stages;

public class ChooseLanguage(ITermsService termsService, IPersonManager personManager) : BaseStage(termsService)
{
    private IPersonManager PersonManager { get; } = personManager;

    public override string Message => "Language/Мова";
    public override IEnumerable<string> Buttons => PersonManager.Exists(CurrentUser.Id) ? Languages.Append(Cancel) : Languages;

    private static List<string> Languages => Enum.GetValues<Language>().Select(l => l.ToString()).ToList();

    public override Task HandleMessage(string message)
    {
        if (IsCanceled(message))
        {
            NextStage = PersonManager.Exists(CurrentUser.Id) ? New<Start>() : this;
            return Task.CompletedTask;
        }

        var language = message.Trim().ToUpper();

        if (Languages.Contains(language))
        {
            CurrentUser.Language = language.ParseEnum<Language>();
            NextStage = New<Start>();
            return Task.CompletedTask;
        }

        NextStage = this;
        return Task.CompletedTask;
    }
}

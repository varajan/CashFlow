using CashFlow.Data.Consts;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages;

public class ChooseLanguage(ITranslationService termsService, IUserService userService, IPersonService personManager, IUserRepository userRepository) : BaseStage(termsService, userService, personManager, userRepository)
{
    public override string Message => "Language/Мова";
    public override IEnumerable<string> Buttons => PersonService.Exists(CurrentUser) ? Languages.Append(Cancel) : Languages;

    private static List<string> Languages => Enum.GetValues<Language>().Select(l => l.ToString()).ToList();

    public override Task HandleMessage(string message)
    {
        if (IsCanceled(message))
        {
            NextStage = PersonService.Exists(CurrentUser) ? New<Start>() : this;
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

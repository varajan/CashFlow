using CashFlow.Extensions;
using CashFlow.Interfaces;
using CashFlow.Stages.SmallCircleStages;

namespace CashFlow.Stages;

public class ChooseProfession(ITranslationService termsService, IPersonService personManager, IUserRepository userRepository)
    : BaseStage(termsService, personManager, userRepository)
{
    public override string Message => TranslationService.Get("Choose your *profession*", CurrentUser);
    public override IEnumerable<string> Buttons => Professions.Append(TranslationService.Get(Terms.PickRandom, CurrentUser));

    private IEnumerable<string> Professions => PersonService.GetAllProfessions()
        .Select(x => TranslationService.Get(x, CurrentUser.Language))
        .OrderBy(x => x);

    public override Task HandleMessage(string message)
    {
        if (IsCanceled(message))
        {
            NextStage = this;
            return Task.CompletedTask;
        }

        var random = MessageEquals(message, Terms.PickRandom);
        var profession = random
            ? Professions.Random()
            : Professions.FirstOrDefault(p => p.Equals(message.Trim(), StringComparison.OrdinalIgnoreCase));

        if (profession is not null)
        {
            PersonService.Create(profession, CurrentUser);
            NextStage = New<SmallCircle>();
            return Task.CompletedTask;
        }

        NextStage = this;
        return Task.CompletedTask;
    }
}

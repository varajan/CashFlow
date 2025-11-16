using CashFlow.Data;
using CashFlow.Extensions;
using CashFlow.Stages.SmallCircleStages;
using System.Text;

namespace CashFlow.Stages;

public class ChooseProfession(ITermsService termsService) : BaseStage(termsService)
{
    public override string Message => Terms.Get(28, CurrentUser, "Choose your *profession*");
    public override List<string> Buttons => Professions;

    private List<string> Professions => Persons.GetAll()
        .Select(x => x.Profession[CurrentUser.Language])
        .OrderBy(x => x)
        .Append(Terms.Get(139, CurrentUser, "Random"))
        .ToList();

    public override Task HandleMessage(string message)
    {
        if (IsCanceled(message))
        {
            NextStage = this;
            return Task.CompletedTask;
        }

        var random = MessageEquals(message, 139, "Random");
        var profession = random
            ? Professions.Random()
            : Professions.FirstOrDefault(p => p.Equals(message.Trim(), StringComparison.OrdinalIgnoreCase));

        if (profession is not null)
        {
            CurrentUser.Person_OBSOLETE.Create(profession);
            NextStage = New<SmallCircle>();
            return Task.CompletedTask;
        }

        NextStage = this;
        return Task.CompletedTask;
    }
}

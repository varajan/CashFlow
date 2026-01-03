using CashFlow.Data;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Extensions;
using CashFlow.Interfaces;
using CashFlow.Stages.SmallCircleStages;
using System.Text;

namespace CashFlow.Stages;

public class ChooseProfession(ITermsService termsService, IPersonManager personManager) : BaseStage(termsService, personManager)
{
    public override string Message => Terms.Get(28, CurrentUser, "Choose your *profession*");
    public override IEnumerable<string> Buttons => Professions.Append(Terms.Get(139, CurrentUser, "Random"));

    private IEnumerable<string> Professions => Persons.GetAll()
        .Select(x => x.Profession[CurrentUser.Language])
        .OrderBy(x => x);

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
            PersonManager.Create(profession, CurrentUser);
            NextStage = New<SmallCircle>();
            return Task.CompletedTask;
        }

        NextStage = this;
        return Task.CompletedTask;
    }
}

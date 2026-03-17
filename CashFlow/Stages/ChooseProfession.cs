using CashFlow.Data;
using CashFlow.Data.Repositories;
using CashFlow.Extensions;
using CashFlow.Interfaces;
using CashFlow.Stages.SmallCircleStages;
using System.Text;

namespace CashFlow.Stages;

public class ChooseProfession(ITermsRepository termsService, IPersonService personManager, IUserRepository userRepository)
    : BaseStage(termsService, personManager, userRepository)
{
    public override string Message => Terms.Get(28, CurrentUser, "Choose your *profession*");
    public override IEnumerable<string> Buttons => Professions.Append(Terms.Get(139, CurrentUser, "Random"));

    private IEnumerable<string> Professions => PersonService.GetAllProfessions()
        .Select(x => Terms.Translate(x, CurrentUser.Language))
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
            PersonService.Create(profession, CurrentUser);
            NextStage = New<SmallCircle>();
            return Task.CompletedTask;
        }

        NextStage = this;
        return Task.CompletedTask;
    }
}

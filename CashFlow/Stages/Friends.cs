using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages;

public class Friends(ITermsRepository termsService, IPersonService personManager, IUserRepository userRepository) : BaseStage(termsService, personManager, userRepository)
{
    private IList<UserDto> ActiveUsers => OtherUsers.Where(x => x.IsActive()).ToList();

    public override string Message
    {
        get
        {
            var NL = Environment.NewLine;
            var message = string.Empty;
            var onSmall = Terms.Get(142, CurrentUser, "On Small circle:");
            var onBig = Terms.Get(143, CurrentUser, "On Big circle:");

            var onSmallCircle = ActiveUsers.Where(x => PersonService.Read(x).BigCircle == false).ToList();
            var onBigCircle = ActiveUsers.Where(x => PersonService.Read(x).BigCircle == true).ToList();

            if (onSmallCircle.Any()) message += $"*{onSmall}*{NL}{string.Join("", onSmallCircle.Select(x => $"• {x.Name.Escape()}{NL}"))}{NL}";
            if (onBigCircle.Any()) message += $"*{onBig}*{NL}{string.Join("", onBigCircle.Select(x => $"• {x.Name.Escape()}{NL}"))}";

            return message;
        }
    }

    public override IEnumerable<string> Buttons => ActiveUsers.Select(x => x.Name).Append(Cancel);

    public async override Task HandleMessage(string message)
    {
        if (IsCanceled(message))
        {
            NextStage = New<Start>();
            return;
        }

        var friend = ActiveUsers.FirstOrDefault(x => x.Name.Equals(message, StringComparison.InvariantCultureIgnoreCase));
        if (friend is null) return;

        var description = PersonService.GetDescription(friend);
        var topFive = PersonService.HistoryTopFive(friend, CurrentUser);

        await CurrentUser.Notify(description);
        await CurrentUser.Notify(topFive);
    }
}

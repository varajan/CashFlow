using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages;

public class Friends(ITermsService termsService) : BaseStage(termsService)
{
    public override string Message
    {
        get
        {
            var message = string.Empty;
            var onSmall = Terms.Get(142, CurrentUser, "On Small circle:");
            var onBig = Terms.Get(143, CurrentUser, "On Big circle:");

            var onSmallCircle = OtherUsers.Where(x => x.IsActive && x.Person_OBSOLETE.Circle == Circle.Small).ToList();
            var onBigCircle = OtherUsers.Where(x => x.IsActive && x.Person_OBSOLETE.Circle == Circle.Big).ToList();

            if (onSmallCircle.Any()) message += $"*{onSmall}*\r\n{string.Join("", onSmallCircle.Select(x => $"• {x.Name.Escape()}\r\n"))}\r\n";
            if (onBigCircle.Any()) message += $"*{onBig}* \r\n{string.Join("", onBigCircle.Select(x => $"• {x.Name.Escape()}\r\n"))}";

            return message;
        }
    }

    public override IEnumerable<string> Buttons => OtherUsers.Where(x => x.IsActive).Select(x => x.Name).Append(Cancel);

    public async override Task HandleMessage(string message)
    {
        var friend = OtherUsers.FirstOrDefault(x => x.Name == message);
        if (friend is null) return;

        await CurrentUser.Notify(friend.Person_OBSOLETE.BigCircle ? friend.Person_OBSOLETE.Description : friend.Description);
        await CurrentUser.Notify(friend.History.TopFive);
    }
}
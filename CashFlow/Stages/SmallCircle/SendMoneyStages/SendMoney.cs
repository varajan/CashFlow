using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.Users;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Extensions;
using CashFlow.Loggers;

namespace CashFlow.Stages;

public class SendMoney(IList<IUser> otherUsers, IUser currentUser, ITermsService termsService, ILogger logger, IAvailableAssets assets) : BaseStage(otherUsers, currentUser, termsService, logger, assets)
{
    public override string Message => Terms.Get(147, CurrentUser, "Whom?");

    public override IEnumerable<string> Buttons
    {
        get
        {
            CurrentUser.Person.Assets.Get(AssetType.Transfer)?.Delete();

            var bank = Terms.Get(149, CurrentUser, "Bank");
            var users = OtherUsers.Where(x => x.IsActive && x.Person.Circle == Circle.Small).Select(x => x.Name).ToList();

            return users.Append(bank).Append(Cancel);
        }
    }

    public async override Task HandleMessage(string message)
    {
        if (IsCanceled(message)) return;

        if (MessageEquals(message, 149, "Bank") || OtherUsers.Any(x => x.IsActive && x.Person.Circle == Circle.Small && x.Name == message))
        {
            CurrentUser.Person.Assets.Add(message, AssetType.Transfer);
            NextStage = New<SendMoneyAmount>();
            return;
        }

        await CurrentUser.Notify(Terms.Get(145, CurrentUser, "Not found."));
    }
}

public class SendMoneyAmount(IList<IUser> otherUsers, IUser currentUser, ITermsService termsService, ILogger logger, IAvailableAssets assets) : BaseStage(otherUsers, currentUser, termsService, logger, assets)
{
    public override string Message => Terms.Get(21, CurrentUser, "How much?");

    public override IEnumerable<string> Buttons
    {
        get
        {
            var cancel = Cancel;
            return Enumerable.Range(1, 8)
                .Select(x => (500 * x).AsCurrency())
                .Append(cancel);
        }
    }

    protected Asset_OLD TransferAsset => CurrentUser.Person.Assets.Get(AssetType.Transfer);

    public override async Task HandleMessage(string message)
    {
        if (IsCanceled(message))
        {
            NextStage = New<Start>();
            return;
        }

        var amount = message.AsCurrency();

        if (amount <= 0)
        {
            await CurrentUser.Notify(Terms.Get(150, CurrentUser, "Invalid value. Try again."));
            return;
        }

        TransferAsset.Qtty = amount;
        //CurrentUser.Person.Assets.Transfer.Qtty = amount;
        if (CurrentUser.Person.Cash < amount)
        {
            NextStage = New<SendMoneyCredit>();
            return;
        }

        await Transfer();
    }

    protected async Task Transfer()
    {
        var bank = Terms.Get(149, CurrentUser, "Bank");
        var to = TransferAsset.Title; // ISSUE
        var amount = TransferAsset.Qtty; // ISSUE
        //var to = CurrentUser.Person.Assets.Transfer.Title; // ISSUE
        //var amount = CurrentUser.Person.Assets.Transfer.Qtty; // ISSUE
        var friend = OtherUsers.FirstOrDefault(x => x.Name == to);
        var message = Terms.Get(146, CurrentUser, "{0} transferred {2} to {1}.", CurrentUser.Name, friend?.Name ?? bank, amount.AsCurrency(), Environment.NewLine);
        var users = OtherUsers
                .Where(x => x.IsActive)
                .Append(CurrentUser)
                .ToList();

        CurrentUser.Person.Cash -= amount;
        CurrentUser.History.Add(ActionType.PayMoney, amount);

        if (friend is not null)
        {
            friend.Person.Cash += amount;
            friend.History.Add(ActionType.GetMoney, amount);
        }

        TransferAsset?.Delete(); // ISSUE
        //CurrentUser.Person.Assets.Transfer.Delete(); // ISSUE

        var notifyAll = users.Select(u => u.Notify(message));
        await Task.WhenAll(notifyAll);
        NextStage = New<Start>();
    }
}

public class SendMoneyCredit(IList<IUser> otherUsers, IUser currentUser, ITermsService termsService, ILogger logger, IAvailableAssets assets) : SendMoneyAmount(otherUsers, currentUser, termsService, logger, assets)
{
    public override string Message
    {
        get
        {
            //var value = CurrentUser.Person.Assets.Transfer.Qtty.AsCurrency();
            var value = TransferAsset.Qtty.AsCurrency();
            var cash = CurrentUser.Person.Cash.AsCurrency();
            return Terms.Get(23, CurrentUser, "You don''t have {0}, but only {1}", value, cash);
        }
    }

    public override IEnumerable<string> Buttons => [Terms.Get(34, CurrentUser, "Get Credit"), Cancel];

    public override async Task HandleMessage(string message)
    {
        switch (message)
        {
            case var m when MessageEquals(m, 6, "Cancel"):
                NextStage = New<Start>();
                return;

            case var m when MessageEquals(m, 34, "Get Credit"):
                var delta = TransferAsset.Qtty - CurrentUser.Person.Cash;
                //var delta = CurrentUser.Person.Assets.Transfer.Qtty - CurrentUser.Person.Cash;
                var credit = (int)Math.Ceiling(delta / 1_000d) * 1_000;
                CurrentUser.GetCredit(credit);
                await Transfer();

                NextStage = New<Start>();
                return;
        }
    }
}

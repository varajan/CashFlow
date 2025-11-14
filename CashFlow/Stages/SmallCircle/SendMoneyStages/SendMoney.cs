using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Extensions;

namespace CashFlow.Stages;

public class SendMoney(IAssetManager assetManager, ITermsService termsService, IAvailableAssets assets) : BaseStage(termsService, assets)
{
    private IAssetManager AssetManager { get; init; } = assetManager;

    public override string Message => Terms.Get(147, CurrentUser, "Whom?");

    public override IEnumerable<string> Buttons
    {
        get
        {
            var asset = AssetManager.Read(AssetType.Transfer, CurrentUser.Id);
            AssetManager.Delete(asset);

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
            var transfer = new AssetDto
            {
                Title = message,
                UserId = CurrentUser.Id,
                Type = AssetType.Transfer,
            };

            AssetManager.Create(transfer);
            NextStage = New<SendMoneyAmount>();
            return;
        }

        await CurrentUser.Notify(Terms.Get(145, CurrentUser, "Not found."));
    }
}

public class SendMoneyAmount(
    IAssetManager assetManager,
    IPersonManager personManager,
    IHistoryManager historyManager,
    ITermsService termsService,
    IAvailableAssets assets) : BaseStage(termsService, assets)
{
    protected IAssetManager AssetManager { get; init; } = assetManager;
    protected IPersonManager PersonManager { get; init; } = personManager;
    private IHistoryManager HistoryManager { get; init; } = historyManager;

    public override string Message => Terms.Get(21, CurrentUser, "How much?");

    public override IEnumerable<string> Buttons => Enumerable
        .Range(1, 8)
        .Select(x => (500 * x).AsCurrency())
        .Append(Cancel);

    public override async Task HandleMessage(string message)
    {
        var asset = AssetManager.Read(AssetType.Transfer, CurrentUser.Id);

        if (IsCanceled(message))
        {
            AssetManager.Delete(asset);
            NextStage = New<Start>();
            return;
        }

        var amount = message.AsCurrency();

        if (amount <= 0)
        {
            await CurrentUser.Notify(Terms.Get(150, CurrentUser, "Invalid value. Try again."));
            return;
        }

        asset.Qtty = amount;
        AssetManager.Update(asset);

        var currentUserPerson = PersonManager.Read(CurrentUser.Id);
        if (currentUserPerson.Cash < amount)
        {
            NextStage = New<SendMoneyCredit>();
            return;
        }

        await Transfer(asset);
    }

    protected async Task Transfer(AssetDto asset)
    {
        var bank = Terms.Get(149, CurrentUser, "Bank");
        var amount = asset.Qtty;
        var friend = OtherUsers.FirstOrDefault(x => x.Name == asset.Title);
        var message = Terms.Get(146, CurrentUser, "{0} transferred {2} to {1}.",
            CurrentUser.Name,
            friend?.Name ?? bank,
            amount.AsCurrency());
        var users = OtherUsers
                .Where(x => x.IsActive)
                .Append(CurrentUser)
                .ToList();

        var currentUserPerson = PersonManager.Read(CurrentUser.Id);
        currentUserPerson.Cash -= amount;
        PersonManager.Update(currentUserPerson);
        HistoryManager.Add(ActionType.PayMoney, amount, CurrentUser);

        if (friend is not null)
        {
            var friendPerson = PersonManager.Read(friend.Id);
            friendPerson.Cash += amount;
            PersonManager.Update(friendPerson);
            HistoryManager.Add(ActionType.GetMoney, amount, friend);
        }

        AssetManager.Delete(asset);

        var notifyAll = users.Select(u => u.Notify(message));
        await Task.WhenAll(notifyAll);
        NextStage = New<Start>();
    }
}

public class SendMoneyCredit(
    IAssetManager assetManager,
    IPersonManager personManager,
    IHistoryManager historyManager,
    ITermsService termsService,
    IAvailableAssets assets) : SendMoneyAmount(assetManager, personManager, historyManager, termsService, assets)
{
    public override string Message
    {
        get
        {
            var asset = AssetManager.Read(AssetType.Transfer, CurrentUser.Id);
            var currentUserPerson = PersonManager.Read(CurrentUser.Id);
            var value = asset.Qtty.AsCurrency();
            var cash = currentUserPerson.Cash.AsCurrency();
            return Terms.Get(23, CurrentUser, "You don''t have {0}, but only {1}", value, cash);
        }
    }

    public override IEnumerable<string> Buttons => [Terms.Get(34, CurrentUser, "Get Credit"), Cancel];

    public override async Task HandleMessage(string message)
    {
        var asset = AssetManager.Read(AssetType.Transfer, CurrentUser.Id);

        switch (message)
        {
            case var m when MessageEquals(m, 6, "Cancel"):
                AssetManager.Delete(asset);
                NextStage = New<Start>();
                return;

            case var m when MessageEquals(m, 34, "Get Credit"):
                var currentUserPerson = PersonManager.Read(CurrentUser.Id);
                var delta = asset.Qtty - currentUserPerson.Cash;
                var credit = (int)Math.Ceiling(delta / 1_000d) * 1_000;
                CurrentUser.GetCredit(credit);
                await Transfer(asset);

                NextStage = New<Start>();
                return;
        }
    }
}

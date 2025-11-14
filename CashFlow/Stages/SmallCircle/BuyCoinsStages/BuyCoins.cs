using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Data;
using CashFlow.Extensions;
using System.Text;

namespace CashFlow.Stages;

public class BuyCoins(ITermsService termsService, IAvailableAssets availableAssets) : BaseStage(termsService)
{
    protected IAvailableAssets AvailableAssets { get; } = availableAssets;

    public override string Message => Terms.Get(7, CurrentUser, "Title:");

    public override IEnumerable<string> Buttons => AvailableAssets.GetAsText(AssetType.CoinTitle, CurrentUser.Language).Append(Cancel);

    public override Task HandleMessage(string message)
    {
        var coinTitle = AvailableAssets
            .GetAsText(AssetType.CoinTitle, CurrentUser.Language)
            .FirstOrDefault(x => x.Equals(message, StringComparison.InvariantCultureIgnoreCase));

        if (coinTitle is not null)
        {
            CurrentUser.Person.Assets.Add(coinTitle, AssetType.Coin);
            NextStage = New<BuyCoinsCount>();
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}

public class BuyCoinsCount(ITermsService termsService, IAvailableAssets availableAssets) : BuyCoins(termsService, availableAssets)
{
    public override string Message => Terms.Get(21, CurrentUser, "How much?");

    public override IEnumerable<string> Buttons => AvailableAssets
        .GetAsText(AssetType.CoinCount, CurrentUser.Language)
        .Append(Cancel);

    public async override Task HandleMessage(string message)
    {
        var number = message.AsCurrency();

        if (number <= 0)
        {
            await CurrentUser.Notify(Terms.Get(18, CurrentUser, "Invalid quantity value. Try again."));
            return;
        }

        CurrentUser.Person.Assets.Coins.First(a => a.IsDraft).Qtty = number;
        NextStage = New<BuyCoinsPrice>();
    }
}

public class BuyCoinsPrice(ITermsService termsService, IAvailableAssets availableAssets) : BuyCoins(termsService, availableAssets)
{
    protected Asset_OLD Asset => CurrentUser.Person.Assets.Coins.First(a => a.IsDraft);
    public override string Message => Terms.Get(8, CurrentUser, "What is the price?");
    public override IEnumerable<string> Buttons => AvailableAssets.GetAsCurrency(AssetType.CoinBuyPrice).Append(Cancel);

    public override async Task HandleMessage(string message)
    {
        var number = message.AsCurrency();

        if (number <= 0)
        {
            await CurrentUser.Notify(Terms.Get(9, CurrentUser, "Invalid price value. Try again."));
            return;
        }

        Asset.Price = number;

        if (CurrentUser.Person.Cash < Asset.Price * Asset.Qtty)
        {
            NextStage = New<BuyCoinsCredit>();
            return;
        }

        await CompleteTransaction();
        NextStage = New<Start>();
    }

    protected async Task CompleteTransaction()
    {
        CurrentUser.Person.Cash -= Asset.Price * Asset.Qtty;
        CurrentUser.History.Add(ActionType.BuyCoins, Asset.Id);
        Asset.IsDraft = false;
        await CurrentUser.Notify(Terms.Get(13, CurrentUser, "Done."));
    }
}

public class BuyCoinsCredit(ITermsService termsService, IAvailableAssets assets) : BuyCoinsPrice(termsService, assets)
{
    public override string Message
    {
        get
        {
            var value = (Asset.Qtty * Asset.Price).AsCurrency();
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
                var delta = Asset.Price * Asset.Qtty - CurrentUser.Person.Cash;
                var credit = (int)Math.Ceiling(delta / 1_000d) * 1_000;

                CurrentUser.GetCredit(credit);
                await CompleteTransaction();

                NextStage = New<Start>();
                return;
        }
    }
}
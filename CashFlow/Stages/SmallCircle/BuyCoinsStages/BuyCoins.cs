using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Data;
using CashFlow.Extensions;
using System.Text;
using CashFlow.Data.DTOs;

namespace CashFlow.Stages;

public class BuyCoins(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IAssetManager assetManager)
    : BaseStage(termsService)
{
    protected IAvailableAssets AvailableAssets { get; } = availableAssets;
    protected IAssetManager AssetManager { get; } = assetManager;

    public override string Message => Terms.Get(7, CurrentUser, "Title:");

    public override IEnumerable<string> Buttons => AvailableAssets.GetAsText(AssetType.CoinTitle, CurrentUser.Language).Append(Cancel);

    public override Task HandleMessage(string message)
    {
        var asset = AssetManager.ReadAll(AssetType.Coin, CurrentUser.Id).FirstOrDefault(x => x.IsDraft);
        AssetManager.Delete(asset);

        if (IsCanceled(message))
        {
            NextStage = New<Start>();
            return Task.CompletedTask;
        }

        var coinTitle = AvailableAssets
            .GetAsText(AssetType.CoinTitle, CurrentUser.Language)
            .FirstOrDefault(x => x.Equals(message, StringComparison.InvariantCultureIgnoreCase));

        if (coinTitle is not null)
        {
            var draftCoinAsset = new AssetDto
            {
                Title = coinTitle,
                BigCircle = false,
                Type = AssetType.Coin,
                UserId = CurrentUser.Id,
                IsDraft = true,
            };

            AssetManager.Create(draftCoinAsset);
            NextStage = New<BuyCoinsCount>();
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}

public class BuyCoinsCount(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IAssetManager assetManager) : BuyCoins(termsService, availableAssets, assetManager)
{
    public override string Message => Terms.Get(21, CurrentUser, "How much?");

    public override IEnumerable<string> Buttons => AvailableAssets
        .GetAsText(AssetType.CoinCount, CurrentUser.Language)
        .Append(Cancel);

    public async override Task HandleMessage(string message)
    {
        if (IsCanceled(message))
        {
            NextStage = New<Start>();
            return;
        }

        var number = message.AsCurrency();

        if (number <= 0)
        {
            await CurrentUser.Notify(Terms.Get(18, CurrentUser, "Invalid quantity value. Try again."));
            return;
        }

        var asset = AssetManager.ReadAll(AssetType.Coin, CurrentUser.Id).First(x => x.IsDraft);
        asset.Qtty = number;
        AssetManager.Update(asset);

        NextStage = New<BuyCoinsPrice>();
    }
}

public class BuyCoinsPrice(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IHistoryManager historyManager,
    IPersonManager personManager,
    IAssetManager assetManager) : BuyCoins(termsService, availableAssets, assetManager)
{
    protected IHistoryManager HistoryManager { get; } = historyManager;
    protected IPersonManager PersonManager { get; } = personManager;

    public override string Message => Terms.Get(8, CurrentUser, "What is the price?");
    public override IEnumerable<string> Buttons => AvailableAssets.GetAsCurrency(AssetType.CoinBuyPrice).Append(Cancel);

    public override async Task HandleMessage(string message)
    {
        if (IsCanceled(message))
        {
            NextStage = New<Start>();
            return;
        }

        var number = message.AsCurrency();

        if (number <= 0)
        {
            await CurrentUser.Notify(Terms.Get(9, CurrentUser, "Invalid price value. Try again."));
            return;
        }

        var asset = AssetManager.ReadAll(AssetType.Coin, CurrentUser.Id).Single(x => x.IsDraft);
        asset.Price = number;
        AssetManager.Update(asset);

        var person = PersonManager.Read(CurrentUser.Id);
        if (person.Cash < asset.Price * asset.Qtty)
        {
            NextStage = New<BuyCoinsCredit>();
            return;
        }

        await CompleteTransaction();
        NextStage = New<Start>();
    }

    protected async Task CompleteTransaction()
    {
        var asset = AssetManager.ReadAll(AssetType.Coin, CurrentUser.Id).First(x => x.IsDraft);
        var person = PersonManager.Read(CurrentUser.Id);

        person.Cash -= asset.Price * asset.Qtty;
        PersonManager.Update(person);
        
        asset.IsDraft = false;
        AssetManager.Update(asset);
        
        HistoryManager.Add(ActionType.BuyCoins, asset.Id, CurrentUser);

        await CurrentUser.Notify(Terms.Get(13, CurrentUser, "Done."));
    }
}

public class BuyCoinsCredit(
    ITermsService termsService,
    IAvailableAssets assets,
    IHistoryManager historyManager,
    IPersonManager personManager,
    IAssetManager assetManager) : BuyCoinsPrice(termsService, assets, historyManager, personManager, assetManager)
{
    public override string Message
    {
        get
        {
            var asset = AssetManager.ReadAll(AssetType.Coin, CurrentUser.Id).First(x => x.IsDraft);
            var value = (asset.Qtty * asset.Price).AsCurrency();
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
                var asset = AssetManager.ReadAll(AssetType.Coin, CurrentUser.Id).First(x => x.IsDraft);
                var delta = asset.Price * asset.Qtty - CurrentUser.Person.Cash;
                var credit = (int)Math.Ceiling(delta / 1_000d) * 1_000;

                CurrentUser.GetCredit(credit);
                await CompleteTransaction();

                NextStage = New<Start>();
                return;
        }
    }
}
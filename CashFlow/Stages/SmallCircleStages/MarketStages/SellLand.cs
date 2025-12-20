using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.MarketStages;

public class SellLand(ITermsService termsService, IAssetManager assetManager) : SellAsset<SellLandPrice>(termsService, assetManager, AssetType.Land) { }

public class SellLandPrice(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IAssetManager assetManager,
    IPersonManager personManager,
    IHistoryManager historyManager) : SellAssetPrice(termsService, availableAssets, assetManager, personManager, historyManager, AssetType.Land) { }

public class SellRealEstate(ITermsService termsService) : BaseStage(termsService)
{
}

public class SellBusiness(ITermsService termsService, IAssetManager assetManager) :
    SellAsset<SellBusinessPrice>(termsService, assetManager, AssetType.Business, AssetType.SmallBusiness) { }

public class SellBusinessPrice(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IAssetManager assetManager,
    IPersonManager personManager,
    IHistoryManager historyManager) : SellAssetPrice(termsService, availableAssets, assetManager, personManager, historyManager, AssetType.Business, AssetType.SmallBusiness)
{ }

public class SellCoins(ITermsService termsService, IAssetManager assetManager) : SellAsset<SellCoinsPrice>(termsService, assetManager, AssetType.Coin) { }

public class SellCoinsPrice(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IAssetManager assetManager,
    IPersonManager personManager,
    IHistoryManager historyManager) : SellAssetPrice(termsService, availableAssets, assetManager, personManager, historyManager, AssetType.Coin)
{ }

public class IncreaseCashflow(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IAssetManager assetManager,
    IHistoryManager historyManager) : BaseStage(termsService)
{
    protected IAvailableAssets AvailableAssets { get; } = availableAssets;
    protected IAssetManager AssetManager { get; } = assetManager;
    protected IHistoryManager HistoryManager { get; } = historyManager;

    public override string Message => Terms.Get(12, CurrentUser, "What is the cash flow?");

    public override IEnumerable<string> Buttons => AvailableAssets.GetAsCurrency(AssetType.IncreaseCashFlow).Append(Cancel);

    public override async Task HandleMessage(string message)
    {
        if (IsCanceled(message))
        {
            NextStage = New<Start>();
            return;
        }

        var cashflow = message.AsCurrency();
        if (cashflow <= 0)
        {
            await CurrentUser.Notify(Terms.Get(150, CurrentUser, "Invalid value. Try again."));
            return;
        }

        var assets = AssetManager.ReadAll(AssetType.SmallBusiness, CurrentUser.Id);
        assets.ForEach(asset =>
        {
            asset.CashFlow += cashflow;
            AssetManager.Update(asset);
            HistoryManager.Add(ActionType.IncreaseCashFlow, asset.Id, CurrentUser);
        });

        await CurrentUser.Notify(Terms.Get(13, CurrentUser, "Done."));
        NextStage = New<Start>();
    }
}

public class SellAsset<TNextStage>(
    ITermsService termsService,
    IAssetManager assetManager,
    params AssetType[] assetTypes)
    : BaseStage(termsService) where TNextStage : BaseStage
{
    protected AssetType[] AssetTypes { get; } = assetTypes;
    protected IAssetManager AssetManager { get; } = assetManager;

    public override string Message
    {
        get
        {
            var assetNames = Assets.Select((a, i) => $"*#{i+1}* {AssetManager.GetDescription(a, CurrentUser)}").Join(Environment.NewLine);

            if (AssetTypes.Contains(AssetType.Land))
            {
                return Terms.Get(99, CurrentUser, "What Land do you want to sell?{0}{1}", Environment.NewLine, assetNames);
            }

            if (AssetTypes.ContainsAny(AssetType.Business, AssetType.SmallBusiness))
            {
                return Terms.Get(99, CurrentUser, "What Business do you want to sell?{0}{1}", Environment.NewLine, assetNames);
            }

            if (AssetTypes.Contains(AssetType.Coin))
            {
                return Terms.Get(122, CurrentUser, "What coins do you want to sell?", Environment.NewLine, assetNames);
            }

            if (AssetTypes.Contains(AssetType.Stock))
            {
                return Terms.Get(27, CurrentUser, "What stocks do you want to sell?", Environment.NewLine, assetNames);
            }

            throw new NotImplementedException();
        }
    }

    public override IEnumerable<string> Buttons
    {
        get
        {
            if (AssetTypes.ContainsAny(AssetType.Stock, AssetType.Coin))
            {
                return Assets.Select(a => a.Title).Distinct().Append(Cancel);
            }

            return Assets.Select((l, i) => $"#{i + 1}").Append(Cancel);
        }
    }

    private List<AssetDto> Assets => AssetTypes.SelectMany(type => AssetManager.ReadAll(type, CurrentUser.Id)).ToList();

    public override async Task HandleMessage(string message)
    {
        if (IsCanceled(message))
        {
            NextStage = New<Start>();
            return;
        }

        var moveNext = AssetTypes.ContainsAny(AssetType.Land, AssetType.Business, AssetType.SmallBusiness)
            ? await HandleByIndex(message)
            : await HandleByTitle(message);

        if (moveNext)
        {
            NextStage = New<TNextStage>();
        }
    }

    private async Task<bool> HandleByIndex(string message)
    {
        var index = message.Replace("#", "").ToInt();
        if (index < 1 || index > Assets.Count)
        {
            if (AssetTypes.Contains(AssetType.Land))
            {
                await CurrentUser.Notify(Terms.Get(101, CurrentUser, "Invalid land number."));
                return false;
            }

            if (AssetTypes.Contains(AssetType.Land) || AssetTypes.Contains(AssetType.SmallBusiness))
            {
                await CurrentUser.Notify(Terms.Get(76, CurrentUser, "Invalid business number."));
                return false;
            }

            throw new NotImplementedException();
        }

        var asset = Assets[index - 1];
        asset.MarkedToSell = true;
        AssetManager.Update(asset);
        return true;
    }

    private async Task<bool> HandleByTitle(string message)
    {
        var assets = Assets
            .Where(x => x.Title.Equals(message, StringComparison.InvariantCultureIgnoreCase))
            .ToList();

        if (assets.Count == 0)
        {
            if (AssetTypes.Contains(AssetType.Coin))
            {
                await CurrentUser.Notify(Terms.Get(123, CurrentUser, "Invalid coins title."));
                return false;
            }

            if (AssetTypes.Contains(AssetType.Stock))
            {
                await CurrentUser.Notify(Terms.Get(124, CurrentUser, "Invalid stocks name."));
                return false;
            }

            throw new NotImplementedException();
        }

        assets.ForEach(asset =>
        {
            asset.MarkedToSell = true;
            AssetManager.Update(asset);
        });

        return true;
    }
}

public class SellAssetPrice(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IAssetManager assetManager,
    IPersonManager personManager,
    IHistoryManager historyManager,
    params AssetType[] assetTypes) : BaseStage(termsService)
{
    protected AssetType[] AssetTypes { get; } = assetTypes;

    protected ActionType ActionType => AssetTypes.First() switch
    {
        AssetType.Land => ActionType.SellLand,
        AssetType.Coin => ActionType.SellCoins,
        AssetType.Business => ActionType.SellBusiness,
        AssetType.SmallBusiness => ActionType.SellBusiness,
        AssetType.Stock => ActionType.SellStocks,

        _ => throw new NotImplementedException(),
    };

    protected AssetType SellPrice => AssetTypes.First() switch
    {
        AssetType.Land => AssetType.LandSellPrice,
        AssetType.Coin => AssetType.CoinSellPrice,
        AssetType.Business => AssetType.BusinessSellPrice,
        AssetType.SmallBusiness => AssetType.BusinessSellPrice,
        AssetType.Stock => AssetType.StockPrice,

        _ => throw new NotImplementedException(),
    };

    protected IAvailableAssets AvailableAssets { get; } = availableAssets;
    protected IAssetManager AssetManager { get; } = assetManager;
    private IPersonManager PersonManager {  get; } = personManager;
    protected IHistoryManager HistoryManager { get; } = historyManager;

    public override string Message => Terms.Get(8, CurrentUser, "What is the price?");

    public override IEnumerable<string> Buttons => AvailableAssets.GetAsCurrency(SellPrice).Append(Cancel);

    public override async Task HandleMessage(string message)
    {
        var assets = AssetTypes.SelectMany(type => AssetManager.ReadAll(type, CurrentUser.Id)).Where(a => a.MarkedToSell).ToList();

        if (IsCanceled(message))
        {
            assets.ForEach(a =>
            {
                a.MarkedToSell = false;
                AssetManager.Update(a);
            });

            NextStage = New<Start>();
            return;
        }

        var price = message.AsCurrency();
        if (price <= 0)
        {
            await CurrentUser.Notify(Terms.Get(9, CurrentUser, "Invalid price value. Try again."));
            return;
        }

        var person = PersonManager.Read(CurrentUser.Id);
        assets.ForEach(asset =>
        {
            person.Cash += price; // * asset.Qtty;
            AssetManager.Sell(asset, ActionType, price, CurrentUser);
            HistoryManager.Add(ActionType, asset.Id, CurrentUser);
        });
        PersonManager.Update(person);

        await CurrentUser.Notify(Terms.Get(13, CurrentUser, "Done."));
        NextStage = New<Start>();
    }
}
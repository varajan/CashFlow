using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.MarketStages;

public class SellLand(ITermsService termsService, IAssetManager assetManager) : SellAsset<SellLandPrice>(AssetType.Land, termsService, assetManager) { }

public class SellLandPrice(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IAssetManager assetManager,
    IPersonManager personManager,
    IHistoryManager historyManager) : SellAssetPrice(AssetType.Land, termsService, availableAssets, assetManager, personManager, historyManager) { }

public class SellRealEstate(ITermsService termsService) : BaseStage(termsService)
{
}

public class SellBusiness(ITermsService termsService, IAssetManager assetManager) : SellAsset<SellBusinessPrice>(AssetType.Business, termsService, assetManager) { }

public class SellBusinessPrice(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IAssetManager assetManager,
    IPersonManager personManager,
    IHistoryManager historyManager) : SellAssetPrice(AssetType.Business, termsService, availableAssets, assetManager, personManager, historyManager)
{ }

public class SellCoins(ITermsService termsService, IAssetManager assetManager) : SellAsset<SellCoinsPrice>(AssetType.Coin, termsService, assetManager) { }

public class SellCoinsPrice(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IAssetManager assetManager,
    IPersonManager personManager,
    IHistoryManager historyManager) : SellAssetPrice(AssetType.Coin, termsService, availableAssets, assetManager, personManager, historyManager)
{ }

public class IncreaseCashflow(ITermsService termsService) : BaseStage(termsService)
{
}

public class SellAsset<TNextStage>(
    AssetType assetType,
    ITermsService termsService,
    IAssetManager assetManager)
    : BaseStage(termsService) where TNextStage : BaseStage
{
    protected AssetType AssetType { get; } = assetType;
    protected IAssetManager AssetManager { get; } = assetManager;

    public override string Message
    {
        get
        {
            var assetNames = Assets.Select((a, i) => $"*#{i+1}* {AssetManager.GetDescription(a, CurrentUser)}").Join(Environment.NewLine);

            return AssetType switch
            {
                AssetType.Land => Terms.Get(99, CurrentUser, "What Land do you want to sell?{0}{1}", Environment.NewLine, assetNames),
                AssetType.Coin => Terms.Get(122, CurrentUser, "What coins do you want to sell?"),
                AssetType.Stock => Terms.Get(27, CurrentUser, "What stocks do you want to sell?"),
                _ => throw new NotImplementedException(),
            };
        }
    }

    public override IEnumerable<string> Buttons =>
        AssetType switch
        {
            AssetType.Stock => Assets.Select(a => a.Title).Distinct().Append(Cancel),

            _ => Assets.Select((l, i) => $"#{i + 1}").Append(Cancel),
        };

    private List<AssetDto> Assets => AssetManager.ReadAll(AssetType, CurrentUser.Id);

    public override async Task HandleMessage(string message)
    {
        if (IsCanceled(message))
        {
            NextStage = New<Start>();
            return;
        }

        var index = message.Replace("#", "").ToInt();
        if (index < 1 || index > Assets.Count)
        {
            switch (AssetType)
            {
                case AssetType.Land:
                    await CurrentUser.Notify(Terms.Get(101, CurrentUser, "Invalid land number."));
                    break;

                case AssetType.Coin:
                    await CurrentUser.Notify(Terms.Get(123, CurrentUser, "Invalid coins title."));
                    break;

                default:
                    throw new NotImplementedException();
            }
            return;
        }

        var asset = Assets[index-1];
        asset.MarkedToSell = true;
        AssetManager.Update(asset);
        NextStage = New<TNextStage>();
    }
}

public class SellAssetPrice(
    AssetType assetType,
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IAssetManager assetManager,
    IPersonManager personManager,
    IHistoryManager historyManager) : BaseStage(termsService)
{
    protected AssetType AssetType { get; } = assetType;

    protected ActionType ActionType => AssetType switch
    {
        AssetType.Land => ActionType.SellLand,
        AssetType.Business => ActionType.SellBusiness,
        AssetType.Stock => ActionType.SellStocks,

        _ => throw new NotImplementedException(),
    };

    protected AssetType SellPrice => AssetType switch
    {
        AssetType.Land => AssetType.LandSellPrice,
        AssetType.Business => AssetType.BusinessSellPrice,
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
        var assets = AssetManager.ReadAll(AssetType, CurrentUser.Id).Where(a => a.MarkedToSell).ToList();

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
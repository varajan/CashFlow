using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.MarketStages;

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
        AssetType.RealEstate => ActionType.SellRealEstate,

        _ => throw new NotImplementedException(),
    };

    protected AssetType SellPrice => AssetTypes.First() switch
    {
        AssetType.Land => AssetType.LandSellPrice,
        AssetType.Coin => AssetType.CoinSellPrice,
        AssetType.Business => AssetType.BusinessSellPrice,
        AssetType.SmallBusiness => AssetType.BusinessSellPrice,
        AssetType.Stock => AssetType.StockPrice,
        AssetType.RealEstate => AssetType.RealEstateSellPrice,

        _ => throw new NotImplementedException(),
    };

    protected IAvailableAssets AvailableAssets { get; } = availableAssets;
    protected IAssetManager AssetManager { get; } = assetManager;
    private IPersonManager PersonManager { get; } = personManager;
    protected IHistoryManager HistoryManager { get; } = historyManager;

    public override string Message
    {
        get
        {
            if (AssetTypes.Contains(AssetType.RealEstate))
            {
                var count = AssetManager.ReadAll(AssetType.RealEstate, CurrentUser.Id).First(a => a.MarkedToSell)
                    .Title
                    .GetApartmentsCount();

                return count == 1
                    ? Terms.Get(8, CurrentUser, "What is the price?")
                    : Terms.Get(137, CurrentUser, "You have *{0}* apartments. What is the price per one appartment?", count);
            }

            return Terms.Get(8, CurrentUser, "What is the price?");
        }
    }

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
            var count = asset.Title.GetApartmentsCount();
            person.Cash += price * count;
            AssetManager.Sell(asset, ActionType, price, CurrentUser);
            HistoryManager.Add(ActionType, asset.Id, CurrentUser);
        });
        PersonManager.Update(person);

        await CurrentUser.Notify(Terms.Get(13, CurrentUser, "Done."));
        NextStage = New<Start>();
    }
}
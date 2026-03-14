using CashFlow.Data.Consts;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.MarketStages;

public class SellAssetPrice(
    ITermsRepository termsService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    params AssetType[] assetTypes) : BaseStage(termsService, personManager)
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

    protected IAvailableAssetsRepository AvailableAssets { get; } = availableAssets;

    public override string Message
    {
        get
        {
            if (AssetTypes.Contains(AssetType.RealEstate))
            {
                var count = PersonManager.ReadAllAssets(AssetType.RealEstate, CurrentUser).First(a => a.MarkedToSell)
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
        var assets = AssetTypes.SelectMany(type => PersonManager.ReadAllAssets(type, CurrentUser)).Where(a => a.MarkedToSell).ToList();

        if (IsCanceled(message))
        {
            assets.ForEach(a =>
            {
                a.MarkedToSell = false;
                PersonManager.UpdateAsset(CurrentUser, a);
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

        assets.ForEach(asset =>
        {
            var person = PersonManager.Read(CurrentUser);
            var count = asset.Type == AssetType.RealEstate ? asset.Title.GetApartmentsCount() : asset.Qtty;
            person.Cash += price * count - asset.Mortgage;

            PersonManager.Update(person);
            PersonManager.SellAsset(asset, ActionType, price, CurrentUser);
            PersonManager.AddHistory(ActionType, price, CurrentUser, asset.Id);
        });

        await CurrentUser.Notify(Terms.Get(13, CurrentUser, "Done."));
        NextStage = New<Start>();
    }
}
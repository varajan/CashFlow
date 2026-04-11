using CashFlow.Data.Consts;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.MarketStages;

public class SellAssetPrice(
    ITranslationService termsService,
    IUserService userService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository,
    params AssetType[] assetTypes) : BaseStage(termsService, userService, personManager, userRepository)
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
                var count = PersonService.ReadAllAssets(AssetType.RealEstate, CurrentUser).First(a => a.MarkedToSell)
                    .Title
                    .GetApartmentsCount();

                return count == 1
                    ? TranslationService.Get(Terms.AskPrice, CurrentUser)
                    : TranslationService.Get(Terms.ApartmentsAsk, CurrentUser, count);
            }

            return TranslationService.Get(Terms.AskPrice, CurrentUser);
        }
    }

    public override IEnumerable<string> Buttons => AvailableAssets.GetAsCurrency(SellPrice).Append(Cancel);

    public override async Task HandleMessage(string message)
    {
        var assets = AssetTypes.SelectMany(type => PersonService.ReadAllAssets(type, CurrentUser)).Where(a => a.MarkedToSell).ToList();

        if (IsCanceled(message))
        {
            assets.ForEach(a =>
            {
                a.MarkedToSell = false;
                PersonService.UpdateAsset(CurrentUser, a);
            });

            NextStage = New<Start>();
            return;
        }

        var price = message.AsCurrency();
        if (price <= 0)
        {
            await UserService.Notify(CurrentUser, TranslationService.Get(Terms.InvalidPrice, CurrentUser));
            return;
        }

        assets.ForEach(asset =>
        {
            var person = PersonService.Read(CurrentUser);
            var count = asset.Type == AssetType.RealEstate ? asset.Title.GetApartmentsCount() : asset.Qtty;
            person.Cash += price * count - asset.Mortgage;

            PersonService.Update(person);
            PersonService.SellAsset(asset, price, CurrentUser);
            PersonService.AddHistory(ActionType, price, CurrentUser, asset.Id);
        });

        await UserService.Notify(CurrentUser, TranslationService.Get(Terms.Done, CurrentUser));
        NextStage = New<Start>();
    }
}
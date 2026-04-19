using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.MarketStages;

public class SellAsset<TNextStage>(
    ITranslationService termsService,
    IUserService userService,
    IPersonService personManager,
    IUserRepository userRepository,
    params AssetType[] assetTypes)
    : BaseStage(termsService, userService, personManager, userRepository) where TNextStage : BaseStage
{
    protected AssetType[] AssetTypes { get; } = assetTypes;

    public override string Message
    {
        get
        {
            var assetNames = Assets.Select((a, i) => $"*#{i + 1}* {PersonService.GetAssetDescription(a, CurrentUser)}").Join(Environment.NewLine);

            if (AssetTypes.Contains(AssetType.Land))
            {
                return TranslationService.Get(Terms.SellLandAsk, CurrentUser, Environment.NewLine, assetNames);
            }

            if (AssetTypes.Contains(AssetType.RealEstate))
            {
                return TranslationService.Get(Terms.SellRealEstateAsk, CurrentUser, Environment.NewLine, assetNames);
            }

            if (AssetTypes.ContainsAny(AssetType.Business, AssetType.SmallBusiness))
            {
                return TranslationService.Get(Terms.SellBusinessAsk, CurrentUser, Environment.NewLine, assetNames);
            }

            if (AssetTypes.Contains(AssetType.Coin))
            {
                return TranslationService.Get(Terms.SellCoinsAsk, CurrentUser);
            }

            if (AssetTypes.Contains(AssetType.Stock))
            {
                return TranslationService.Get(Terms.SellStocksAsk, CurrentUser);
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

    private List<AssetDto> Assets => AssetTypes.SelectMany(type => PersonService.ReadActiveAssets(type, CurrentUser).Where(a => !a.IsDeleted)).ToList();

    public override async Task HandleMessage(string message)
    {
        if (IsCanceled(message))
        {
            NextStage = New<Start>();
            return;
        }

        var moveNext = AssetTypes.ContainsAny(AssetType.Land, AssetType.Business, AssetType.SmallBusiness, AssetType.RealEstate)
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
                await UserService.Notify(CurrentUser, TranslationService.Get(Terms.InvalidLand, CurrentUser));
                return false;
            }

            if (AssetTypes.Contains(AssetType.RealEstate))
            {
                await UserService.Notify(CurrentUser, TranslationService.Get(Terms.InvalidRealEstate, CurrentUser));
                return false;
            }

            if (AssetTypes.Contains(AssetType.Business) || AssetTypes.Contains(AssetType.SmallBusinessType))
            {
                await UserService.Notify(CurrentUser, TranslationService.Get(Terms.InvalidBusiness, CurrentUser));
                return false;
            }

            throw new NotImplementedException();
        }

        var asset = Assets[index - 1];
        asset.MarkedToSell = true;
        PersonService.UpdateAsset(CurrentUser, asset);
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
                await UserService.Notify(CurrentUser, TranslationService.Get(Terms.InvalidCoins, CurrentUser));
                return false;
            }

            if (AssetTypes.Contains(AssetType.Stock))
            {
                await UserService.Notify(CurrentUser, TranslationService.Get(Terms.InvalidStockName, CurrentUser));
                return false;
            }

            throw new NotImplementedException();
        }

        assets.ForEach(asset =>
        {
            asset.MarkedToSell = true;
            PersonService.UpdateAsset(CurrentUser, asset);
        });

        return true;
    }
}

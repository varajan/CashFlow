using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.MarketStages;

public class SellAsset<TNextStage>(
    ITermsRepository termsService,
    IPersonService personManager,
    IUserRepository userRepository,
    params AssetType[] assetTypes)
    : BaseStage(termsService, personManager, userRepository) where TNextStage : BaseStage
{
    protected AssetType[] AssetTypes { get; } = assetTypes;

    public override string Message
    {
        get
        {
            var assetNames = Assets.Select((a, i) => $"*#{i + 1}* {PersonManager.GetAssetDescription(a, CurrentUser)}").Join(Environment.NewLine);

            if (AssetTypes.Contains(AssetType.Land))
            {
                return Terms.Get(99, CurrentUser, "What Land do you want to sell?{0}{1}", Environment.NewLine, assetNames);
            }

            if (AssetTypes.Contains(AssetType.RealEstate))
            {
                return Terms.Get(99, CurrentUser, "What RealEstate do you want to sell?{0}{1}", Environment.NewLine, assetNames);
            }

            if (AssetTypes.ContainsAny(AssetType.Business, AssetType.SmallBusiness))
            {
                return Terms.Get(99, CurrentUser, "What Business do you want to sell?{0}{1}", Environment.NewLine, assetNames);
            }

            if (AssetTypes.Contains(AssetType.Coin))
            {
                return Terms.Get(122, CurrentUser, "What coins do you want to sell?");
            }

            if (AssetTypes.Contains(AssetType.Stock))
            {
                return Terms.Get(27, CurrentUser, "What stocks do you want to sell?");
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

    private List<AssetDto> Assets => AssetTypes.SelectMany(type => PersonManager.ReadAllAssets(type, CurrentUser).Where(a => !a.IsDeleted)).ToList();

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
                await CurrentUser.Notify(Terms.Get(101, CurrentUser, "Invalid land number."));
                return false;
            }

            if (AssetTypes.Contains(AssetType.RealEstate))
            {
                await CurrentUser.Notify(Terms.Get(16, CurrentUser, "Invalid Real Estate number."));
                return false;
            }

            if (AssetTypes.Contains(AssetType.Business) || AssetTypes.Contains(AssetType.SmallBusinessType))
            {
                await CurrentUser.Notify(Terms.Get(76, CurrentUser, "Invalid business number."));
                return false;
            }

            throw new NotImplementedException();
        }

        var asset = Assets[index - 1];
        asset.MarkedToSell = true;
        PersonManager.UpdateAsset(CurrentUser, asset);
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
            PersonManager.UpdateAsset(CurrentUser, asset);
        });

        return true;
    }
}

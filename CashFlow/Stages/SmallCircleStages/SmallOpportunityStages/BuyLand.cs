using CashFlow.Data.Consts;
using CashFlow.Interfaces;
using CashFlow.Stages.BuyAssetStages;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages;

public class BuyLand(ITermsRepository termsService, IAvailableAssetsRepository availableAssets, IPersonService personManager)
    : BuyAsset<BuyLandPrice>(AssetType.LandTitle, AssetType.Land, termsService, availableAssets, personManager)
{ }

public class BuyLandPrice(
    ITermsRepository termsService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager)
    : BuyAssetPrice<BuyLandCredit>(AssetType.LandBuyPrice, AssetType.Land, ActionType.BuyLand, termsService, availableAssets, personManager)
{ }

public class BuyLandCredit(
    ITermsRepository termsService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager) : BuyAssetCredit<Start>(AssetType.LandBuyPrice, AssetType.Land, ActionType.BuyLand, termsService, availableAssets, personManager)
{ }

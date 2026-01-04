using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Interfaces;
using CashFlow.Stages.BuyAssetStages;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages;

public class BuyLand(ITermsService termsService, IAvailableAssets availableAssets, IPersonManager personManager)
    : BuyAsset<BuyLandPrice>(AssetType.LandTitle, AssetType.Land, termsService, availableAssets, personManager)
{ }

public class BuyLandPrice(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IPersonManager personManager)
    : BuyAssetPrice<BuyLandCredit>(AssetType.LandBuyPrice, AssetType.Land, ActionType.BuyLand, termsService, availableAssets, personManager)
{ }

public class BuyLandCredit(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IPersonManager personManager) : BuyAssetCredit<Start>(AssetType.LandBuyPrice, AssetType.Land, ActionType.BuyLand, termsService, availableAssets, personManager)
{ }

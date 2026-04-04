using CashFlow.Data.Consts;
using CashFlow.Interfaces;
using CashFlow.Stages.BuyAssetStages;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages;

public class BuyLand(ITranslationService termsService, IAvailableAssetsRepository availableAssets, IPersonService personManager, IUserRepository userRepository)
    : BuyAsset<BuyLandPrice>(AssetType.LandTitle, AssetType.Land, termsService, availableAssets, personManager, userRepository)
{ }

public class BuyLandPrice(
    ITranslationService termsService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository)
    : BuyAssetPrice<BuyLandCredit>(AssetType.LandBuyPrice, AssetType.Land, ActionType.BuyLand, termsService, availableAssets, personManager, userRepository)
{ }

public class BuyLandCredit(
    ITranslationService termsService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository)
    : BuyAssetCredit<Start>(AssetType.LandBuyPrice, AssetType.Land, ActionType.BuyLand, termsService, availableAssets, personManager, userRepository)
{ }

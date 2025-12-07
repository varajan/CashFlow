using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Interfaces;
using CashFlow.Stages.BuyAssetStages;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages;

public class BuyLand(ITermsService termsService, IAvailableAssets availableAssets, IAssetManager assetManager)
    : BuyAsset<BuyLandPrice>(AssetType.LandTitle, AssetType.LandTitle, termsService, availableAssets, assetManager)
{ }

public class BuyLandPrice(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IAssetManager assetManager,
    IHistoryManager historyManager,
    IPersonManager personManager)
    : BuyAssetPrice<BuyLandCredit>(AssetType.LandBuyPrice, AssetType.LandTitle, ActionType.BuyLand, termsService, availableAssets, assetManager, historyManager, personManager)
{ }

public class BuyLandCredit(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IAssetManager assetManager,
    IHistoryManager historyManager,
    IPersonManager personManager) : BuyAssetCredit<Start>(AssetType.LandBuyPrice, AssetType.LandTitle, ActionType.BuyLand, termsService, availableAssets, assetManager, historyManager, personManager)
{ }

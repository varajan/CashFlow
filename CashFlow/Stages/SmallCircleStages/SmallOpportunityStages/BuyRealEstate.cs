using CashFlow.Data;
using CashFlow.Interfaces;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Stages.BuyAssetStages;
using CashFlow.Data.Consts;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages;

public class BuySmallRealEstate(ITermsService termsService, IAvailableAssets availableAssets, IAssetManager assetManager)
    : BuyAsset<BuySmallRealEstatePrice>(AssetType.RealEstateSmallType, AssetType.RealEstate, termsService, availableAssets, assetManager) { }

public class BuySmallRealEstatePrice(ITermsService termsService, IAvailableAssets availableAssets, IAssetManager assetManager)
    : BuyAssetPriceWithFirstPayment<BuySmallRealEstateFirstPayment>(AssetType.RealEstateSmallBuyPrice, AssetType.RealEstate, termsService, availableAssets, assetManager) { }

public class BuySmallRealEstateFirstPayment(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IAssetManager assetManager,
    IHistoryManager historyManager,
    IPersonManager personManager)
    : BuyAssetFirstPayment<BuySmallRealEstateCredit>(
        AssetType.RealEstateSmallFirstPayment, AssetType.RealEstate, ActionType.BuyRealEstate, termsService, availableAssets, assetManager, historyManager, personManager) { }

public class BuySmallRealEstateCredit(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IAssetManager assetManager,
    IHistoryManager historyManager,
    IPersonManager personManager) : BuyAssetCredit<Start>(AssetType.RealEstateSmallFirstPayment, AssetType.RealEstate, ActionType.BuyRealEstate, termsService, availableAssets, assetManager, historyManager, personManager) { }

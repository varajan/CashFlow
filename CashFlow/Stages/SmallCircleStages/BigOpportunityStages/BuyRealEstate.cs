using CashFlow.Data;
using CashFlow.Interfaces;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Stages.BuyAssetStages;
using CashFlow.Data.Consts;

namespace CashFlow.Stages.SmallCircleStages.BigOpportunityStages;

public class BuyBigRealEstate(ITermsService termsService, IAvailableAssets availableAssets, IAssetManager assetManager, IPersonManager personManager)
    : BuyAsset<BuyBigRealEstatePrice>(AssetType.RealEstateBigType, AssetType.RealEstate, termsService, availableAssets, assetManager, personManager) { }

public class BuyBigRealEstatePrice(ITermsService termsService, IAvailableAssets availableAssets, IAssetManager assetManager, IPersonManager personManager)
    : BuyAssetPriceWithFirstPayment<BuyBigRealEstateFirstPayment>(
        AssetType.RealEstateBigBuyPrice, AssetType.RealEstate, termsService, availableAssets, assetManager, personManager) { }

public class BuyBigRealEstateFirstPayment(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IAssetManager assetManager,
    IPersonManager personManager)
    : BuyAssetWithCashflowFirstPayment<BuyBigRealEstateCashFlow, BuyBigRealEstateCredit>(
        AssetType.RealEstateBigFirstPayment, AssetType.RealEstate, termsService, availableAssets, assetManager, personManager) { }

public class BuyBigRealEstateCredit(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IAssetManager assetManager,
    IPersonManager personManager)
    : BuyAssetWithCashflowCredit<BuyBigRealEstateCashFlow>(
        AssetType.RealEstateBigFirstPayment, AssetType.RealEstate, termsService, availableAssets, assetManager, personManager) { }

public class BuyBigRealEstateCashFlow(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IAssetManager assetManager,
    IHistoryManager historyManager,
    IPersonManager personManager)
    : BuyAssetCashFlow<Start>(
        AssetType.RealEstateBigCashFlow, AssetType.RealEstate, ActionType.BuyRealEstate, termsService, availableAssets, assetManager, historyManager, personManager) { }

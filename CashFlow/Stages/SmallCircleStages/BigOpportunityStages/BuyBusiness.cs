using CashFlow.Data;
using CashFlow.Interfaces;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Stages.BuyAssetStages;
using CashFlow.Data.Consts;

namespace CashFlow.Stages.SmallCircleStages.BigOpportunityStages;

public class BuyBusiness(ITermsService termsService, IAvailableAssets availableAssets, IAssetManager assetManager)
    : BuyAsset<BuyBusinessPrice>(AssetType.Business, AssetType.Business, termsService, availableAssets, assetManager) { }

public class BuyBusinessPrice(ITermsService termsService, IAvailableAssets availableAssets, IAssetManager assetManager)
    : BuyAssetPriceWithFirstPayment<BuyBusinessFirstPayment>(
        AssetType.BusinessBuyPrice, AssetType.Business, termsService, availableAssets, assetManager) { }

public class BuyBusinessFirstPayment(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IAssetManager assetManager,
    IPersonManager personManager)
    : BuyAssetWithCashflowFirstPayment<BuyBusinessCashFlow, BuyBusinessCredit>(
        AssetType.BusinessFirstPayment, AssetType.Business, termsService, availableAssets, assetManager, personManager) { }

public class BuyBusinessCredit(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IAssetManager assetManager,
    IPersonManager personManager)
    : BuyAssetWithCashflowCredit<BuyBusinessCashFlow>(
        AssetType.BusinessFirstPayment, AssetType.Business, termsService, availableAssets, assetManager, personManager) { }

public class BuyBusinessCashFlow(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IAssetManager assetManager,
    IHistoryManager historyManager,
    IPersonManager personManager)
    : BuyAssetCashFlow<Start>(
        AssetType.BusinessCashFlow, AssetType.Business, ActionType.BuyBusiness, termsService, availableAssets, assetManager, historyManager, personManager) { }

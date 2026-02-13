using CashFlow.Data;
using CashFlow.Interfaces;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Stages.BuyAssetStages;
using CashFlow.Data.Consts;

namespace CashFlow.Stages.SmallCircleStages.BigOpportunityStages;

public class BuyBusiness(ITermsService termsService, IAvailableAssets availableAssets, IPersonManager personManager)
    : BuyAsset<BuyBusinessPrice>(AssetType.BusinessType, AssetType.Business, termsService, availableAssets, personManager) { }

public class BuyBusinessPrice(ITermsService termsService, IAvailableAssets availableAssets, IPersonManager personManager)
    : BuyAssetPriceWithFirstPayment<BuyBusinessFirstPayment>(
        AssetType.BusinessBuyPrice, AssetType.Business, termsService, availableAssets, personManager) { }

public class BuyBusinessFirstPayment(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IPersonManager personManager)
    : BuyAssetWithCashflowFirstPayment<BuyBusinessCashFlow, BuyBusinessCredit>(
        AssetType.BusinessFirstPayment, AssetType.Business, termsService, availableAssets, personManager) { }

public class BuyBusinessCredit(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IPersonManager personManager)
    : BuyAssetWithCashflowCredit<BuyBusinessCashFlow>(
        AssetType.BusinessFirstPayment, AssetType.Business, termsService, availableAssets, personManager) { }

public class BuyBusinessCashFlow(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IPersonManager personManager)
    : BuyAssetCashFlow<Start>(
        AssetType.BusinessCashFlow, AssetType.Business, ActionType.BuyBusiness, termsService, availableAssets, personManager) { }

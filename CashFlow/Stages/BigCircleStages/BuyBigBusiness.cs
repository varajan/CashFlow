using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Interfaces;
using CashFlow.Stages.BuyAssetStages;

namespace CashFlow.Stages.BigCircleStages;

public class BuyBigBusiness(ITermsService termsService, IAvailableAssets availableAssets, IAssetManager assetManager, IPersonManager personManager)
    : BuyAsset<BuyBigBusinessPrice>(AssetType.Business, AssetType.Business, termsService, availableAssets, assetManager, personManager)
{ }

public class BuyBigBusinessPrice(ITermsService termsService, IAvailableAssets availableAssets, IAssetManager assetManager, IPersonManager personManager)
    : BuyAssetPriceWithFirstPayment<BuyBigBusinessFirstPayment>(
        AssetType.BusinessBuyPrice, AssetType.Business, termsService, availableAssets, assetManager, personManager)
{ }

public class BuyBigBusinessFirstPayment(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IAssetManager assetManager,
    IPersonManager personManager)
    : BuyAssetWithCashflowFirstPayment<BuyBigBusinessCashFlow, BuyBigBusinessCashFlow>(
        AssetType.BusinessFirstPayment, AssetType.Business, termsService, availableAssets, assetManager, personManager)
{ }

//public class BuyBigBusinessCredit(
//    ITermsService termsService,
//    IAvailableAssets availableAssets,
//    IAssetManager assetManager,
//    IPersonManager personManager)
//    : BuyAssetWithCashflowCredit<BuyBigBusinessCashFlow>(
//        AssetType.BusinessFirstPayment, AssetType.Business, termsService, availableAssets, assetManager, personManager)
//{ }

public class BuyBigBusinessCashFlow(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IAssetManager assetManager,
    IHistoryManager historyManager,
    IPersonManager personManager)
    : BuyAssetCashFlow<Start>(
        AssetType.BusinessCashFlow, AssetType.Business, ActionType.BuyBusiness, termsService, availableAssets, assetManager, historyManager, personManager)
{ }

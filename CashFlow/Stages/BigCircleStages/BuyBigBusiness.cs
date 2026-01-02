using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Interfaces;
using CashFlow.Stages.BuyAssetStages;

namespace CashFlow.Stages.BigCircleStages;

public class BuyBigBusiness(ITermsService termsService, IAvailableAssets availableAssets, IPersonManager personManager)
    : BuyAsset<BuyBigBusinessPrice>(AssetType.BigBusinessType, AssetType.BigBusinessType, termsService, availableAssets, personManager)
{ }

public class BuyBigBusinessPrice(ITermsService termsService, IAvailableAssets availableAssets, IPersonManager personManager)
    : BuyAssetPriceWithFirstPayment<BuyBigBusinessCashFlow>(
        AssetType.BigBusinessBuyPrice, AssetType.BigBusinessType, termsService, availableAssets, personManager)
{ }

public class BuyBigBusinessFirstPayment(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IPersonManager personManager)
    : BuyAssetWithCashflowFirstPayment<BuyBigBusinessCashFlow, Start>(
        AssetType.BigBusinessCashFlow, AssetType.BigBusinessType, termsService, availableAssets, personManager)
{ }

public class BuyBigBusinessCashFlow(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IHistoryManager historyManager,
    IPersonManager personManager)
    : BuyAssetCashFlow<Start>(
        AssetType.BigBusinessCashFlow, AssetType.BigBusinessType, ActionType.BuyBusiness, termsService, availableAssets, historyManager, personManager)
{ }

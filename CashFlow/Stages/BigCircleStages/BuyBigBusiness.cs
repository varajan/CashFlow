using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Interfaces;
using CashFlow.Stages.BuyAssetStages;

namespace CashFlow.Stages.BigCircleStages;

public class BuyBigBusiness(ITermsRepository termsService, IAvailableAssetsRepository availableAssets, IPersonService personManager)
    : BuyAsset<BuyBigBusinessPrice>(AssetType.BigBusinessType, AssetType.BigBusinessType, termsService, availableAssets, personManager)
{ }

public class BuyBigBusinessPrice(ITermsRepository termsService, IAvailableAssetsRepository availableAssets, IPersonService personManager)
    : BuyAssetPriceWithFirstPayment<BuyBigBusinessCashFlow>(
        AssetType.BigBusinessBuyPrice, AssetType.BigBusinessType, termsService, availableAssets, personManager)
{ }

public class BuyBigBusinessFirstPayment(
    ITermsRepository termsService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager)
    : BuyAssetWithCashflowFirstPayment<BuyBigBusinessCashFlow, Start>(
        AssetType.BigBusinessCashFlow, AssetType.BigBusinessType, termsService, availableAssets, personManager)
{ }

public class BuyBigBusinessCashFlow(
    ITermsRepository termsService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager)
    : BuyAssetCashFlow<Start>(
        AssetType.BigBusinessCashFlow, AssetType.BigBusinessType, ActionType.BuyBusiness, termsService, availableAssets, personManager)
{ }

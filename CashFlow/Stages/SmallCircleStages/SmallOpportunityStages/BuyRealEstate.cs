using CashFlow.Data;
using CashFlow.Interfaces;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Stages.BuyAssetStages;
using CashFlow.Data.Consts;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages;

public class BuySmallRealEstate(ITermsService termsService, IAvailableAssets availableAssets, IPersonManager personManager)
    : BuyAsset<BuySmallRealEstatePrice>(AssetType.RealEstateSmallType, AssetType.RealEstate, termsService, availableAssets, personManager) { }

public class BuySmallRealEstatePrice(ITermsService termsService, IAvailableAssets availableAssets, IPersonManager personManager)
    : BuyAssetPriceWithFirstPayment<BuySmallRealEstateFirstPayment>(
        AssetType.RealEstateSmallBuyPrice, AssetType.RealEstate, termsService, availableAssets, personManager) { }

public class BuySmallRealEstateFirstPayment(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IPersonManager personManager)
    : BuyAssetWithCashflowFirstPayment<BuySmallRealEstateCashFlow, BuySmallRealEstateCredit>(
        AssetType.RealEstateSmallFirstPayment, AssetType.RealEstate, termsService, availableAssets, personManager) { }

public class BuySmallRealEstateCredit(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IPersonManager personManager)
    : BuyAssetWithCashflowCredit<BuySmallRealEstateCashFlow>(
        AssetType.RealEstateSmallFirstPayment, AssetType.RealEstate, termsService, availableAssets, personManager) { }

public class BuySmallRealEstateCashFlow(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IPersonManager personManager)
    : BuyAssetCashFlow<Start>(
        AssetType.RealEstateSmallCashFlow, AssetType.RealEstate, ActionType.BuyRealEstate, termsService, availableAssets, personManager) { }

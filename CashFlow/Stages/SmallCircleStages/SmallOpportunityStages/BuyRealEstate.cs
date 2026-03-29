using CashFlow.Interfaces;
using CashFlow.Stages.BuyAssetStages;
using CashFlow.Data.Consts;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages;

public class BuySmallRealEstate(ITermsRepository termsService, IAvailableAssetsRepository availableAssets, IPersonService personManager, IUserRepository userRepository)
    : BuyAsset<BuySmallRealEstatePrice>(AssetType.RealEstateSmallType, AssetType.RealEstate, termsService, availableAssets, personManager, userRepository) { }

public class BuySmallRealEstatePrice(ITermsRepository termsService, IAvailableAssetsRepository availableAssets, IPersonService personManager, IUserRepository userRepository)
    : BuyAssetPriceWithFirstPayment<BuySmallRealEstateFirstPayment>(
        AssetType.RealEstateSmallBuyPrice, AssetType.RealEstate, termsService, availableAssets, personManager, userRepository) { }

public class BuySmallRealEstateFirstPayment(
    ITermsRepository termsService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository)
    : BuyAssetWithCashflowFirstPayment<BuySmallRealEstateCashFlow, BuySmallRealEstateCredit>(
        AssetType.RealEstateSmallFirstPayment, AssetType.RealEstate, termsService, availableAssets, personManager, userRepository) { }

public class BuySmallRealEstateCredit(
    ITermsRepository termsService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository)
    : BuyAssetWithCashflowCredit<BuySmallRealEstateCashFlow>(
        AssetType.RealEstateSmallFirstPayment, AssetType.RealEstate, termsService, availableAssets, personManager, userRepository) { }

public class BuySmallRealEstateCashFlow(
    ITermsRepository termsService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository)
    : BuyAssetCashFlow<Start>(
        AssetType.RealEstateSmallCashFlow, AssetType.RealEstate, ActionType.BuyRealEstate, termsService, availableAssets, personManager, userRepository) { }

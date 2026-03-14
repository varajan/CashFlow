using CashFlow.Interfaces;
using CashFlow.Stages.BuyAssetStages;
using CashFlow.Data.Consts;

namespace CashFlow.Stages.SmallCircleStages.BigOpportunityStages;

public class BuyBigRealEstate(ITermsRepository termsService, IAvailableAssetsRepository availableAssets, IPersonService personManager, IUserRepository userRepository)
    : BuyAsset<BuyBigRealEstatePrice>(AssetType.RealEstateBigType, AssetType.RealEstate, termsService, availableAssets, personManager, userRepository) { }

public class BuyBigRealEstatePrice(ITermsRepository termsService, IAvailableAssetsRepository availableAssets, IPersonService personManager, IUserRepository userRepository)
    : BuyAssetPriceWithFirstPayment<BuyBigRealEstateFirstPayment>(
        AssetType.RealEstateBigBuyPrice, AssetType.RealEstate, termsService, availableAssets, personManager, userRepository) { }

public class BuyBigRealEstateFirstPayment(
    ITermsRepository termsService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository)
    : BuyAssetWithCashflowFirstPayment<BuyBigRealEstateCashFlow, BuyBigRealEstateCredit>(
        AssetType.RealEstateBigFirstPayment, AssetType.RealEstate, termsService, availableAssets, personManager, userRepository) { }

public class BuyBigRealEstateCredit(
    ITermsRepository termsService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository)
    : BuyAssetWithCashflowCredit<BuyBigRealEstateCashFlow>(
        AssetType.RealEstateBigFirstPayment, AssetType.RealEstate, termsService, availableAssets, personManager, userRepository) { }

public class BuyBigRealEstateCashFlow(
    ITermsRepository termsService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository)
    : BuyAssetCashFlow<Start>(
        AssetType.RealEstateBigCashFlow, AssetType.RealEstate, ActionType.BuyRealEstate, termsService, availableAssets, personManager, userRepository) { }

using CashFlow.Interfaces;
using CashFlow.Stages.BuyAssetStages;
using CashFlow.Data.Consts;

namespace CashFlow.Stages.SmallCircleStages.BigOpportunityStages;

public class BuyBigRealEstate(ITranslationService termsService, IUserService userService, IAvailableAssetsRepository availableAssets, IPersonService personManager, IUserRepository userRepository)
    : BuyAsset<BuyBigRealEstatePrice>(AssetType.RealEstateBigType, AssetType.RealEstate, termsService, userService, availableAssets, personManager, userRepository)
{ }

public class BuyBigRealEstatePrice(ITranslationService termsService, IUserService userService, IAvailableAssetsRepository availableAssets, IPersonService personManager, IUserRepository userRepository)
    : BuyAssetPriceWithFirstPayment<BuyBigRealEstateFirstPayment>(
        AssetType.RealEstateBigBuyPrice, AssetType.RealEstate, termsService, userService, availableAssets, personManager, userRepository)
{ }

public class BuyBigRealEstateFirstPayment(
    ITranslationService termsService, IUserService userService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository)
    : BuyAssetWithCashflowFirstPayment<BuyBigRealEstateCashFlow, BuyBigRealEstateCredit>(
        AssetType.RealEstateBigFirstPayment, AssetType.RealEstate, termsService, userService, availableAssets, personManager, userRepository)
{ }

public class BuyBigRealEstateCredit(
    ITranslationService termsService, IUserService userService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository)
    : BuyAssetWithCashflowCredit<BuyBigRealEstateCashFlow>(
        AssetType.RealEstateBigFirstPayment, AssetType.RealEstate, termsService, userService, availableAssets, personManager, userRepository)
{ }

public class BuyBigRealEstateCashFlow(
    ITranslationService termsService, IUserService userService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository)
    : BuyAssetCashFlow<Start>(
        AssetType.RealEstateBigCashFlow, AssetType.RealEstate, ActionType.BuyRealEstate, termsService, userService, availableAssets, personManager, userRepository)
{ }

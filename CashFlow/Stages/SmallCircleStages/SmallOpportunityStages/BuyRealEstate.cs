using CashFlow.Data.Consts;
using CashFlow.Interfaces;
using CashFlow.Stages.BuyAssetStages;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages;

public class BuySmallRealEstate(ITranslationService termsService, IUserService userService, IAvailableAssetsRepository availableAssets, IPersonService personManager, IUserRepository userRepository)
    : BuyAsset<BuySmallRealEstatePrice>(AssetType.RealEstateSmallType, AssetType.RealEstate, termsService, userService, availableAssets, personManager, userRepository)
{ }

public class BuySmallRealEstatePrice(ITranslationService termsService, IUserService userService, IAvailableAssetsRepository availableAssets, IPersonService personManager, IUserRepository userRepository)
    : BuyAssetPriceWithFirstPayment<BuySmallRealEstateFirstPayment>(
        AssetType.RealEstateSmallBuyPrice, AssetType.RealEstate, termsService, userService, availableAssets, personManager, userRepository)
{ }

public class BuySmallRealEstateFirstPayment(
    ITranslationService termsService, IUserService userService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository)
    : BuyAssetWithCashflowFirstPayment<BuySmallRealEstateCashFlow, BuySmallRealEstateCredit>(
        AssetType.RealEstateSmallFirstPayment, AssetType.RealEstate, termsService, userService, availableAssets, personManager, userRepository)
{ }

public class BuySmallRealEstateCredit(
    ITranslationService termsService, IUserService userService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository)
    : BuyAssetWithCashflowCredit<BuySmallRealEstateCashFlow>(
        AssetType.RealEstateSmallFirstPayment, AssetType.RealEstate, termsService, userService, availableAssets, personManager, userRepository)
{ }

public class BuySmallRealEstateCashFlow(
    ITranslationService termsService, IUserService userService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository)
    : BuyAssetCashFlow<Start>(
        AssetType.RealEstateSmallCashFlow, AssetType.RealEstate, ActionType.BuyRealEstate, termsService, userService, availableAssets, personManager, userRepository)
{ }

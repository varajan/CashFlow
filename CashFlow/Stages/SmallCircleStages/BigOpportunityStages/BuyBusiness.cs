using CashFlow.Interfaces;
using CashFlow.Stages.BuyAssetStages;
using CashFlow.Data.Consts;

namespace CashFlow.Stages.SmallCircleStages.BigOpportunityStages;

public class BuyBusiness(ITranslationService termsService, IUserService userService, IAvailableAssetsRepository availableAssets, IPersonService personManager, IUserRepository userRepository)
    : BuyAsset<BuyBusinessPrice>(AssetType.BusinessType, AssetType.Business, termsService, userService, availableAssets, personManager, userRepository) { }

public class BuyBusinessPrice(ITranslationService termsService, IUserService userService, IAvailableAssetsRepository availableAssets, IPersonService personManager, IUserRepository userRepository)
    : BuyAssetPriceWithFirstPayment<BuyBusinessFirstPayment>(
        AssetType.BusinessBuyPrice, AssetType.Business, termsService, userService, availableAssets, personManager, userRepository) { }

public class BuyBusinessFirstPayment(
    ITranslationService termsService, IUserService userService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository)
    : BuyAssetWithCashflowFirstPayment<BuyBusinessCashFlow, BuyBusinessCredit>(
        AssetType.BusinessFirstPayment, AssetType.Business, termsService, userService, availableAssets, personManager, userRepository) { }

public class BuyBusinessCredit(
    ITranslationService termsService, IUserService userService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository)
    : BuyAssetWithCashflowCredit<BuyBusinessCashFlow>(
        AssetType.BusinessFirstPayment, AssetType.Business, termsService, userService, availableAssets, personManager, userRepository) { }

public class BuyBusinessCashFlow(
    ITranslationService termsService, IUserService userService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository)
    : BuyAssetCashFlow<Start>(
        AssetType.BusinessCashFlow, AssetType.Business, ActionType.BuyBusiness, termsService, userService, availableAssets, personManager, userRepository) { }

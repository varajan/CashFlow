using CashFlow.Interfaces;
using CashFlow.Stages.BuyAssetStages;
using CashFlow.Data.Consts;

namespace CashFlow.Stages.SmallCircleStages.BigOpportunityStages;

public class BuyBusiness(ITermsRepository termsService, IAvailableAssetsRepository availableAssets, IPersonService personManager, IUserRepository userRepository)
    : BuyAsset<BuyBusinessPrice>(AssetType.BusinessType, AssetType.Business, termsService, availableAssets, personManager, userRepository) { }

public class BuyBusinessPrice(ITermsRepository termsService, IAvailableAssetsRepository availableAssets, IPersonService personManager, IUserRepository userRepository)
    : BuyAssetPriceWithFirstPayment<BuyBusinessFirstPayment>(
        AssetType.BusinessBuyPrice, AssetType.Business, termsService, availableAssets, personManager, userRepository) { }

public class BuyBusinessFirstPayment(
    ITermsRepository termsService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository)
    : BuyAssetWithCashflowFirstPayment<BuyBusinessCashFlow, BuyBusinessCredit>(
        AssetType.BusinessFirstPayment, AssetType.Business, termsService, availableAssets, personManager, userRepository) { }

public class BuyBusinessCredit(
    ITermsRepository termsService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository)
    : BuyAssetWithCashflowCredit<BuyBusinessCashFlow>(
        AssetType.BusinessFirstPayment, AssetType.Business, termsService, availableAssets, personManager, userRepository) { }

public class BuyBusinessCashFlow(
    ITermsRepository termsService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository)
    : BuyAssetCashFlow<Start>(
        AssetType.BusinessCashFlow, AssetType.Business, ActionType.BuyBusiness, termsService, availableAssets, personManager, userRepository) { }

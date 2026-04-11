using CashFlow.Data.Consts;
using CashFlow.Interfaces;
using CashFlow.Stages.BuyAssetStages;

namespace CashFlow.Stages.BigCircleStages;

public class BuyBigBusiness(ITranslationService termsService, IUserService userService, IAvailableAssetsRepository availableAssets, IPersonService personManager, IUserRepository userRepository)
    : BuyAsset<BuyBigBusinessPrice>(AssetType.BigBusinessType, AssetType.BigBusinessType, termsService, userService, availableAssets, personManager, userRepository)
{ }

public class BuyBigBusinessPrice(ITranslationService termsService, IUserService userService, IAvailableAssetsRepository availableAssets, IPersonService personManager, IUserRepository userRepository)
    : BuyAssetPriceWithFirstPayment<BuyBigBusinessCashFlow>(
        AssetType.BigBusinessBuyPrice, AssetType.BigBusinessType, termsService, userService, availableAssets, personManager, userRepository)
{ }

public class BuyBigBusinessFirstPayment(
    ITranslationService termsService, IUserService userService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository)
    : BuyAssetWithCashflowFirstPayment<BuyBigBusinessCashFlow, Start>(
        AssetType.BigBusinessCashFlow, AssetType.BigBusinessType, termsService, userService, availableAssets, personManager, userRepository)
{ }

public class BuyBigBusinessCashFlow(
    ITranslationService termsService, IUserService userService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository)
    : BuyAssetCashFlow<Start>(
        AssetType.BigBusinessCashFlow, AssetType.BigBusinessType, ActionType.BuyBusiness, termsService, userService, availableAssets, personManager, userRepository)
{ }

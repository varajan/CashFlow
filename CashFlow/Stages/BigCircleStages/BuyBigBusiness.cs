using CashFlow.Data.Consts;
using CashFlow.Interfaces;
using CashFlow.Stages.BuyAssetStages;

namespace CashFlow.Stages.BigCircleStages;

public class BuyBigBusiness(ITermsRepository termsService, IAvailableAssetsRepository availableAssets, IPersonService personManager, IUserRepository userRepository)
    : BuyAsset<BuyBigBusinessPrice>(AssetType.BigBusinessType, AssetType.BigBusinessType, termsService, availableAssets, personManager, userRepository)
{ }

public class BuyBigBusinessPrice(ITermsRepository termsService, IAvailableAssetsRepository availableAssets, IPersonService personManager, IUserRepository userRepository)
    : BuyAssetPriceWithFirstPayment<BuyBigBusinessCashFlow>(
        AssetType.BigBusinessBuyPrice, AssetType.BigBusinessType, termsService, availableAssets, personManager, userRepository)
{ }

public class BuyBigBusinessFirstPayment(
    ITermsRepository termsService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository)
    : BuyAssetWithCashflowFirstPayment<BuyBigBusinessCashFlow, Start>(
        AssetType.BigBusinessCashFlow, AssetType.BigBusinessType, termsService, availableAssets, personManager, userRepository)
{ }

public class BuyBigBusinessCashFlow(
    ITermsRepository termsService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository)
    : BuyAssetCashFlow<Start>(
        AssetType.BigBusinessCashFlow, AssetType.BigBusinessType, ActionType.BuyBusiness, termsService, availableAssets, personManager, userRepository)
{ }

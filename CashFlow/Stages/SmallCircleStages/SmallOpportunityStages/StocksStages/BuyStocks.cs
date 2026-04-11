using CashFlow.Data.Consts;
using CashFlow.Interfaces;
using CashFlow.Stages.BuyAssetStages;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.StocksStages;

public class BuyStocks(ITranslationService termsService, IUserService userService, IAvailableAssetsRepository availableAssets, IPersonService personManager, IUserRepository userRepository)
    : BuyAsset<BuyStocksPrice>(AssetType.Stock, AssetType.Stock, termsService, userService, availableAssets, personManager, userRepository)
{ }

public class BuyStocksPrice(ITranslationService termsService, IUserService userService, IAvailableAssetsRepository availableAssets, IPersonService personManager, IUserRepository userRepository)
    : BuyAssetPriceWithCount<BuyStocksCount>(AssetType.StockPrice, AssetType.Stock, termsService, userService, availableAssets, personManager, userRepository)
{ }

public class BuyStocksCount(
    ITranslationService termsService, IUserService userService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository)
    : BuyAssetCount<BuyStocksCredit, BuyStocksCashFlow>(
        AssetType.Stock, AssetType.Stock, ActionType.BuyStocks, termsService, userService, availableAssets, personManager, userRepository) { }

public class BuyStocksCredit(
    ITranslationService termsService, IUserService userService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository) : BuyAssetCredit<BuyStocksCashFlow>(AssetType.Stock, AssetType.Stock, ActionType.BuyStocks, termsService, userService, availableAssets, personManager, userRepository)
{ }

public class BuyStocksCashFlow(
    ITranslationService termsService, IUserService userService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository)
    : BuyAssetCashFlow<Start>(
        AssetType.Stock, AssetType.Stock, ActionType.BuyStocks, termsService, userService, availableAssets, personManager, userRepository)
{ }

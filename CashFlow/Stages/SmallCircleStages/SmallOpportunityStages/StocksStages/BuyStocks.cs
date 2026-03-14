using CashFlow.Data.Consts;
using CashFlow.Interfaces;
using CashFlow.Stages.BuyAssetStages;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.StocksStages;

public class BuyStocks(ITermsRepository termsService, IAvailableAssetsRepository availableAssets, IPersonService personManager, IUserRepository userRepository)
    : BuyAsset<BuyStocksPrice>(AssetType.Stock, AssetType.Stock, termsService, availableAssets, personManager, userRepository)
{ }

public class BuyStocksPrice(ITermsRepository termsService, IAvailableAssetsRepository availableAssets, IPersonService personManager, IUserRepository userRepository)
    : BuyAssetPriceWithCount<BuyStocksCount>(AssetType.StockPrice, AssetType.Stock, termsService, availableAssets, personManager, userRepository)
{ }

public class BuyStocksCount(
    ITermsRepository termsService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository)
    : BuyAssetCount<BuyStocksCredit, BuyStocksCashFlow>(
        AssetType.Stock, AssetType.Stock, ActionType.BuyStocks, termsService, availableAssets, personManager, userRepository) { }

public class BuyStocksCredit(
    ITermsRepository termsService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository) : BuyAssetCredit<BuyStocksCashFlow>(AssetType.Stock, AssetType.Stock, ActionType.BuyStocks, termsService, availableAssets, personManager, userRepository)
{ }

public class BuyStocksCashFlow(
    ITermsRepository termsService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository)
    : BuyAssetCashFlow<Start>(
        AssetType.Stock, AssetType.Stock, ActionType.BuyStocks, termsService, availableAssets, personManager, userRepository)
{ }

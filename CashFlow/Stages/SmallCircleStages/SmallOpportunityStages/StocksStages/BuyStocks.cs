using CashFlow.Data.Consts;
using CashFlow.Interfaces;
using CashFlow.Stages.BuyAssetStages;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.StocksStages;

public class BuyStocks(ITermsRepository termsService, IAvailableAssetsRepository availableAssets, IPersonService personManager)
    : BuyAsset<BuyStocksPrice>(AssetType.Stock, AssetType.Stock, termsService, availableAssets, personManager)
{ }

public class BuyStocksPrice(ITermsRepository termsService, IAvailableAssetsRepository availableAssets, IPersonService personManager)
    : BuyAssetPriceWithCount<BuyStocksCount>(AssetType.StockPrice, AssetType.Stock, termsService, availableAssets, personManager)
{ }

public class BuyStocksCount(
    ITermsRepository termsService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager)
    : BuyAssetCount<BuyStocksCredit, BuyStocksCashFlow>(
        AssetType.Stock, AssetType.Stock, ActionType.BuyStocks, termsService, availableAssets, personManager) { }

public class BuyStocksCredit(
    ITermsRepository termsService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager) : BuyAssetCredit<BuyStocksCashFlow>(AssetType.Stock, AssetType.Stock, ActionType.BuyStocks, termsService, availableAssets, personManager)
{ }

public class BuyStocksCashFlow(
    ITermsRepository termsService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager)
    : BuyAssetCashFlow<Start>(
        AssetType.Stock, AssetType.Stock, ActionType.BuyStocks, termsService, availableAssets, personManager)
{ }

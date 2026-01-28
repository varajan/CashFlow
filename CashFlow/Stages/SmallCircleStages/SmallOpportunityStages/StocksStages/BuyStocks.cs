using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Interfaces;
using CashFlow.Stages.BuyAssetStages;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.StocksStages;

public class BuyStocks(ITermsService termsService, IAvailableAssets availableAssets, IPersonManager personManager)
    : BuyAsset<BuyStocksPrice>(AssetType.Stock, AssetType.Stock, termsService, availableAssets, personManager)
{ }

public class BuyStocksPrice(ITermsService termsService, IAvailableAssets availableAssets, IPersonManager personManager)
    : BuyAssetPriceWithCount<BuyStocksCount>(AssetType.StockPrice, AssetType.Stock, termsService, availableAssets, personManager)
{ }

public class BuyStocksCount(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IPersonManager personManager)
    : BuyAssetCount<BuyStocksCredit, BuyStocksCashFlow>(
        AssetType.Stock, AssetType.Stock, ActionType.BuyStocks, termsService, availableAssets, personManager) { }

public class BuyStocksCredit(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IPersonManager personManager) : BuyAssetCredit<BuyStocksCashFlow>(AssetType.Stock, AssetType.Stock, ActionType.BuyStocks, termsService, availableAssets, personManager)
{ }

public class BuyStocksCashFlow(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IPersonManager personManager)
    : BuyAssetCashFlow<Start>(
        AssetType.Stock, AssetType.Stock, ActionType.BuyStocks, termsService, availableAssets, personManager)
{ }

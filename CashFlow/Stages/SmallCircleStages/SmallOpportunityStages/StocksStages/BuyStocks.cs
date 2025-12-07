using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Interfaces;
using CashFlow.Stages.BuyAssetStages;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.StocksStages;

public class BuyStocks(ITermsService termsService, IAvailableAssets availableAssets, IAssetManager assetManager)
    : BuyAsset<BuyStocksPrice>(AssetType.Stock, AssetType.Stock, termsService, availableAssets, assetManager)
{ }

public class BuyStocksPrice(ITermsService termsService, IAvailableAssets availableAssets, IAssetManager assetManager)
    : BuyAssetPriceWithCount<BuyStocksCount>(AssetType.StockPrice, AssetType.Stock, termsService, availableAssets, assetManager)
{ }

public class BuyStocksCount(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IAssetManager assetManager,
    IHistoryManager historyManager,
    IPersonManager personManager)
    : BuyAssetCount<BuyStocksCredit>(
        AssetType.Stock, AssetType.Stock, ActionType.BuyStocks, termsService, availableAssets, assetManager, historyManager, personManager) { }

public class BuyStocksCredit(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IAssetManager assetManager,
    IHistoryManager historyManager,
    IPersonManager personManager) : BuyAssetCredit<Start>(AssetType.Stock, AssetType.Stock, ActionType.BuyStocks, termsService, availableAssets, assetManager, historyManager, personManager)
{ }

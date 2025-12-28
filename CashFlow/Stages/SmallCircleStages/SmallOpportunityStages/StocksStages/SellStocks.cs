using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Interfaces;
using CashFlow.Stages.SmallCircleStages.MarketStages;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.StocksStages;

public class SellStocks(ITermsService termsService, IAssetManager assetManager, IPersonManager personManager)
    : SellAsset<SellStocksPrice>(termsService, assetManager, personManager, AssetType.Stock) { }

public class SellStocksPrice(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IAssetManager assetManager,
    IPersonManager personManager,
    IHistoryManager historyManager) : SellAssetPrice(termsService, availableAssets, assetManager, personManager, historyManager, AssetType.Stock) { }

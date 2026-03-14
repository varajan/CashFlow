using CashFlow.Data.Consts;
using CashFlow.Interfaces;
using CashFlow.Stages.SmallCircleStages.MarketStages;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.StocksStages;

public class SellStocks(ITermsRepository termsService, IPersonService personManager)
    : SellAsset<SellStocksPrice>(termsService, personManager, AssetType.Stock) { }

public class SellStocksPrice(
    ITermsRepository termsService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager) : SellAssetPrice(termsService, availableAssets, personManager, AssetType.Stock) { }

using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Interfaces;
using CashFlow.Stages.SmallCircleStages.MarketStages;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.StocksStages;

public class SellStocks(ITermsService termsService, IPersonManager personManager)
    : SellAsset<SellStocksPrice>(termsService, personManager, AssetType.Stock) { }

public class SellStocksPrice(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IPersonManager personManager) : SellAssetPrice(termsService, availableAssets, personManager, AssetType.Stock) { }

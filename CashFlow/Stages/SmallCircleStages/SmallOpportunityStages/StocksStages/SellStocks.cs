using CashFlow.Data.Consts;
using CashFlow.Interfaces;
using CashFlow.Stages.SmallCircleStages.MarketStages;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.StocksStages;

public class SellStocks(ITermsRepository termsService, IPersonService personManager, IUserRepository userRepository)
    : SellAsset<SellStocksPrice>(termsService, personManager, userRepository, AssetType.Stock) { }

public class SellStocksPrice(
    ITermsRepository termsService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager, IUserRepository userRepository) : SellAssetPrice(termsService, availableAssets, personManager, userRepository, AssetType.Stock) { }

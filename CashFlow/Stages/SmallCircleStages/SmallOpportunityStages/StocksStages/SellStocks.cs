using CashFlow.Data.Consts;
using CashFlow.Interfaces;
using CashFlow.Stages.SmallCircleStages.MarketStages;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.StocksStages;

public class SellStocks(ITranslationService termsService, IUserService userService, IPersonService personManager, IUserRepository userRepository)
    : SellAsset<SellStocksPrice>(termsService, userService, personManager, userRepository, AssetType.Stock)
{ }

public class SellStocksPrice(
    ITranslationService termsService, IUserService userService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager, IUserRepository userRepository) : SellAssetPrice(termsService, userService, availableAssets, personManager, userRepository, AssetType.Stock)
{ }

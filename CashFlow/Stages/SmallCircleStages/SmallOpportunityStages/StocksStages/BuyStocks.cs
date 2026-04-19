using CashFlow.Data.Consts;
using CashFlow.Interfaces;
using CashFlow.Stages.BuyAssetStages;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.StocksStages;

public class BuyStocks(ITranslationService termsService, IUserService userService, IPersonService personManager, IUserRepository userRepository)
    : BuyAsset<BuyStocksPrice>(Terms.StockNames, AssetType.Stock, termsService, userService, personManager, userRepository)
{ }

public class BuyStocksPrice(ITranslationService termsService, IUserService userService, IPersonService personManager, IUserRepository userRepository)
    : BuyAssetPriceWithCount<BuyStocksCount>(Prices.StockPrice, AssetType.Stock, termsService, userService, personManager, userRepository)
{ }

public class BuyStocksCount(ITranslationService termsService, IUserService userService, IPersonService personManager, IUserRepository userRepository)
    : BuyAssetCount<BuyStocksCredit, BuyStocksCashFlow>(AssetType.Stock, ActionType.BuyStocks, termsService, userService, personManager, userRepository)
{ }

public class BuyStocksCredit(ITranslationService termsService, IUserService userService, IPersonService personManager, IUserRepository userRepository) : BuyAssetCredit<BuyStocksCashFlow>(AssetType.Stock, ActionType.BuyStocks, termsService, userService, personManager, userRepository)
{ }

public class BuyStocksCashFlow(ITranslationService termsService, IUserService userService, IPersonService personManager, IUserRepository userRepository)
    : BuyAssetCashFlow<Start>(Cashflow.Stock, AssetType.Stock, ActionType.BuyStocks, termsService, userService, personManager, userRepository)
{ }

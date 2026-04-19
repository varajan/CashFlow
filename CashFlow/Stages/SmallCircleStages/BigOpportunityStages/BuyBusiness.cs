using CashFlow.Data.Consts;
using CashFlow.Interfaces;
using CashFlow.Stages.BuyAssetStages;

namespace CashFlow.Stages.SmallCircleStages.BigOpportunityStages;

public class BuyBusiness(ITranslationService termsService, IUserService userService, IPersonService personManager, IUserRepository userRepository)
    : BuyAsset<BuyBusinessPrice>(Terms.BusinessTypes, AssetType.Business, termsService, userService, personManager, userRepository)
{ }

public class BuyBusinessPrice(ITranslationService termsService, IUserService userService, IPersonService personManager, IUserRepository userRepository)
    : BuyAssetPriceWithFirstPayment<BuyBusinessFirstPayment>(
        Prices.BigBusinessBuyPrice, AssetType.Business, termsService, userService, personManager, userRepository)
{ }

public class BuyBusinessFirstPayment(
    ITranslationService termsService, IUserService userService,

    IPersonService personManager,
    IUserRepository userRepository)
    : BuyAssetWithCashflowFirstPayment<BuyBusinessCashFlow, BuyBusinessCredit>(
        Prices.BusinessFirstPayment, AssetType.Business, termsService, userService, personManager, userRepository)
{ }

public class BuyBusinessCredit(
    ITranslationService termsService, IUserService userService,

    IPersonService personManager,
    IUserRepository userRepository)
    : BuyAssetWithCashflowCredit<BuyBusinessCashFlow>(AssetType.Business, termsService, userService, personManager, userRepository)
{ }

public class BuyBusinessCashFlow(
    ITranslationService termsService, IUserService userService,

    IPersonService personManager,
    IUserRepository userRepository)
    : BuyAssetCashFlow<Start>(
        Prices.BigBusinessCashFlow, AssetType.Business, ActionType.BuyBusiness, termsService, userService, personManager, userRepository)
{ }

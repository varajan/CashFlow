using CashFlow.Data.Consts;
using CashFlow.Interfaces;
using CashFlow.Stages.BuyAssetStages;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages;

public class BuySmallRealEstate(ITranslationService termsService, IUserService userService, IPersonService personManager, IUserRepository userRepository)
    : BuyAsset<BuySmallRealEstatePrice>(Terms.RealEstateSmallTypes, AssetType.RealEstate, termsService, userService, personManager, userRepository)
{ }

public class BuySmallRealEstatePrice(ITranslationService termsService, IUserService userService, IPersonService personManager, IUserRepository userRepository)
    : BuyAssetPriceWithFirstPayment<BuySmallRealEstateFirstPayment>(
        Prices.RealEstateSmallBuyPrice, AssetType.RealEstate, termsService, userService, personManager, userRepository)
{ }

public class BuySmallRealEstateFirstPayment(ITranslationService termsService, IUserService userService, IPersonService personManager, IUserRepository userRepository)
    : BuyAssetWithCashflowFirstPayment<BuySmallRealEstateCashFlow, BuySmallRealEstateCredit>(
        Prices.RealEstateSmallFirstPayment, AssetType.RealEstate, termsService, userService, personManager, userRepository)
{ }

public class BuySmallRealEstateCredit(ITranslationService termsService, IUserService userService, IPersonService personManager, IUserRepository userRepository)
    : BuyAssetWithCashflowCredit<BuySmallRealEstateCashFlow>(AssetType.RealEstate, termsService, userService, personManager, userRepository)
{ }

public class BuySmallRealEstateCashFlow(ITranslationService termsService, IUserService userService, IPersonService personManager, IUserRepository userRepository)
    : BuyAssetCashFlow<Start>(Prices.RealEstateSmallCashFlow, AssetType.RealEstate, ActionType.BuyRealEstate, termsService, userService, personManager, userRepository)
{ }

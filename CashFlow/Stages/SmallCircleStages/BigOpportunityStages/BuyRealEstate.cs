using CashFlow.Data.Consts;
using CashFlow.Interfaces;
using CashFlow.Stages.BuyAssetStages;

namespace CashFlow.Stages.SmallCircleStages.BigOpportunityStages;

public class BuyBigRealEstate(ITranslationService termsService, IUserService userService, IPersonService personManager, IUserRepository userRepository)
    : BuyAsset<BuyBigRealEstatePrice>(Terms.RealEstateBigTypes, AssetType.RealEstate, termsService, userService, personManager, userRepository)
{ }

public class BuyBigRealEstatePrice(ITranslationService termsService, IUserService userService, IPersonService personManager, IUserRepository userRepository)
    : BuyAssetPriceWithFirstPayment<BuyBigRealEstateFirstPayment>(
        BuyPrice.RealEstateBig, AssetType.RealEstate, termsService, userService, personManager, userRepository)
{ }

public class BuyBigRealEstateFirstPayment(ITranslationService termsService, IUserService userService, IPersonService personManager, IUserRepository userRepository)
    : BuyAssetWithCashflowFirstPayment<BuyBigRealEstateCashFlow, BuyBigRealEstateCredit>(
        FirstPayment.RealEstateBig, AssetType.RealEstate, termsService, userService, personManager, userRepository)
{ }

public class BuyBigRealEstateCredit(ITranslationService termsService, IUserService userService, IPersonService personManager, IUserRepository userRepository)
    : BuyAssetWithCashflowCredit<BuyBigRealEstateCashFlow>(AssetType.RealEstate, termsService, userService, personManager, userRepository)
{ }

public class BuyBigRealEstateCashFlow(ITranslationService termsService, IUserService userService, IPersonService personManager, IUserRepository userRepository)
    : BuyAssetCashFlow<Start>(
        Cashflow.RealEstateBig, AssetType.RealEstate, ActionType.BuyRealEstate, termsService, userService, personManager, userRepository)
{ }

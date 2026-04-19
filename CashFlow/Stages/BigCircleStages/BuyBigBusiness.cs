using CashFlow.Data.Consts;
using CashFlow.Interfaces;
using CashFlow.Stages.BuyAssetStages;

namespace CashFlow.Stages.BigCircleStages;

public class BuyBigBusiness(ITranslationService termsService, IUserService userService, IPersonService personManager, IUserRepository userRepository)
    : BuyAsset<BuyBigBusinessPrice>(Terms.BigBusinessTypes, AssetType.BigBusinessType, termsService, userService, personManager, userRepository)
{ }

public class BuyBigBusinessPrice(ITranslationService termsService, IUserService userService, IPersonService personManager, IUserRepository userRepository)
    : BuyAssetPriceWithFirstPayment<BuyBigBusinessCashFlow>(
        Prices.BigBusinessBuyPrice, AssetType.BigBusinessType, termsService, userService, personManager, userRepository)
{ }


public class BuyBigBusinessCashFlow(
    ITranslationService termsService,
    IUserService userService,

    IPersonService personManager,
    IUserRepository userRepository)
    : BuyAssetCashFlow<Start>(
        Prices.BigBusinessCashFlow, AssetType.BigBusinessType, ActionType.BuyBusiness, termsService, userService, personManager, userRepository)
{ }

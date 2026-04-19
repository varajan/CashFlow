using CashFlow.Data.Consts;
using CashFlow.Interfaces;
using CashFlow.Stages.BuyAssetStages;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages;

public class BuyLand(ITranslationService termsService, IUserService userService, IPersonService personManager, IUserRepository userRepository)
    : BuyAsset<BuyLandPrice>(Terms.LandTitles, AssetType.Land, termsService, userService, personManager, userRepository)
{ }

public class BuyLandPrice(ITranslationService termsService, IUserService userService, IPersonService personManager, IUserRepository userRepository)
    : BuyAssetPrice<BuyLandCredit>(Prices.LandBuyPrice, AssetType.Land, ActionType.BuyLand, termsService, userService, personManager, userRepository)
{ }

public class BuyLandCredit(ITranslationService termsService, IUserService userService, IPersonService personManager, IUserRepository userRepository)
    : BuyAssetCredit<Start>(AssetType.Land, ActionType.BuyLand, termsService, userService, personManager, userRepository)
{ }

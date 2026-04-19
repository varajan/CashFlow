using CashFlow.Data.Consts;
using CashFlow.Interfaces;
using CashFlow.Stages.BuyAssetStages;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages;

public class StartCompany(ITranslationService termsService, IUserService userService, IPersonService personManager, IUserRepository userRepository)
    : BuyAsset<StartCompanyPrice>(Terms.SmallBusinessTypes, AssetType.SmallBusinessType, termsService, userService, personManager, userRepository)
{ }

public class StartCompanyPrice(ITranslationService termsService, IUserService userService, IPersonService personManager, IUserRepository userRepository)
    : BuyAssetPrice<StartCompanyCredit>(BuyPrices.BusinessSmall, AssetType.SmallBusinessType, ActionType.StartCompany, termsService, userService, personManager, userRepository)
{ }

public class StartCompanyCredit(ITranslationService termsService, IUserService userService, IPersonService personManager, IUserRepository userRepository)
    : BuyAssetCredit<Start>(AssetType.SmallBusinessType, ActionType.StartCompany, termsService, userService, personManager, userRepository)
{ }

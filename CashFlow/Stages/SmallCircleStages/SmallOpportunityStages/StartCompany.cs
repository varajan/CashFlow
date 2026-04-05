using CashFlow.Data.Consts;
using CashFlow.Interfaces;
using CashFlow.Stages.BuyAssetStages;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages;

public class StartCompany(ITranslationService termsService, IAvailableAssetsRepository availableAssets, IPersonService personManager, IUserRepository userRepository)
    : BuyAsset<StartCompanyPrice>(AssetType.SmallBusinessType, AssetType.SmallBusinessType, termsService, availableAssets, personManager, userRepository)
{ }

public class StartCompanyPrice(
    ITranslationService termsService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository)
    : BuyAssetPrice<StartCompanyCredit>(AssetType.SmallBusinessBuyPrice, AssetType.SmallBusinessType, ActionType.StartCompany, termsService, availableAssets, personManager, userRepository)
{ }

public class StartCompanyCredit(
    ITranslationService termsService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository)
    : BuyAssetCredit<Start>(AssetType.SmallBusinessBuyPrice, AssetType.SmallBusinessType, ActionType.StartCompany, termsService, availableAssets, personManager, userRepository)
{ }

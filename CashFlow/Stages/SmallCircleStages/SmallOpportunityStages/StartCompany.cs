using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Interfaces;
using CashFlow.Stages.BuyAssetStages;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages;

public class StartCompany(ITermsService termsService, IAvailableAssets availableAssets, IAssetManager assetManager)
    : BuyAsset<StartCompanyPrice>(AssetType.SmallBusinessType, AssetType.SmallBusinessType, termsService, availableAssets, assetManager)
{ }

public class StartCompanyPrice(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IAssetManager assetManager,
    IHistoryManager historyManager,
    IPersonManager personManager)
    : BuyAssetPrice<StartCompanyCredit>(AssetType.SmallBusinessBuyPrice, AssetType.SmallBusinessType, ActionType.StartCompany, termsService, availableAssets, assetManager, historyManager, personManager)
{ }

public class StartCompanyCredit(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IAssetManager assetManager,
    IHistoryManager historyManager,
    IPersonManager personManager) : BuyAssetCredit<Start>(AssetType.SmallBusinessBuyPrice, AssetType.SmallBusinessType, ActionType.StartCompany, termsService, availableAssets, assetManager, historyManager, personManager)
{ }

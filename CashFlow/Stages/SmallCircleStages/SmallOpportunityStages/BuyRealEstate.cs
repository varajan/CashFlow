using CashFlow.Data;
using CashFlow.Interfaces;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Stages.BuyRealEstateStages;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages;

public class BuySmallRealEstate(ITermsService termsService, IAvailableAssets availableAssets, IAssetManager assetManager)
    : BuyRealEstate(true, termsService, availableAssets, assetManager) { }

public class BuySmallRealEstatePrice(ITermsService termsService, IAvailableAssets availableAssets, IAssetManager assetManager)
    : BuyRealEstatePrice(true, termsService, availableAssets, assetManager) { }

public class BuySmallRealEstateFirstPayment(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IAssetManager assetManager,
    IHistoryManager historyManager,
    IPersonManager personManager)
    : BuyRealEstateFirstPayment(true, termsService, availableAssets, assetManager, historyManager, personManager) { }

public class BuySmallRealEstateCredit(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IAssetManager assetManager,
    IHistoryManager historyManager,
    IPersonManager personManager) : BuyRealEstateCredit(true, termsService, availableAssets, assetManager, historyManager, personManager) { }

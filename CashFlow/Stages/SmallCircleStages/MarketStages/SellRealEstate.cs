using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.MarketStages;

public class SellRealEstate(ITermsService termsService, IAssetManager assetManager, IPersonManager personManager)
    : SellAsset<SellRealEstatePrice>(termsService, assetManager, personManager, AssetType.RealEstate) { }

public class SellRealEstatePrice(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IAssetManager assetManager,
    IPersonManager personManager,
    IHistoryManager historyManager) : SellAssetPrice(termsService, availableAssets, assetManager, personManager, historyManager, AssetType.RealEstate)
{ }
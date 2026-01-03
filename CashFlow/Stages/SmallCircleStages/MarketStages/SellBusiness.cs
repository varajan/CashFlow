using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.MarketStages;

public class SellBusiness(ITermsService termsService, IAssetManager assetManager, IPersonManager personManager) :
    SellAsset<SellBusinessPrice>(termsService, assetManager, personManager, AssetType.Business, AssetType.SmallBusiness)
{ }

public class SellBusinessPrice(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IPersonManager personManager,
    IHistoryManager historyManager) : SellAssetPrice(termsService, availableAssets, personManager, historyManager, AssetType.Business, AssetType.SmallBusiness)
{ }
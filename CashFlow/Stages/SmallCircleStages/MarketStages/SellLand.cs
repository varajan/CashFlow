using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.MarketStages;

public class SellLand(ITermsService termsService, IAssetManager assetManager) : SellAsset<SellLandPrice>(termsService, assetManager, AssetType.Land) { }

public class SellLandPrice(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IAssetManager assetManager,
    IPersonManager personManager,
    IHistoryManager historyManager) : SellAssetPrice(termsService, availableAssets, assetManager, personManager, historyManager, AssetType.Land)
{ }

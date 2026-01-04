using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.MarketStages;

public class SellLand(ITermsService termsService, IAssetManager assetManager, IPersonManager personManager)
    : SellAsset<SellLandPrice>(termsService, assetManager, personManager, AssetType.Land) { }

public class SellLandPrice(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IPersonManager personManager) : SellAssetPrice(termsService, availableAssets, personManager, AssetType.Land)
{ }

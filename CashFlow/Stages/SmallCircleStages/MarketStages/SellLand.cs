using CashFlow.Data.Consts;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.MarketStages;

public class SellLand(ITermsRepository termsService, IPersonService personManager)
    : SellAsset<SellLandPrice>(termsService, personManager, AssetType.Land) { }

public class SellLandPrice(
    ITermsRepository termsService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager) : SellAssetPrice(termsService, availableAssets, personManager, AssetType.Land)
{ }

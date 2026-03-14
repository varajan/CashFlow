using CashFlow.Data.Consts;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.MarketStages;

public class SellRealEstate(ITermsRepository termsService, IPersonService personManager)
    : SellAsset<SellRealEstatePrice>(termsService, personManager, AssetType.RealEstate) { }

public class SellRealEstatePrice(
    ITermsRepository termsService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager) : SellAssetPrice(termsService, availableAssets, personManager, AssetType.RealEstate)
{ }
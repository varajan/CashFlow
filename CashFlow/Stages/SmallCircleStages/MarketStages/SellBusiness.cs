using CashFlow.Data.Consts;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.MarketStages;

public class SellBusiness(ITermsRepository termsService, IPersonService personManager) :
    SellAsset<SellBusinessPrice>(termsService, personManager, AssetType.Business, AssetType.SmallBusinessType)
{ }

public class SellBusinessPrice(
    ITermsRepository termsService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager) : SellAssetPrice(termsService, availableAssets, personManager, AssetType.Business, AssetType.SmallBusinessType)
{ }
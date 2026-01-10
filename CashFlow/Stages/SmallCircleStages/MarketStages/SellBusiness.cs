using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.MarketStages;

public class SellBusiness(ITermsService termsService, IPersonManager personManager) :
    SellAsset<SellBusinessPrice>(termsService, personManager, AssetType.Business, AssetType.SmallBusiness)
{ }

public class SellBusinessPrice(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IPersonManager personManager) : SellAssetPrice(termsService, availableAssets, personManager, AssetType.Business, AssetType.SmallBusiness)
{ }
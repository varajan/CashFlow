using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.MarketStages;

public class SellRealEstate(ITermsService termsService, IPersonManager personManager)
    : SellAsset<SellRealEstatePrice>(termsService, personManager, AssetType.RealEstate) { }

public class SellRealEstatePrice(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IPersonManager personManager) : SellAssetPrice(termsService, availableAssets, personManager, AssetType.RealEstate)
{ }
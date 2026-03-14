using CashFlow.Data.Consts;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.MarketStages;

public class SellRealEstate(ITermsRepository termsService, IPersonService personManager, IUserRepository userRepository)
    : SellAsset<SellRealEstatePrice>(termsService, personManager, userRepository, AssetType.RealEstate) { }

public class SellRealEstatePrice(
    ITermsRepository termsService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository) : SellAssetPrice(termsService, availableAssets, personManager, userRepository, AssetType.RealEstate)
{ }
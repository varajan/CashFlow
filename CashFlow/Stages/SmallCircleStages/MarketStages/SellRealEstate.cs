using CashFlow.Data.Consts;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.MarketStages;

public class SellRealEstate(ITranslationService termsService, IUserService userService, IPersonService personManager, IUserRepository userRepository)
    : SellAsset<SellRealEstatePrice>(termsService, userService, personManager, userRepository, AssetType.RealEstate) { }

public class SellRealEstatePrice(
    ITranslationService termsService,
    IUserService userService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository) : SellAssetPrice(termsService, userService,availableAssets, personManager, userRepository, AssetType.RealEstate)
{ }
using CashFlow.Data.Consts;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.MarketStages;

public class SellBusiness(ITranslationService termsService, IUserService userService, IPersonService personManager, IUserRepository userRepository) :
    SellAsset<SellBusinessPrice>(termsService, userService, personManager, userRepository, AssetType.Business, AssetType.SmallBusinessType)
{ }

public class SellBusinessPrice(
    ITranslationService termsService, IUserService userService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository) :
    SellAssetPrice(termsService, userService, availableAssets, personManager, userRepository, AssetType.Business, AssetType.SmallBusinessType)
{ }
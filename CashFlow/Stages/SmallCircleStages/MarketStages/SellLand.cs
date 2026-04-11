using CashFlow.Data.Consts;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.MarketStages;

public class SellLand(ITranslationService termsService, IUserService userService, IPersonService personManager, IUserRepository userRepository)
    : SellAsset<SellLandPrice>(termsService, userService, personManager, userRepository, AssetType.Land)
{ }

public class SellLandPrice(
    ITranslationService termsService, IUserService userService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository) : SellAssetPrice(termsService, userService, availableAssets, personManager, userRepository, AssetType.Land)
{ }

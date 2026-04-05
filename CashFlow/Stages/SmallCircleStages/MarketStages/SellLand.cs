using CashFlow.Data.Consts;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.MarketStages;

public class SellLand(ITranslationService termsService, IPersonService personManager, IUserRepository userRepository)
    : SellAsset<SellLandPrice>(termsService, personManager, userRepository, AssetType.Land) { }

public class SellLandPrice(
    ITranslationService termsService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository) : SellAssetPrice(termsService, availableAssets, personManager, userRepository, AssetType.Land)
{ }

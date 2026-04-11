using CashFlow.Data.Consts;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.MarketStages;

public class SellCoins(ITranslationService termsService, IUserService userService, IPersonService personManager, IUserRepository userRepository)
    : SellAsset<SellCoinsPrice>(termsService, userService, personManager, userRepository, AssetType.Coin) { }

public class SellCoinsPrice(
    ITranslationService termsService, IUserService userService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository) : SellAssetPrice(termsService, userService,availableAssets, personManager, userRepository, AssetType.Coin)
{ }

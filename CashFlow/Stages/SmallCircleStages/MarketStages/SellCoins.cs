using CashFlow.Data.Consts;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.MarketStages;

public class SellCoins(ITranslationService termsService, IPersonService personManager, IUserRepository userRepository)
    : SellAsset<SellCoinsPrice>(termsService, personManager, userRepository, AssetType.Coin) { }

public class SellCoinsPrice(
    ITranslationService termsService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository) : SellAssetPrice(termsService, availableAssets, personManager, userRepository, AssetType.Coin)
{ }

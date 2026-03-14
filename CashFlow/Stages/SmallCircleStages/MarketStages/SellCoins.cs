using CashFlow.Data.Consts;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.MarketStages;

public class SellCoins(ITermsRepository termsService, IPersonService personManager)
    : SellAsset<SellCoinsPrice>(termsService, personManager, AssetType.Coin) { }

public class SellCoinsPrice(
    ITermsRepository termsService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager) : SellAssetPrice(termsService, availableAssets, personManager, AssetType.Coin)
{ }

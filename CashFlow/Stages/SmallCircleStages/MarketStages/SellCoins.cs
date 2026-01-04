using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.MarketStages;

public class SellCoins(ITermsService termsService, IAssetManager assetManager, IPersonManager personManager)
    : SellAsset<SellCoinsPrice>(termsService, assetManager, personManager, AssetType.Coin) { }

public class SellCoinsPrice(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IPersonManager personManager) : SellAssetPrice(termsService, availableAssets, personManager, AssetType.Coin)
{ }

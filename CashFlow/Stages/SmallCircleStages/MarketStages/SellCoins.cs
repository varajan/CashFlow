using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.MarketStages;

public class SellCoins(ITermsService termsService, IAssetManager assetManager) : SellAsset<SellCoinsPrice>(termsService, assetManager, AssetType.Coin) { }

public class SellCoinsPrice(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IAssetManager assetManager,
    IPersonManager personManager,
    IHistoryManager historyManager) : SellAssetPrice(termsService, availableAssets, assetManager, personManager, historyManager, AssetType.Coin)
{ }

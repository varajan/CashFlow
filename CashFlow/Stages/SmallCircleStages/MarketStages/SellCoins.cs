using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.MarketStages;

public class SellCoins(ITermsService termsService, IPersonManager personManager)
    : SellAsset<SellCoinsPrice>(termsService, personManager, AssetType.Coin) { }

public class SellCoinsPrice(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IPersonManager personManager) : SellAssetPrice(termsService, availableAssets, personManager, AssetType.Coin)
{ }

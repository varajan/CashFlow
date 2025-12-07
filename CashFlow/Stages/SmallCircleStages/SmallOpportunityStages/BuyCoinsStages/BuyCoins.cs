using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Interfaces;
using CashFlow.Stages.BuyAssetStages;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.BuyCoinsStages;

public class BuyCoins(ITermsService termsService, IAvailableAssets availableAssets, IAssetManager assetManager)
    : BuyAsset<BuyCoinsCount>(AssetType.CoinTitle, AssetType.Coin, termsService, availableAssets, assetManager)
{ }

using CashFlow.Data.Consts;
using CashFlow.Interfaces;
using CashFlow.Stages.BuyAssetStages;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.BuyCoinsStages;

public class BuyCoins(ITermsRepository termsService, IAvailableAssetsRepository availableAssets, IPersonService personManager)
    : BuyAsset<BuyCoinsCount>(AssetType.CoinTitle, AssetType.Coin, termsService, availableAssets, personManager)
{ }

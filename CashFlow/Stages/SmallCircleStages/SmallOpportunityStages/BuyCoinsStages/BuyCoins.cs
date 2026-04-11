using CashFlow.Data.Consts;
using CashFlow.Interfaces;
using CashFlow.Stages.BuyAssetStages;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.BuyCoinsStages;

public class BuyCoins(ITranslationService termsService, IUserService userService, IAvailableAssetsRepository availableAssets, IPersonService personManager, IUserRepository userRepository)
    : BuyAsset<BuyCoinsCount>(AssetType.CoinTitle, AssetType.Coin, termsService, userService, availableAssets, personManager, userRepository)
{ }

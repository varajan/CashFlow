using CashFlow.Data.Consts;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.MarketStages;

public class SellBusiness(ITranslationService termsService, IPersonService personManager, IUserRepository userRepository) :
    SellAsset<SellBusinessPrice>(termsService, personManager, userRepository, AssetType.Business, AssetType.SmallBusinessType)
{ }

public class SellBusinessPrice(
    ITranslationService termsService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository) :
    SellAssetPrice(termsService, availableAssets, personManager, userRepository, AssetType.Business, AssetType.SmallBusinessType)
{ }
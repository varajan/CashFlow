using CashFlow.Data.Consts;
using CashFlow.Extensions;
using CashFlow.Interfaces;
using MoreLinq;

namespace CashFlow.Stages.BuyAssetStages;

public abstract class BuyAssetCashFlow<TNextStage>(
    AssetType assetName,
    AssetType assetType,
    ActionType actionType,
    ITranslationService termsService, IUserService userService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository)
    : BuyAsset<TNextStage>(assetName, assetType, termsService, userService, availableAssets, personManager, userRepository) where TNextStage : BaseStage
{
    protected ActionType ActionType { get; } = actionType;
    public override string Message => TranslationService.Get(Terms.AskCashflow, CurrentUser);
    public override IEnumerable<string> Buttons => AvailableAssets.GetAsCurrency(AssetName).Append(Cancel);

    public override async Task HandleMessage(string message)
    {
        var asset = PersonService.ReadAllAssets(AssetType, CurrentUser).Single(x => x.IsDraft);

        if (IsCanceled(message))
        {
            PersonService.DeleteAsset(CurrentUser, asset);
            NextStage = New<Start>();
            return;
        }

        asset.CashFlow = message.AsCurrency();
        asset.IsDraft = false;
        PersonService.UpdateAsset(CurrentUser, asset);

        var person = PersonService.Read(CurrentUser);
        var amount = (asset.Price * asset.Qtty) - asset.Mortgage;
        person.Cash -= amount;
        PersonService.Update(person);
        PersonService.AddHistory(ActionType, asset.CashFlow, CurrentUser, asset.Id);
        await UserService.Notify(CurrentUser, TranslationService.Get(Terms.Done, CurrentUser));

        NextStage = New<Start>();
    }
}

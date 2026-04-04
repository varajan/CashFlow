using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.BuyAssetStages;

public abstract class BuyAssetPrice<TNextStage>(
    AssetType assetName,
    AssetType assetType,
    ActionType actionType,
    ITranslationService termsService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository)
    : BuyAsset<TNextStage>(assetName, assetType, termsService, availableAssets, personManager, userRepository) where TNextStage : BaseStage
{
    protected ActionType ActionType { get; } = actionType;
    public override string Message => TranslationService.Get(Terms.AskPrice, CurrentUser);
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

        var number = message.AsCurrency();
        if (number <= 0)
        {
            await CurrentUser.Notify(TranslationService.Get(Terms.InvalidPrice, CurrentUser));
            return;
        }

        asset.Price = number;
        PersonService.UpdateAsset(CurrentUser, asset);

        var person = PersonService.Read(CurrentUser);
        if (person.Cash < asset.Price)
        {
            NextStage = New<TNextStage>();
            return;
        }

        await CompleteTransaction(asset);
        NextStage = New<Start>();
    }

    protected async Task CompleteTransaction(AssetDto asset)
    {
        var person = PersonService.Read(CurrentUser);

        person.Cash -= asset.Price;
        PersonService.Update(person);

        asset.IsDraft = false;
        PersonService.UpdateAsset(CurrentUser, asset);

        PersonService.AddHistory(ActionType, asset.Price, CurrentUser, asset.Id);

        await CurrentUser.Notify(TranslationService.Get(Terms.Done, CurrentUser));
    }
}

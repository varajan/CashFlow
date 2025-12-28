using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.BuyAssetStages;

public abstract class BuyAssetFirstPayment<TNextStage>(
    AssetType assetName,
    AssetType assetType,
    ActionType actionType,
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IAssetManager assetManager,
    IHistoryManager historyManager,
    IPersonManager personManager)
    : BuyAsset<TNextStage>(assetName, assetType, termsService, availableAssets, assetManager, personManager) where TNextStage : BaseStage
{
    protected ActionType ActionType { get; } = actionType;
    protected IHistoryManager HistoryManager { get; } = historyManager;

    public override string Message => Terms.Get(10, CurrentUser, "What is the first payment?");
    public override IEnumerable<string> Buttons => AvailableAssets.GetAsCurrency(AssetName).Append(Cancel);

    public override async Task HandleMessage(string message)
    {
        var asset = AssetManager.ReadAll(AssetType, CurrentUser.Id).Single(x => x.IsDraft);

        if (IsCanceled(message))
        {
            AssetManager.Delete(asset);
            NextStage = New<Start>();
            return;
        }

        var number = message.AsCurrency();
        if (number < 0)
        {
            await CurrentUser.Notify(Terms.Get(11, CurrentUser, "Invalid first payment value. Try again."));
            NextStage = this;
            return;
        }

        asset.Mortgage = asset.Price - number;
        AssetManager.Update(asset);

        var person = PersonManager.Read(CurrentUser.Id);
        if (person.Cash < number)
        {
            NextStage = New<TNextStage>();
            return;
        }

        await CompleteTransaction(asset);
        NextStage = New<Start>();
    }

    protected async Task CompleteTransaction(AssetDto asset)
    {
        var person = PersonManager.Read(CurrentUser.Id);
        var amount = asset.Price * asset.Qtty - asset.Mortgage;

        person.Cash -= amount;
        PersonManager.Update(person);

        asset.IsDraft = false;
        AssetManager.Update(asset);

        HistoryManager.Add(ActionType, asset.Id, CurrentUser);

        await CurrentUser.Notify(Terms.Get(13, CurrentUser, "Done."));
    }
}

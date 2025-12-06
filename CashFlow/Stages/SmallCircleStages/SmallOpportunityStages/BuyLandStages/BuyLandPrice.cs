using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.BuyLandStages;

public class BuyLandPrice(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IHistoryManager historyManager,
    IPersonManager personManager,
    IAssetManager assetManager) : BuyLand(termsService, availableAssets, assetManager)
{
    protected IHistoryManager HistoryManager { get; } = historyManager;
    protected IPersonManager PersonManager { get; } = personManager;


    public override string Message => Terms.Get(8, CurrentUser, "What is the price?");
    public override IEnumerable<string> Buttons => AvailableAssets.GetAsCurrency(AssetType.LandBuyPrice).Append(Cancel);

    public override async Task HandleMessage(string message)
    {
        if (IsCanceled(message))
        {
            NextStage = New<Start>();
            return;
        }

        var number = message.AsCurrency();

        if (number <= 0)
        {
            await CurrentUser.Notify(Terms.Get(9, CurrentUser, "Invalid price value. Try again."));
            return;
        }

        var asset = AssetManager.ReadAll(AssetType.LandTitle, CurrentUser.Id).Single(x => x.IsDraft);
        asset.Price = number;
        AssetManager.Update(asset);

        var person = PersonManager.Read(CurrentUser.Id);
        if (person.Cash < asset.Price)
        {
            NextStage = New<BuyLandCredit>();
            return;
        }

        await CompleteTransaction(asset);
        NextStage = New<Start>();
    }

    protected async Task CompleteTransaction(AssetDto asset)
    {
        var person = PersonManager.Read(CurrentUser.Id);

        person.Cash -= asset.Price;
        PersonManager.Update(person);

        asset.IsDraft = false;
        AssetManager.Update(asset);

        HistoryManager.Add(ActionType.BuyLand, asset.Id, CurrentUser);

        await CurrentUser.Notify(Terms.Get(13, CurrentUser, "Done."));
    }
}

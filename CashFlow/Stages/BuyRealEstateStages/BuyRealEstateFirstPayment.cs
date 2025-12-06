using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Extensions;
using CashFlow.Interfaces;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages;

namespace CashFlow.Stages.BuyRealEstateStages;

public abstract class BuyRealEstateFirstPayment(
    bool small,
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IAssetManager assetManager,
    IHistoryManager historyManager,
    IPersonManager personManager) : BuyRealEstate(small, termsService, availableAssets, assetManager)
{
    protected IHistoryManager HistoryManager { get; } = historyManager;
    protected IPersonManager PersonManager { get; } = personManager;

    public override string Message => Terms.Get(10, CurrentUser, "What is the first payment?");
    public override IEnumerable<string> Buttons => AvailableAssets.GetAsCurrency(AssetType.RealEstateSmallFirstPayment).Append(Cancel);

    public override async Task HandleMessage(string message)
    {
        var asset = AssetManager.ReadAll(AssetType.RealEstate, CurrentUser.Id).Single(x => x.IsDraft);

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
            NextStage = IsSmall ? New<BuySmallRealEstateCredit>() : throw new NotImplementedException();
            return;
        }

        await CompleteTransaction(asset);
        NextStage = New<Start>();
    }

    protected async Task CompleteTransaction(AssetDto asset)
    {
        var person = PersonManager.Read(CurrentUser.Id);
        var firstPayment = asset.Price - asset.Mortgage;

        person.Cash -= firstPayment;
        PersonManager.Update(person);

        asset.IsDraft = false;
        AssetManager.Update(asset);

        HistoryManager.Add(ActionType.BuyRealEstate, asset.Id, CurrentUser);

        await CurrentUser.Notify(Terms.Get(13, CurrentUser, "Done."));
    }
}

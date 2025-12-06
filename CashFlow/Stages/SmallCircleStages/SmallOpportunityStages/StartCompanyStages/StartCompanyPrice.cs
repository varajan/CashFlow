using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Extensions;
using CashFlow.Interfaces;
using System.Text;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.StartCompanyStages;

public class StartCompanyPrice(
    ITermsService termsService,
    IAvailableAssets assets,
    IAssetManager assetManager,
    IPersonManager personManager,
    IHistoryManager historyManager)
    : StartCompany(termsService, assets, assetManager)
{
    protected IPersonManager PersonManager { get; } = personManager;
    protected IHistoryManager HistoryManager { get; } = historyManager;

    public override string Message => Terms.Get(8, CurrentUser, "What is the price?");
    public override IEnumerable<string> Buttons => AvailableAssets.GetAsCurrency(AssetType.SmallBusinessBuyPrice).Append(Cancel);

    public async override Task HandleMessage(string message)
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

        var person = PersonManager.Read(CurrentUser.Id);
        var asset = AssetManager.ReadAll(AssetType.SmallBusinessType, CurrentUser.Id).Single(x => x.IsDraft);
        asset.Price = number;
        AssetManager.Update(asset);

        if (person.Cash < number)
        {
            NextStage = New<StartCompanyCredit>();
            return;
        }

        await CompleteTransaction();
        NextStage = New<Start>();
    }

    protected async Task CompleteTransaction()
    {
        var asset = AssetManager.ReadAll(AssetType.SmallBusinessType, CurrentUser.Id).Single(x => x.IsDraft);
        var person = PersonManager.Read(CurrentUser.Id);

        person.Cash -= asset.Price;
        PersonManager.Update(person);
        
        asset.IsDraft = false;
        AssetManager.Update(asset);
        
        HistoryManager.Add(ActionType.StartCompany, asset.Id, CurrentUser);

        await CurrentUser.Notify(Terms.Get(13, CurrentUser, "Done."));
    }
}

using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.BuyLandStages;

public class BuyLandCredit(
    ITermsService termsService,
    IAvailableAssets assets,
    IHistoryManager historyManager,
    IPersonManager personManager,
    IAssetManager assetManager) : BuyLandPrice(termsService, assets, historyManager, personManager, assetManager)
{
    public override string Message
    {
        get
        {
            var asset = AssetManager.ReadAll(AssetType.LandTitle, CurrentUser.Id).First(x => x.IsDraft);
            var value = asset.Price.AsCurrency();
            var cash = PersonManager.Read(CurrentUser.Id).Cash.AsCurrency();

            return Terms.Get(23, CurrentUser, "You don''t have {0}, but only {1}", value, cash);
        }
    }

    public override IEnumerable<string> Buttons => [Terms.Get(34, CurrentUser, "Get Credit"), Cancel];

    public override async Task HandleMessage(string message)
    {
        var asset = AssetManager.ReadAll(AssetType.LandTitle, CurrentUser.Id).First(x => x.IsDraft);

        switch (message)
        {
            case var m when MessageEquals(m, 6, "Cancel"):
                AssetManager.Delete(asset);
                NextStage = New<Start>();
                return;

            case var m when MessageEquals(m, 34, "Get Credit"):
                var currentUserPerson = PersonManager.Read(CurrentUser.Id);
                var delta = asset.Price - currentUserPerson.Cash;
                var credit = (int)Math.Ceiling(delta / 1_000d) * 1_000;
                CurrentUser.GetCredit(credit);
                await CompleteTransaction(asset);

                NextStage = New<Start>();
                return;
        }
    }
}
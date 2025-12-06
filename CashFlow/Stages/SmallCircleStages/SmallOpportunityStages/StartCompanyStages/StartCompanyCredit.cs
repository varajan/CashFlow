using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.StartCompanyStages;

public class StartCompanyCredit(
    ITermsService termsService,
    IAvailableAssets assets,
    IAssetManager assetManager,
    IPersonManager personManager,
    IHistoryManager historyManager) : StartCompanyPrice(termsService, assets, assetManager, personManager, historyManager)
{
    public override string Message
    {
        get
        {
            var asset = AssetManager.ReadAll(AssetType.SmallBusinessType, CurrentUser.Id).Single(x => x.IsDraft);
            var person = PersonManager.Read(CurrentUser.Id);
            var value = asset.Price.AsCurrency();
            var cash = person.Cash.AsCurrency();
            return Terms.Get(23, CurrentUser, "You don''t have {0}, but only {1}", value, cash);
        }
    }

    public override IEnumerable<string> Buttons => [Terms.Get(34, CurrentUser, "Get Credit"), Cancel];

    public override async Task HandleMessage(string message)
    {
        var asset = AssetManager.ReadAll(AssetType.SmallBusinessType, CurrentUser.Id).Single(x => x.IsDraft);

        switch (message)
        {
            case var m when MessageEquals(m, 6, "Cancel"):
                AssetManager.Delete(asset);
                NextStage = New<Start>();
                return;

            case var m when MessageEquals(m, 34, "Get Credit"):
                var person = PersonManager.Read(CurrentUser.Id);
                var delta = asset.Price - person.Cash;
                var credit = (int)Math.Ceiling(delta / 1_000d) * 1_000;

                CurrentUser.GetCredit(credit);
                await CompleteTransaction();

                NextStage = New<Start>();
                return;
        }
    }
}

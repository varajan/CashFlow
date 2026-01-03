using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Data;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.BuyCoinsStages;

public class BuyCoinsCredit(
    ITermsService termsService,
    IAvailableAssets assets,
    IHistoryManager historyManager,
    IPersonManager personManager) : BuyCoinsPrice(termsService, assets, historyManager, personManager)
{
    public override string Message
    {
        get
        {
            var asset = PersonManager.ReadAllAssets(AssetType.Coin, CurrentUser).First(x => x.IsDraft);
            var value = (asset.Qtty * asset.Price).AsCurrency();
            var cash = PersonManager.Read(CurrentUser).Cash.AsCurrency();

            return Terms.Get(23, CurrentUser, "You don''t have {0}, but only {1}", value, cash);
        }
    }

    public override IEnumerable<string> Buttons => [GetCredit, Cancel];

    public override async Task HandleMessage(string message)
    {
        var asset = PersonManager.ReadAllAssets(AssetType.Coin, CurrentUser).First(x => x.IsDraft);

        switch (message)
        {
            case var m when MessageEquals(m, 6, "Cancel"):
                PersonManager.DeleteAsset(asset);
                NextStage = New<Start>();
                return;

            case var m when MessageEquals(m, 34, "Get Credit"):
                var person = PersonManager.Read(CurrentUser);
                var delta = asset.Price * asset.Qtty - person.Cash;
                var credit = (int)Math.Ceiling(delta / 1_000d) * 1_000;

                CurrentUser.GetCredit_OBSOLETE(credit);
                await CompleteTransaction(asset);

                NextStage = New<Start>();
                return;
        }
    }
}
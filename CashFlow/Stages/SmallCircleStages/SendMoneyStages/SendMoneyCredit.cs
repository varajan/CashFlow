using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.SendMoneyStages;

public class SendMoneyCredit(
    IAssetManager assetManager,
    IPersonManager personManager,
    ITermsService termsService,
    IAvailableAssets availableAssets) : SendMoneyAmount(assetManager, personManager, termsService, availableAssets)
{
    public override string Message
    {
        get
        {
            var asset = PersonManager.ReadAllAssets(AssetType.Transfer, CurrentUser).First(x => x.IsDraft);
            var currentUserPerson = PersonManager.Read(CurrentUser);
            var value = asset.Qtty.AsCurrency();
            var cash = currentUserPerson.Cash.AsCurrency();
            return Terms.Get(23, CurrentUser, "You don''t have {0}, but only {1}", value, cash);
        }
    }

    public override IEnumerable<string> Buttons => [GetCredit, Cancel];

    public override async Task HandleMessage(string message)
    {
        var asset = PersonManager.ReadAllAssets(AssetType.Transfer, CurrentUser).First(x => x.IsDraft);

        switch (message)
        {
            case var m when MessageEquals(m, 6, "Cancel"):
                PersonManager.DeleteAsset(asset);
                NextStage = New<Start>();
                return;

            case var m when MessageEquals(m, 34, "Get Credit"):
                var currentUserPerson = PersonManager.Read(CurrentUser);
                var delta = asset.Qtty - currentUserPerson.Cash;
                var credit = (int)Math.Ceiling(delta / 1_000d) * 1_000;
                var person = PersonManager.Read(CurrentUser);
                
                person.GetCredit(credit);
                PersonManager.Update(person);
                PersonManager.AddHistory(ActionType.Credit, credit, CurrentUser);
                await CurrentUser.Notify(Terms.Get(88, CurrentUser, "You've taken {0} from bank.", credit.AsCurrency()));
                await Transfer(asset);

                NextStage = New<Start>();
                return;
        }
    }
}

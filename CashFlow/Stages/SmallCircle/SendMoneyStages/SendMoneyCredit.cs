using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Data;
using CashFlow.Extensions;

namespace CashFlow.Stages;

public class SendMoneyCredit(
    IAssetManager assetManager,
    IPersonManager personManager,
    IHistoryManager historyManager,
    ITermsService termsService) : SendMoneyAmount(assetManager, personManager, historyManager, termsService)
{
    public override string Message
    {
        get
        {
            var asset = AssetManager.ReadAll(AssetType.Transfer, CurrentUser.Id).First(x => x.IsDraft);
            var currentUserPerson = PersonManager.Read(CurrentUser.Id);
            var value = asset.Qtty.AsCurrency();
            var cash = currentUserPerson.Cash.AsCurrency();
            return Terms.Get(23, CurrentUser, "You don''t have {0}, but only {1}", value, cash);
        }
    }

    public override IEnumerable<string> Buttons => [Terms.Get(34, CurrentUser, "Get Credit"), Cancel];

    public override async Task HandleMessage(string message)
    {
        var asset = AssetManager.ReadAll(AssetType.Transfer, CurrentUser.Id).First(x => x.IsDraft);

        switch (message)
        {
            case var m when MessageEquals(m, 6, "Cancel"):
                AssetManager.Delete(asset);
                NextStage = New<Start>();
                return;

            case var m when MessageEquals(m, 34, "Get Credit"):
                var currentUserPerson = PersonManager.Read(CurrentUser.Id);
                var delta = asset.Qtty - currentUserPerson.Cash;
                var credit = (int)Math.Ceiling(delta / 1_000d) * 1_000;
                CurrentUser.GetCredit(credit);
                await Transfer(asset);

                NextStage = New<Start>();
                return;
        }
    }
}

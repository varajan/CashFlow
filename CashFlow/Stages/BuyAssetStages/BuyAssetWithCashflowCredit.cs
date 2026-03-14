using CashFlow.Data.Consts;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.BuyAssetStages;

public abstract class BuyAssetWithCashflowCredit<TNextStage>(
    AssetType assetName,
    AssetType assetType,
    ITermsRepository termsService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager)
    : BuyAsset<TNextStage>(assetName, assetType, termsService, availableAssets, personManager) where TNextStage : BaseStage
{
    public override string Message
    {
        get
        {
            var person = PersonManager.Read(CurrentUser);
            var asset = PersonManager.ReadAllAssets(AssetType, CurrentUser).Single(x => x.IsDraft);
            var amount = asset.Price * asset.Qtty - asset.Mortgage;

            return Terms.Get(23, CurrentUser, "You don''t have {0}, but only {1}", amount.AsCurrency(), person.Cash.AsCurrency());
        }
    }

    public override IEnumerable<string> Buttons => [GetCredit, Cancel];

    public override async Task HandleMessage(string message)
    {
        var asset = PersonManager.ReadAllAssets(AssetType, CurrentUser).Single(x => x.IsDraft);

        switch (message)
        {
            case var m when MessageEquals(m, 6, "Cancel"):
                PersonManager.DeleteAsset(CurrentUser, asset);
                NextStage = New<Start>();
                return;

            case var m when MessageEquals(m, 34, "Get Credit"):
                var person = PersonManager.Read(CurrentUser);
                var delta = asset.Price * asset.Qtty - asset.Mortgage - person.Cash;
                var credit = (int)Math.Ceiling(delta / 1_000d) * 1_000;

                person.GetCredit(credit);
                PersonManager.Update(person);
                PersonManager.AddHistory(ActionType.Credit, credit, CurrentUser);
                await CurrentUser.Notify(Terms.Get(88, CurrentUser, "You've taken {0} from bank.", credit.AsCurrency()));

                NextStage = New<TNextStage>();
                return;

            default:
                return;
        }
    }
}
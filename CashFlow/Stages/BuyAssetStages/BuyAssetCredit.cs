using CashFlow.Data.Consts;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.BuyAssetStages;

public abstract class BuyAssetCredit<TNextStage>(
    AssetType assetName,
    AssetType assetType,
    ActionType actionType,
    ITermsRepository termsService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository)
    : BuyAssetFirstPayment<TNextStage>(assetName, assetType, actionType, termsService, availableAssets, personManager, userRepository) where TNextStage : BaseStage
{
    public override string Message
    {
        get
        {
            var person = PersonService.Read(CurrentUser);
            var asset = PersonService.ReadAllAssets(AssetType, CurrentUser).Single(x => x.IsDraft);
            var amount = asset.Price * asset.Qtty - asset.Mortgage;

            return Terms.Get(23, CurrentUser, "You don''t have {0}, but only {1}", amount.AsCurrency(), person.Cash.AsCurrency());
        }
    }

    public override IEnumerable<string> Buttons => [GetCredit, Cancel];

    public override async Task HandleMessage(string message)
    {
        var asset = PersonService.ReadAllAssets(AssetType, CurrentUser).Single(x => x.IsDraft);

        switch (message)
        {
            case var m when MessageEquals(m, 6, "Cancel"):
                PersonService.DeleteAsset(CurrentUser, asset);
                NextStage = New<Start>();
                return;

            case var m when MessageEquals(m, 34, "Get Credit"):
                var person = PersonService.Read(CurrentUser);
                var delta = asset.Price * asset.Qtty - asset.Mortgage - person.Cash;
                var credit = (int)Math.Ceiling(delta / 1_000d) * 1_000;

                person.GetCredit(credit);
                PersonService.Update(person);
                PersonService.AddHistory(ActionType.Credit, credit, CurrentUser);
                await CurrentUser.Notify(Terms.Get(88, CurrentUser, "You've taken {0} from bank.", credit.AsCurrency()));
                await CompleteTransaction(asset);

                NextStage = New<Start>();
                return;
        }
    }
}
using CashFlow.Data.Consts;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.BuyAssetStages;

public abstract class BuyAssetWithCashflowCredit<TNextStage>(
    AssetType assetName,
    AssetType assetType,
    ITranslationService termsService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository)
    : BuyAsset<TNextStage>(assetName, assetType, termsService, availableAssets, personManager, userRepository) where TNextStage : BaseStage
{
    public override string Message
    {
        get
        {
            var person = PersonService.Read(CurrentUser);
            var asset = PersonService.ReadAllAssets(AssetType, CurrentUser).Single(x => x.IsDraft);
            var amount = asset.Price * asset.Qtty - asset.Mortgage;

            return TranslationService.Get(Terms.NotEnoughAmount, CurrentUser, amount.AsCurrency(), person.Cash.AsCurrency());
        }
    }

    public override IEnumerable<string> Buttons => [GetCredit, Cancel];

    public override async Task HandleMessage(string message)
    {
        var asset = PersonService.ReadAllAssets(AssetType, CurrentUser).Single(x => x.IsDraft);

        switch (message)
        {
            case var m when MessageEquals(m, Terms.Cancel):
                PersonService.DeleteAsset(CurrentUser, asset);
                NextStage = New<Start>();
                return;

            case var m when MessageEquals(m, Terms.GetCredit):
                var person = PersonService.Read(CurrentUser);
                var delta = asset.Price * asset.Qtty - asset.Mortgage - person.Cash;
                var credit = (int)Math.Ceiling(delta / 1_000d) * 1_000;

                person.GetCredit(credit);
                PersonService.Update(person);
                PersonService.AddHistory(ActionType.Credit, credit, CurrentUser);
                await CurrentUser.Notify(TranslationService.Get(Terms.TookLoan, CurrentUser, credit.AsCurrency()));

                NextStage = New<TNextStage>();
                return;

            default:
                return;
        }
    }
}
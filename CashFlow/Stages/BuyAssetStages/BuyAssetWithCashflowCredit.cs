using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.BuyAssetStages;

public abstract class BuyAssetWithCashflowCredit<TNextStage>(
    AssetType assetName,
    AssetType assetType,
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IPersonManager personManager)
    : BuyAsset<TNextStage>(assetName, assetType, termsService, availableAssets, personManager) where TNextStage : BaseStage
{
    public override string Message
    {
        get
        {
            var person = PersonManager.Read(CurrentUser.Id);
            var asset = PersonManager.ReadAllAssets(AssetType, CurrentUser.Id).Single(x => x.IsDraft);
            var amount = asset.Price * asset.Qtty - asset.Mortgage;

            return Terms.Get(23, CurrentUser, "You don''t have {0}, but only {1}", amount.AsCurrency(), person.Cash.AsCurrency());
        }
    }

    public override IEnumerable<string> Buttons => [GetCredit, Cancel];

    public override Task HandleMessage(string message)
    {
        var asset = PersonManager.ReadAllAssets(AssetType, CurrentUser.Id).Single(x => x.IsDraft);

        switch (message)
        {
            case var m when MessageEquals(m, 6, "Cancel"):
                PersonManager.DeleteAsset(asset);
                NextStage = New<Start>();
                return Task.CompletedTask;

            case var m when MessageEquals(m, 34, "Get Credit"):
                var person = PersonManager.Read(CurrentUser.Id);
                var delta = asset.Price * asset.Qtty - asset.Mortgage - person.Cash;
                var credit = (int)Math.Ceiling(delta / 1_000d) * 1_000;

                CurrentUser.GetCredit_OBSOLETE(credit);
                NextStage = New<TNextStage>();
                return Task.CompletedTask;

            default:
                return Task.CompletedTask;
        }
    }
}
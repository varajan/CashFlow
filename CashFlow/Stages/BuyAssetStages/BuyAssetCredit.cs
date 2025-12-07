using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.BuyAssetStages;

public abstract class BuyAssetCredit<TNextStage>(
    AssetType assetName,
    AssetType assetType,
    ActionType actionType,
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IAssetManager assetManager,
    IHistoryManager historyManager,
    IPersonManager personManager)
    : BuyAssetFirstPayment<TNextStage>(assetName, assetType, actionType, termsService, availableAssets, assetManager, historyManager, personManager) where TNextStage : BaseStage
{
    public override string Message
    {
        get
        {
            var person = PersonManager.Read(CurrentUser.Id);
            var asset = AssetManager.ReadAll(AssetType, CurrentUser.Id).Single(x => x.IsDraft);
            var firstPayment = asset.Price - asset.Mortgage;

            return Terms.Get(23, CurrentUser, "You don''t have {0}, but only {1}", firstPayment.AsCurrency(), person.Cash.AsCurrency());
        }
    }

    public override IEnumerable<string> Buttons => [GetCredit, Cancel];

    public override async Task HandleMessage(string message)
    {
        var asset = AssetManager.ReadAll(AssetType, CurrentUser.Id).Single(x => x.IsDraft);

        switch (message)
        {
            case var m when MessageEquals(m, 6, "Cancel"):
                AssetManager.Delete(asset);
                NextStage = New<Start>();
                return;

            case var m when MessageEquals(m, 34, "Get Credit"):
                var person = PersonManager.Read(CurrentUser.Id);
                var delta = asset.Price - asset.Mortgage - person.Cash;
                var credit = (int)Math.Ceiling(delta / 1_000d) * 1_000;

                CurrentUser.GetCredit(credit);
                await CompleteTransaction(asset);

                NextStage = New<Start>();
                return;
        }
    }
}
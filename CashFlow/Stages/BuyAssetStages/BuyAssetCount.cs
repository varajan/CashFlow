using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.BuyAssetStages;

public class BuyAssetCount<TNextStage>(
    AssetType assetName,
    AssetType assetType,
    ActionType actionType,
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IHistoryManager historyManager,
    IPersonManager personManager)
    : BuyAsset<TNextStage>(assetName, assetType, termsService, availableAssets, personManager) where TNextStage : BaseStage
{
    protected ActionType ActionType { get; } = actionType;
    protected IHistoryManager HistoryManager { get; } = historyManager;

    public override string Message
    {
        get
        {
            var asset = PersonManager.ReadAllAssets(AssetType, CurrentUser.Id).First(x => x.IsDraft);
            var person = PersonManager.Read(CurrentUser.Id);
            int upToQtty = person.Cash / asset.Price;

            return upToQtty == 0
                ? Terms.Get(21, CurrentUser, "How much?")
                : Terms.Get(17, CurrentUser, "You can buy up to {0} stocks. How much stocks would you like to buy?", upToQtty);
        }
    }

    public override IEnumerable<string> Buttons
    {
        get
        {
            var asset = PersonManager.ReadAllAssets(AssetType, CurrentUser.Id).First(x => x.IsDraft);
            var person = PersonManager.Read(CurrentUser.Id);
            int upToQtty = person.Cash / asset.Price;
            int upTo50 = upToQtty / 50 * 50;
            var isSimple = asset.Price < 1000;

            var buttons = new List<int>();

            if (upToQtty == 0)
            {
                buttons.AddRange(isSimple ? new[] { 100, 150, 200 } : new[] { 1, 2, 3, 4 });
            }
            else if (isSimple)
            {
                buttons.AddRange([upToQtty, upTo50, upTo50 - 50, upTo50 - 100]);
            }
            else
            {
                buttons.AddRange([upToQtty, upToQtty - 1, upToQtty - 2, upToQtty - 3]);
            }

            return buttons
                .Where(x => x > 0)
                .Distinct()
                .OrderBy(x => x)
                .Select(x => x.ToString())
                .Append(Cancel);
        }
    }

    public async override Task HandleMessage(string message)
    {
        var asset = PersonManager.ReadAllAssets(AssetType, CurrentUser.Id).First(x => x.IsDraft);

        if (IsCanceled(message))
        {
            PersonManager.DeleteAsset(asset);
            NextStage = New<Start>();
            return;
        }

        var number = message.AsCurrency();
        if (number <= 0)
        {
            await CurrentUser.Notify(Terms.Get(19, CurrentUser, "Invalid quantity value. Try again."));
            return;
        }

        asset.Qtty = number;
        PersonManager.UpdateAsset(asset);

        var person = PersonManager.Read(CurrentUser.Id);
        if (person.Cash < asset.Qtty * asset.Price)
        {

            NextStage = New<TNextStage>();
            return;
        }

        await CompleteTransaction(asset);
        NextStage = New<Start>();
    }

    protected async Task CompleteTransaction(AssetDto asset)
    {
        var person = PersonManager.Read(CurrentUser.Id);
        person.Cash -= asset.Price * asset.Qtty;
        PersonManager.Update(person);

        asset.IsDraft = false;
        PersonManager.UpdateAsset(asset);

        HistoryManager.Add(ActionType, asset.Id, CurrentUser);

        await CurrentUser.Notify(Terms.Get(13, CurrentUser, "Done."));
    }
}

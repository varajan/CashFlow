using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Data.Repositories;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.BuyAssetStages;

public class BuyAssetCount<TCreditStage, TCashFlowStage>(
    AssetType assetName,
    AssetType assetType,
    ActionType actionType,
    ITermsRepository termsService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository)
    : BuyAsset<TCreditStage>(assetName, assetType, termsService, availableAssets, personManager, userRepository)
        where TCreditStage : BaseStage
        where TCashFlowStage : BaseStage
{
    protected ActionType ActionType { get; } = actionType;

    public override string Message
    {
        get
        {
            var asset = PersonManager.ReadAllAssets(AssetType, CurrentUser).First(x => x.IsDraft);
            var person = PersonManager.Read(CurrentUser);
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
            var asset = PersonManager.ReadAllAssets(AssetType, CurrentUser).First(x => x.IsDraft);
            var person = PersonManager.Read(CurrentUser);
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
        var asset = PersonManager.ReadAllAssets(AssetType, CurrentUser).First(x => x.IsDraft);

        if (IsCanceled(message))
        {
            PersonManager.DeleteAsset(CurrentUser, asset);
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
        PersonManager.UpdateAsset(CurrentUser, asset);

        var person = PersonManager.Read(CurrentUser);
        if (person.Cash < asset.Qtty * asset.Price)
        {
            NextStage = New<TCreditStage>();
            return;
        }

        if (asset.Price > 1000)
        {
            NextStage = New<TCashFlowStage>();
            return;
        }

        await CompleteTransaction(asset);
        NextStage = New<Start>();
    }

    protected async Task CompleteTransaction(AssetDto asset)
    {
        var person = PersonManager.Read(CurrentUser);
        person.Cash -= asset.Price * asset.Qtty;
        PersonManager.Update(person);

        asset.IsDraft = false;
        PersonManager.UpdateAsset(CurrentUser, asset);

        PersonManager.AddHistory(ActionType, asset.Qtty, CurrentUser, asset.Id);

        await CurrentUser.Notify(Terms.Get(13, CurrentUser, "Done."));
    }
}

using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Data;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.BuyCoinsStages;

public class BuyCoinsPrice(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IPersonManager personManager) : BuyCoins(termsService, availableAssets, personManager)
{
    public override string Message => Terms.Get(8, CurrentUser, "What is the price?");
    public override IEnumerable<string> Buttons => AvailableAssets.GetAsCurrency(AssetType.CoinBuyPrice).Append(Cancel);

    public override async Task HandleMessage(string message)
    {
        if (IsCanceled(message))
        {
            NextStage = New<Start>();
            return;
        }

        var number = message.AsCurrency();

        if (number <= 0)
        {
            await CurrentUser.Notify(Terms.Get(9, CurrentUser, "Invalid price value. Try again."));
            return;
        }

        var asset = PersonManager.ReadAllAssets(AssetType.Coin, CurrentUser).Single(x => x.IsDraft);
        asset.Price = number;
        PersonManager.UpdateAsset(CurrentUser, asset);

        var person = PersonManager.Read(CurrentUser);
        if (person.Cash < asset.Price * asset.Qtty)
        {
            NextStage = New<BuyCoinsCredit>();
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

        PersonManager.AddHistory(ActionType.BuyCoins, asset.Id, CurrentUser);

        await CurrentUser.Notify(Terms.Get(13, CurrentUser, "Done."));
    }
}

using CashFlow.Data.Consts;
using CashFlow.Data.Consts.Terms;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.BuyCoinsStages;

public class BuyCoinsPrice(
    ITranslationService termsService,
    IUserService userService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository) : BuyCoins(termsService, userService, availableAssets, personManager, userRepository)
{
    public override string Message => TranslationService.Get(Terms.AskPrice, CurrentUser);
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
            await UserService.Notify(CurrentUser, TranslationService.Get(Terms.InvalidPrice, CurrentUser));
            return;
        }

        var asset = PersonService.ReadAllAssets(AssetType.Coin, CurrentUser).Single(x => x.IsDraft);
        asset.Price = number;
        PersonService.UpdateAsset(CurrentUser, asset);

        var person = PersonService.Read(CurrentUser);
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
        var person = PersonService.Read(CurrentUser);

        person.Cash -= asset.Price * asset.Qtty;
        PersonService.Update(person);

        asset.IsDraft = false;
        PersonService.UpdateAsset(CurrentUser, asset);

        PersonService.AddHistory(ActionType.BuyCoins, asset.Qtty, CurrentUser, asset.Id);

        await UserService.Notify(CurrentUser, TranslationService.Get(Terms.Done, CurrentUser));
    }
}

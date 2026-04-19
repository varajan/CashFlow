using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.BuyAssetStages;

public abstract class BuyAssetFirstPayment<TNextStage>(
    int[] firstPayments,
    AssetType assetType,
    ActionType actionType,
    ITranslationService termsService,
    IUserService userService,
    IPersonService personManager,
    IUserRepository userRepository)
    : BuyAsset<TNextStage>(null, assetType, termsService, userService, personManager, userRepository) where TNextStage : BaseStage
{
    protected ActionType ActionType { get; } = actionType;

    public override string Message => TranslationService.Get(Terms.AskFirstPayment, CurrentUser);
    public override IEnumerable<string> Buttons => firstPayments.AsCurrency().Append(Cancel);

    public override async Task HandleMessage(string message)
    {
        var asset = PersonService.ReadActiveAssets(AssetType, CurrentUser).Single(x => x.IsDraft);

        if (IsCanceled(message))
        {
            PersonService.DeleteAsset(CurrentUser, asset);
            NextStage = New<Start>();
            return;
        }

        var number = message.AsCurrency();
        if (number < 0)
        {
            await UserService.Notify(CurrentUser, TranslationService.Get(Terms.InvalidFirstPayment, CurrentUser));
            NextStage = this;
            return;
        }

        asset.Mortgage = asset.Price - number;
        PersonService.UpdateAsset(CurrentUser, asset);

        var person = PersonService.Read(CurrentUser);
        if (person.Cash < number)
        {
            NextStage = New<TNextStage>();
            return;
        }

        await CompleteTransaction(asset);
        NextStage = New<Start>();
    }

    protected async Task CompleteTransaction(AssetDto asset)
    {
        var person = PersonService.Read(CurrentUser);
        var amount = (asset.Price * asset.Qtty) - asset.Mortgage;

        person.Cash -= amount;
        PersonService.Update(person);

        asset.IsDraft = false;
        PersonService.UpdateAsset(CurrentUser, asset);

        PersonService.AddHistory(ActionType, asset.Qtty, CurrentUser, asset.Id);

        await UserService.Notify(CurrentUser, TranslationService.Get(Terms.Done, CurrentUser));
    }
}

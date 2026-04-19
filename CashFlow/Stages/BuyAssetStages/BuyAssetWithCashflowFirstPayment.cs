using CashFlow.Data.Consts;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.BuyAssetStages;

public abstract class BuyAssetWithCashflowFirstPayment<TNextStage, TCreditStage>(
    int[] firtPayments,
    AssetType assetType,
    ITranslationService termsService,
    IUserService userService,
    IPersonService personManager,
    IUserRepository userRepository)
     : BaseStage(termsService, userService, personManager, userRepository)
        where TNextStage : BaseStage
        where TCreditStage : BaseStage
{
    protected AssetType AssetType { get; } = assetType;
    public override string Message => TranslationService.Get(Terms.AskFirstPayment, CurrentUser);
    public override IEnumerable<string> Buttons => firtPayments.OrderBy(x => x).AsCurrency().Append(Cancel);

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
        if ((number < 0 && asset.Type != AssetType.BigBusinessType) ||
            (number <= 0 && asset.Type == AssetType.BigBusinessType))
        {
            await UserService.Notify(CurrentUser, TranslationService.Get(Terms.InvalidFirstPayment, CurrentUser));
            NextStage = this;
            return;
        }

        asset.Mortgage = asset.Price - number;
        PersonService.UpdateAsset(CurrentUser, asset);

        var person = PersonService.Read(CurrentUser);
        if (person.Cash < number && asset.Type == AssetType.BigBusinessType)
        {
            PersonService.DeleteAsset(CurrentUser, asset);
            await UserService.Notify(CurrentUser, TranslationService.Get(Terms.NotEnoughMoney, CurrentUser));
        }

        NextStage = person.Cash < number
            ? New<TCreditStage>()
            : New<TNextStage>();
    }
}

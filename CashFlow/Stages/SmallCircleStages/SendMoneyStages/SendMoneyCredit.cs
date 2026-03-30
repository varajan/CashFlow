using CashFlow.Data.Consts;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.SendMoneyStages;

public class SendMoneyCredit(
    IPersonService personManager,
    ITranslationService termsService,
    IAvailableAssetsRepository availableAssets,
    IUserRepository userRepository) : SendMoneyAmount(personManager, termsService, availableAssets, userRepository)
{
    public override string Message
    {
        get
        {
            var asset = PersonService.ReadAllAssets(AssetType.Transfer, CurrentUser).First(x => x.IsDraft);
            var currentUserPerson = PersonService.Read(CurrentUser);
            var value = asset.Qtty.AsCurrency();
            var cash = currentUserPerson.Cash.AsCurrency();
            return TranslationService.Get(Terms.NotEnoughAmount, CurrentUser, value, cash);
        }
    }

    public override IEnumerable<string> Buttons => [GetCredit, Cancel];

    public override async Task HandleMessage(string message)
    {
        var asset = PersonService.ReadAllAssets(AssetType.Transfer, CurrentUser).First(x => x.IsDraft);

        switch (message)
        {
            case var m when MessageEquals(m, "Cancel"):
                PersonService.DeleteAsset(CurrentUser, asset);
                NextStage = New<Start>();
                return;

            case var m when MessageEquals(m, "Get Credit"):
                var currentUserPerson = PersonService.Read(CurrentUser);
                var delta = asset.Qtty - currentUserPerson.Cash;
                var credit = (int)Math.Ceiling(delta / 1_000d) * 1_000;
                var person = PersonService.Read(CurrentUser);
                
                person.GetCredit(credit);
                PersonService.Update(person);
                PersonService.AddHistory(ActionType.Credit, credit, CurrentUser);
                await CurrentUser.Notify(TranslationService.Get(Terms.TookLoan, CurrentUser, credit.AsCurrency()));
                await Transfer(asset);

                NextStage = New<Start>();
                return;
        }
    }
}

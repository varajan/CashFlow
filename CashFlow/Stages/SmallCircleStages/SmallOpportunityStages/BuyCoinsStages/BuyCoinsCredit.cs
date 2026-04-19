using CashFlow.Data.Consts;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.BuyCoinsStages;

public class BuyCoinsCredit(
    ITranslationService termsService, IUserService userService, IPersonService personManager, IUserRepository userRepository)
    : BuyCoinsPrice(termsService, userService, personManager, userRepository)
{
    public override string Message
    {
        get
        {
            var asset = PersonService.ReadAllAssets(AssetType.Coin, CurrentUser).First(x => x.IsDraft);
            var value = (asset.Qtty * asset.Price).AsCurrency();
            var cash = PersonService.Read(CurrentUser).Cash.AsCurrency();

            return TranslationService.Get(Terms.NotEnoughAmount, CurrentUser, value, cash);
        }
    }

    public override IEnumerable<string> Buttons => [GetCredit, Cancel];

    public override async Task HandleMessage(string message)
    {
        var asset = PersonService.ReadAllAssets(AssetType.Coin, CurrentUser).First(x => x.IsDraft);

        switch (message)
        {
            case var m when MessageEquals(m, Terms.Cancel):
                PersonService.DeleteAsset(CurrentUser, asset);
                NextStage = New<Start>();
                return;

            case var m when MessageEquals(m, Terms.GetCredit):
                var person = PersonService.Read(CurrentUser);
                var delta = (asset.Price * asset.Qtty) - person.Cash;
                var credit = (int)Math.Ceiling(delta / 1_000d) * 1_000;

                person.GetCredit(credit);
                PersonService.Update(person);
                PersonService.AddHistory(ActionType.Credit, credit, CurrentUser);
                await UserService.Notify(CurrentUser, TranslationService.Get(Terms.TookLoan, CurrentUser, credit.AsCurrency()));
                await CompleteTransaction(asset);

                NextStage = New<Start>();
                return;
        }
    }
}
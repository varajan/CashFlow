using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.DoodadsStages;

public class Doodads(ITranslationService termsService, IPersonService personManager, IUserRepository userRepository)
    : BaseStage(termsService, personManager, userRepository)
{
    public override string Message => TranslationService.Get("What do you want?", CurrentUser);

    public override List<string> Buttons =>
    [
        TranslationService.Get("Pay with Cash", CurrentUser),
        TranslationService.Get("Pay with Credit Card", CurrentUser),
        TranslationService.Get("Buy a boat", CurrentUser),
        Cancel
    ];

    public override async Task HandleMessage(string message)
    {
        if (IsCanceled(message))
        {
            NextStage = New<Start>();
            return;
        }

        switch (message)
        {
            case var m when MessageEquals(m, "Pay with Cash"):
                NextStage = New<PayWithCash>();
                return;

            case var m when MessageEquals(m, "Pay with Credit Card"):
                NextStage = New<PayWithCreditCard>();
                return;

            case var m when MessageEquals(m, "Buy a boat"):
                await BuyBoat();
                NextStage = New<Start>();
                return;
        }
    }

    private async Task BuyBoat()
    {
        const int firstPayment = 1_000;

        var boat = PersonService.ReadAllAssets(AssetType.Boat, CurrentUser).FirstOrDefault();
        if (boat != null)
        {
            await CurrentUser.Notify(TranslationService.Get("You already have a boat.", CurrentUser));
            return;
        }

        var person = PersonService.Read(CurrentUser);

        if (person.Cash < firstPayment)
        {
            person.GetCredit(firstPayment);
            PersonService.Update(person);
            PersonService.AddHistory(ActionType.Credit, firstPayment, CurrentUser);
            await CurrentUser.Notify(TranslationService.Get("You've taken {0} from bank.", CurrentUser, firstPayment.AsCurrency()));
        }

        boat = new AssetDto
        {
            Type = AssetType.Boat,
            UserId = CurrentUser.Id,
            CashFlow = -340,
            Price = 18_000,
            Mortgage = 17_000,
            IsDraft = false,
            Qtty = 1,
            Title = "Boat"
        };
        person.Cash -= firstPayment;
        person.UpdateLiability(Liability.Boat_Loan, boat.CashFlow, boat.Mortgage);

        PersonService.Update(person);
        PersonService.CreateAsset(CurrentUser, boat);
        PersonService.AddHistory(ActionType.BuyBoat, boat.Price, CurrentUser);

        var message = TranslationService.Get("You've bot a boat for {0} in credit, first payment is {1}, monthly payment is {2}", CurrentUser,
            boat.Price.AsCurrency(),
            firstPayment.AsCurrency(),
            Math.Abs(boat.CashFlow).AsCurrency());
        await CurrentUser.Notify(message);
    }
}

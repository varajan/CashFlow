using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.DoodadsStages;

public class Doodads(ITermsRepository termsService, IPersonService personManager, IUserRepository userRepository)
    : BaseStage(termsService, personManager, userRepository)
{
    public override string Message => Terms.Get(89, CurrentUser, "What do you want?");

    public override List<string> Buttons =>
    [
        Terms.Get(95, CurrentUser, "Pay with Cash"),
        Terms.Get(96, CurrentUser, "Pay with Credit Card"),
        Terms.Get(112, CurrentUser, "Buy a boat"),
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
            case var m when MessageEquals(m, 95, "Pay with Cash"):
                NextStage = New<PayWithCash>();
                return;

            case var m when MessageEquals(m, 96, "Pay with Credit Card"):
                NextStage = New<PayWithCreditCard>();
                return;

            case var m when MessageEquals(m, 112, "Buy a boat"):
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
            await CurrentUser.Notify(Terms.Get(113, CurrentUser, "You already have a boat."));
            return;
        }

        var person = PersonService.Read(CurrentUser);

        if (person.Cash < firstPayment)
        {
            person.GetCredit(firstPayment);
            PersonService.Update(person);
            PersonService.AddHistory(ActionType.Credit, firstPayment, CurrentUser);
            await CurrentUser.Notify(Terms.Get(88, CurrentUser, "You've taken {0} from bank.", firstPayment.AsCurrency()));
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

        var message = Terms.Get(117, CurrentUser,
            "You've bot a boat for {0} in credit, first payment is {1}, monthly payment is {2}",
            boat.Price.AsCurrency(),
            firstPayment.AsCurrency(),
            Math.Abs(boat.CashFlow).AsCurrency());
        await CurrentUser.Notify(message);
    }
}

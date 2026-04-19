using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.DoodadsStages;

public class Doodads(ITranslationService termsService, IUserService userService, IPersonService personManager, IUserRepository userRepository)
    : BaseStage(termsService, userService, personManager, userRepository)
{
    public override string Message => TranslationService.Get(Terms.WhatDoYouWant, CurrentUser);

    public override List<string> Buttons =>
    [
        TranslationService.Get(Terms.PayCash, CurrentUser),
        TranslationService.Get(Terms.PayCard, CurrentUser),
        TranslationService.Get(Terms.BuyBoat, CurrentUser),
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
            case var m when MessageEquals(m, Terms.PayCash):
                NextStage = New<PayWithCash>();
                return;

            case var m when MessageEquals(m, Terms.PayCard):
                NextStage = New<PayWithCreditCard>();
                return;

            case var m when MessageEquals(m, Terms.BuyBoat):
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
            await UserService.Notify(CurrentUser, TranslationService.Get(Terms.AlreadyBoat, CurrentUser));
            return;
        }

        var person = PersonService.Read(CurrentUser);

        if (person.Cash < firstPayment)
        {
            person.GetCredit(firstPayment);
            PersonService.Update(person);
            PersonService.AddHistory(ActionType.Credit, firstPayment, CurrentUser);
            await UserService.Notify(CurrentUser, TranslationService.Get(Terms.TookLoan, CurrentUser, firstPayment.AsCurrency()));
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
        person.UpdateLiability(Liability.BoatLoan, boat.CashFlow, boat.Mortgage);

        PersonService.Update(person);
        PersonService.CreateAsset(CurrentUser, boat);
        PersonService.AddHistory(ActionType.BuyBoat, boat.Price, CurrentUser);

        var message = TranslationService.Get(Terms.BoatBought, CurrentUser,
            boat.Price.AsCurrency(),
            firstPayment.AsCurrency(),
            Math.Abs(boat.CashFlow).AsCurrency());
        await UserService.Notify(CurrentUser, message);
    }
}

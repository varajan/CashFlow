using CashFlow.Data.Consts;
using CashFlow.Data.Users;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Extensions;
using CashFlow.Interfaces;
using System;

namespace CashFlow.Stages.BigCircleStages;

public class BigCircle(ITermsService termsService, IPersonManager personManager) : BaseStage(termsService, personManager)
{
    public override string Message => PersonManager.GetDescription(CurrentUser.Id);

    public override List<string> Buttons =>
    [
        Terms.Get(79, CurrentUser, "Pay Check"),
        Terms.Get(32, CurrentUser, "Get Money"),
        Terms.Get(33, CurrentUser, "Give Money"),
        Terms.Get(69, CurrentUser, "Divorce"),
        Terms.Get(70, CurrentUser, "Tax Audit"),
        Terms.Get(71, CurrentUser, "Lawsuit"),
        Terms.Get(74, CurrentUser, "Buy Business"),
        Terms.Get(140, CurrentUser, "Friends"),
        Terms.Get(2, CurrentUser, "History"),
        Terms.Get(41, CurrentUser, "Stop Game"),
    ];

    public override async Task HandleMessage(string message)
    {
        var person = PersonManager.Read(CurrentUser.Id);

        if (MessageEquals(message, 79, "Pay Check"))
        {
            person.Cash += person.CashFlow;
            PersonManager.Update(person);
            PersonManager.AddHistory(ActionType.GetMoney, person.CashFlow, CurrentUser);
            NextStage = New< BigCircle>();
            return;
        }

        if (MessageEquals(message, 32, "Get Money"))
        {
            throw new NotImplementedException();
            return;
        }

        if (MessageEquals(message, 33, "Give Money"))
        {
            throw new NotImplementedException();
            return;
        }

        if (MessageEquals(message, 69, "Divorce"))
        {
            var amount = person.Cash;
            person.Cash -= amount;
            PersonManager.Update(person);
            PersonManager.AddHistory(ActionType.Divorce, amount, CurrentUser);
            await CurrentUser.Notify(Terms.Get(72, CurrentUser, "You've lost {0}.", amount.AsCurrency()));
            return;
        }

        if (MessageEquals(message, 70, "Tax Audit"))
        {
            var amount = person.Cash / 2;
            person.Cash -= amount;
            PersonManager.Update(person);
            PersonManager.AddHistory(ActionType.TaxAudit, amount, CurrentUser);
            await CurrentUser.Notify(Terms.Get(72, CurrentUser, "You've lost {0}.", amount.AsCurrency()));
            return;
        }

        if (MessageEquals(message, 71, "Lawsuit"))
        {
            var amount = person.Cash / 2;
            person.Cash -= amount;
            PersonManager.Update(person);
            PersonManager.AddHistory(ActionType.Lawsuit, amount, CurrentUser);
            await CurrentUser.Notify(Terms.Get(72, CurrentUser, "You've lost {0}.", amount.AsCurrency()));
            return;
        }

        if (MessageEquals(message, 74, "Buy Business"))
        {
            throw new NotImplementedException();
            return;
        }

        if (MessageEquals(message, 140, "Friends"))
        {
            throw new NotImplementedException();
            return;
        }

        if (MessageEquals(message, 2, "History"))
        {
            throw new NotImplementedException();
            return;
        }

        if (MessageEquals(message, 41, "Stop Game"))
        {
            throw new NotImplementedException();
            return;
        }
    }
}
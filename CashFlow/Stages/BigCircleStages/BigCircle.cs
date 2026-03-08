using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Extensions;
using CashFlow.Interfaces;
using CashFlow.Stages.SmallCircleStages.SendMoneyStages;
using CashFlow.Stages.SmallCircleStages.ShowMyDataStages;

namespace CashFlow.Stages.BigCircleStages;

public class BigCircle(ITermsService termsService, IPersonManager personManager) : BaseStage(termsService, personManager)
{
    public override string Message
    {
        get
        {
            var person = PersonManager.Read(CurrentUser);

            return person.CurrentCashFlow >= person.TargetCashFlow
                ? Terms.Get(73, CurrentUser, "You are the winner!")
                : PersonManager.GetDescription(CurrentUser);
        }
    }

    public override List<string> Buttons
    {
        get
        {
            var person = PersonManager.Read(CurrentUser);

            return person.CurrentCashFlow >= person.TargetCashFlow
            ? [
                Terms.Get(2, CurrentUser, "History"),
                Terms.Get(41, CurrentUser, "Stop Game"),
            ]
            : [
                Terms.Get(79, CurrentUser, "Paycheck"),
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
        }
    }

    public override async Task BeforeStage()
    {
        var person = PersonManager.Read(CurrentUser);

        if (person.CurrentCashFlow < person.TargetCashFlow && !person.IsWinning)
        {
            return;
        }

        if (person.CurrentCashFlow >= person.TargetCashFlow && person.IsWinning)
        {
            return;
        }

        if (person.CurrentCashFlow < person.TargetCashFlow && person.IsWinning)
        {
            person.IsWinning = false;
            PersonManager.Update(person);
            return;
        }

        person.IsWinning = true;
        PersonManager.Update(person);

        var users = OtherUsers.Where(x => x.IsActive).ToList();
        var message = Terms.Get(148, CurrentUser, "{0} is the winner!", CurrentUser.Name);
        var notifyAll = users.Select(u => u.Notify(message));
        await Task.WhenAll(notifyAll);
    }

    public override async Task HandleMessage(string message)
    {
        var person = PersonManager.Read(CurrentUser);

        if (person.CurrentCashFlow >= person.TargetCashFlow)
        {
            await HandleWinGame(message);
            return;
        }
        else
        {
            await HandleBigCircleMessage(message);
            return;
        }
    }

    private Task HandleWinGame(string message)
    {
        if (MessageEquals(message, 2, "History"))
        {
            NextStage = New<History>();
            return Task.CompletedTask;
        }

        if (MessageEquals(message, 41, "Stop Game"))
        {
            NextStage = New<StopGame>();
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }

    private async Task HandleBigCircleMessage(string message)
    {
        var person = PersonManager.Read(CurrentUser);

        if (MessageEquals(message, 79, "Paycheck"))
        {
            person.Cash += person.CurrentCashFlow;
            PersonManager.Update(person);
            PersonManager.AddHistory(ActionType.GetMoney, person.CurrentCashFlow, CurrentUser);
            NextStage = New<BigCircle>();
            return;
        }

        if (MessageEquals(message, 32, "Get Money"))
        {
            NextStage = New<GetMoney>();
            return;
        }

        if (MessageEquals(message, 33, "Give Money"))
        {
            var transfer = new AssetDto
            {
                UserId = CurrentUser.Id,
                Type = AssetType.Transfer,
                Title = "Bank",
                IsDraft = true,
            };

            PersonManager.CreateAsset(CurrentUser, transfer);
            NextStage = New<SendMoneyAmount>();
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
            NextStage = New<BuyBigBusiness>();
            return;
        }

        if (MessageEquals(message, 140, "Friends"))
        {
            NextStage = New< Friends>();
            return;
        }

        if (MessageEquals(message, 2, "History"))
        {
            NextStage = New<History>();
            return;
        }

        if (MessageEquals(message, 41, "Stop Game"))
        {
            NextStage = New<StopGame>();
            return;
        }
    }
}
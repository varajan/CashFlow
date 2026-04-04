using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Interfaces;
using CashFlow.Stages.SmallCircleStages.SendMoneyStages;
using CashFlow.Stages.SmallCircleStages.ShowMyDataStages;

namespace CashFlow.Stages.BigCircleStages;

public class BigCircle(ITranslationService termsService, IPersonService personManager, IUserRepository userRepository)
    : BaseStage(termsService, personManager, userRepository)
{
    public override string Message
    {
        get
        {
            var person = PersonService.Read(CurrentUser);

            return person.GetBigCircleCashflow() >= person.TargetCashFlow
                ? TranslationService.Get(Terms.Winner, CurrentUser)
                : PersonService.GetDescription(CurrentUser);
        }
    }

    public override List<string> Buttons
    {
        get
        {
            var person = PersonService.Read(CurrentUser);

            return person.GetBigCircleCashflow() >= person.TargetCashFlow
            ? [ History, StopGame ]
            : [
                TranslationService.Get(Terms.Paycheck, CurrentUser),
                TranslationService.Get(Terms.GetMoney, CurrentUser),
                TranslationService.Get(Terms.GiveMoney, CurrentUser),
                TranslationService.Get(Terms.Divorce, CurrentUser),
                TranslationService.Get(Terms.TaxAudit, CurrentUser),
                TranslationService.Get(Terms.Lawsuit, CurrentUser),
                TranslationService.Get(Terms.BuyBusiness, CurrentUser),
                TranslationService.Get(Terms.Friends, CurrentUser),
                History,
                StopGame,
            ];
        }
    }

    public override async Task BeforeStage()
    {
        var person = PersonService.Read(CurrentUser);

        if (person.GetBigCircleCashflow() < person.TargetCashFlow && !person.IsWinning)
        {
            return;
        }

        if (person.GetBigCircleCashflow() >= person.TargetCashFlow && person.IsWinning)
        {
            return;
        }

        if (person.GetBigCircleCashflow() < person.TargetCashFlow && person.IsWinning)
        {
            person.IsWinning = false;
            PersonService.Update(person);
            return;
        }

        person.IsWinning = true;
        PersonService.Update(person);

        var users = OtherUsers.Where(x => x.IsActive()).ToList();
        var message = TranslationService.Get(Terms.WinnerName, CurrentUser, CurrentUser.Name);
        var notifyAll = users.Select(u => u.Notify(message));
        await Task.WhenAll(notifyAll);
    }

    public override async Task HandleMessage(string message)
    {
        var person = PersonService.Read(CurrentUser);

        if (person.GetBigCircleCashflow() >= person.TargetCashFlow)
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
        if (MessageEquals(message, Terms.History))
        {
            NextStage = New<History>();
            return Task.CompletedTask;
        }

        if (MessageEquals(message, Terms.StopGame))
        {
            NextStage = New<StopGame>();
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }

    private async Task HandleBigCircleMessage(string message)
    {
        var person = PersonService.Read(CurrentUser);

        if (MessageEquals(message, Terms.Paycheck))
        {
            person.Cash += person.GetBigCircleCashflow();
            PersonService.Update(person);
            PersonService.AddHistory(ActionType.GetMoney, person.GetBigCircleCashflow(), CurrentUser);
            NextStage = New<BigCircle>();
            return;
        }

        if (MessageEquals(message, Terms.GetMoney))
        {
            NextStage = New<GetMoney>();
            return;
        }

        if (MessageEquals(message, Terms.GiveMoney))
        {
            var transfer = new AssetDto
            {
                UserId = CurrentUser.Id,
                Type = AssetType.Transfer,
                Title = Terms.Bank,
                IsDraft = true,
            };

            PersonService.CreateAsset(CurrentUser, transfer);
            NextStage = New<SendMoneyAmount>();
            return;
        }

        if (MessageEquals(message, Terms.Divorce))
        {
            var amount = person.Cash;
            person.Cash -= amount;
            PersonService.Update(person);
            PersonService.AddHistory(ActionType.Divorce, amount, CurrentUser);
            await CurrentUser.Notify(TranslationService.Get(Terms.LostMoney, CurrentUser, amount.AsCurrency()));
            return;
        }

        if (MessageEquals(message, Terms.TaxAudit))
        {
            var amount = person.Cash / 2;
            person.Cash -= amount;
            PersonService.Update(person);
            PersonService.AddHistory(ActionType.TaxAudit, amount, CurrentUser);
            await CurrentUser.Notify(TranslationService.Get(Terms.LostMoney, CurrentUser, amount.AsCurrency()));
            return;
        }

        if (MessageEquals(message, Terms.Lawsuit))
        {
            var amount = person.Cash / 2;
            person.Cash -= amount;
            PersonService.Update(person);
            PersonService.AddHistory(ActionType.Lawsuit, amount, CurrentUser);
            await CurrentUser.Notify(TranslationService.Get(Terms.LostMoney, CurrentUser, amount.AsCurrency()));
            return;
        }

        if (MessageEquals(message, Terms.BuyBusiness))
        {
            NextStage = New<BuyBigBusiness>();
            return;
        }

        if (MessageEquals(message, Terms.Friends))
        {
            NextStage = New< Friends>();
            return;
        }

        if (MessageEquals(message, Terms.History))
        {
            NextStage = New<History>();
            return;
        }

        if (MessageEquals(message, Terms.StopGame))
        {
            NextStage = New<StopGame>();
            return;
        }
    }
}
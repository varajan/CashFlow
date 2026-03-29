using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.SendMoneyStages;

public class SendMoney(ITranslationService termsService, IPersonService personManager, IUserRepository userRepository)
    : BaseStage(termsService, personManager, userRepository)
{
    public override string Message => TranslationService.Get("Whom?", CurrentUser);

    public override IEnumerable<string> Buttons
    {
        get
        {
            var bank = TranslationService.Get("Bank", CurrentUser);
            var users = OtherUsers
                .Where(x => x.IsActive() && PersonService.Read(x) is { BigCircle: false })
                .Select(x => x.Name)
                .ToList();

            return users.Append(bank).Append(Cancel);
        }
    }

    public async override Task HandleMessage(string message)
    {
        if (IsCanceled(message))
        {
            NextStage = New<Start>();
            return;
        }

        if (MessageEquals(message, "Bank") ||
            OtherUsers.Any(x => x.IsActive() && PersonService.Read(x) is { BigCircle: false } && x.Name == message))
        {
            var transfer = new AssetDto
            {
                Title = message,
                UserId = CurrentUser.Id,
                Type = AssetType.Transfer,
                IsDraft = true,
            };

            PersonService.CreateAsset(CurrentUser, transfer);
            NextStage = New<SendMoneyAmount>();
            return;
        }

        await CurrentUser.Notify(TranslationService.Get("Not found.", CurrentUser));
    }
}

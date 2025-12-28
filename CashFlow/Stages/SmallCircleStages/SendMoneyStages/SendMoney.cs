using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.SendMoneyStages;

public class SendMoney(IAssetManager assetManager, ITermsService termsService, IPersonManager personManager) : BaseStage(termsService, personManager)
{
    private IAssetManager AssetManager { get; init; } = assetManager;

    public override string Message => Terms.Get(147, CurrentUser, "Whom?");

    public override IEnumerable<string> Buttons
    {
        get
        {
            var asset = AssetManager.ReadAll(AssetType.Transfer, CurrentUser.Id).FirstOrDefault(x => x.IsDraft);
            AssetManager.Delete(asset);

            var bank = Terms.Get(149, CurrentUser, "Bank");
            var users = OtherUsers.Where(x => x.IsActive && x.Person_OBSOLETE.Circle == Circle.Small).Select(x => x.Name).ToList();

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

        if (MessageEquals(message, 149, "Bank") || OtherUsers.Any(x => x.IsActive && x.Person_OBSOLETE.Circle == Circle.Small && x.Name == message))
        {
            var transfer = new AssetDto
            {
                Title = message,
                UserId = CurrentUser.Id,
                Type = AssetType.Transfer,
            };

            AssetManager.Create(transfer);
            NextStage = New<SendMoneyAmount>();
            return;
        }

        await CurrentUser.Notify(Terms.Get(145, CurrentUser, "Not found."));
    }
}

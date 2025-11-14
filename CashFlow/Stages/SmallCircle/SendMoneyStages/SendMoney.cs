using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Data.Users.UserData.PersonData;

namespace CashFlow.Stages;

public class SendMoney(IAssetManager assetManager, ITermsService termsService, IAvailableAssets assets) : BaseStage(termsService, assets)
{
    private IAssetManager AssetManager { get; init; } = assetManager;

    public override string Message => Terms.Get(147, CurrentUser, "Whom?");

    public override IEnumerable<string> Buttons
    {
        get
        {
            var asset = AssetManager.Read(AssetType.Transfer, CurrentUser.Id);
            AssetManager.Delete(asset);

            var bank = Terms.Get(149, CurrentUser, "Bank");
            var users = OtherUsers.Where(x => x.IsActive && x.Person.Circle == Circle.Small).Select(x => x.Name).ToList();

            return users.Append(bank).Append(Cancel);
        }
    }

    public async override Task HandleMessage(string message)
    {
        if (IsCanceled(message)) return;

        if (MessageEquals(message, 149, "Bank") || OtherUsers.Any(x => x.IsActive && x.Person.Circle == Circle.Small && x.Name == message))
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

using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Data;
using CashFlow.Extensions;

namespace CashFlow.Stages;

public class SendMoneyAmount(
    IAssetManager assetManager,
    IPersonManager personManager,
    IHistoryManager historyManager,
    ITermsService termsService) : BaseStage(termsService)
{
    protected IAssetManager AssetManager { get; init; } = assetManager;
    protected IPersonManager PersonManager { get; init; } = personManager;
    private IHistoryManager HistoryManager { get; init; } = historyManager;

    public override string Message => Terms.Get(21, CurrentUser, "How much?");

    public override IEnumerable<string> Buttons => Enumerable
        .Range(1, 8)
        .Select(x => (500 * x).AsCurrency())
        .Append(Cancel);

    public override async Task HandleMessage(string message)
    {
        var asset = AssetManager.ReadAll(AssetType.Transfer, CurrentUser.Id).First(x => x.IsDraft);

        if (IsCanceled(message))
        {
            AssetManager.Delete(asset);
            NextStage = New<Start>();
            return;
        }

        var amount = message.AsCurrency();

        if (amount <= 0)
        {
            await CurrentUser.Notify(Terms.Get(150, CurrentUser, "Invalid value. Try again."));
            return;
        }

        asset.Qtty = amount;
        AssetManager.Update(asset);

        var currentUserPerson = PersonManager.Read(CurrentUser.Id);
        if (currentUserPerson.Cash < amount)
        {
            NextStage = New<SendMoneyCredit>();
            return;
        }

        await Transfer(asset);
    }

    protected async Task Transfer(AssetDto asset)
    {
        var bank = Terms.Get(149, CurrentUser, "Bank");
        var amount = asset.Qtty;
        var friend = OtherUsers.FirstOrDefault(x => x.Name == asset.Title);
        var message = Terms.Get(146, CurrentUser, "{0} transferred {2} to {1}.",
            CurrentUser.Name,
            friend?.Name ?? bank,
            amount.AsCurrency());
        var users = OtherUsers
                .Where(x => x.IsActive)
                .Append(CurrentUser)
                .ToList();

        var currentUserPerson = PersonManager.Read(CurrentUser.Id);
        currentUserPerson.Cash -= amount;
        PersonManager.Update(currentUserPerson);
        HistoryManager.Add(ActionType.PayMoney, amount, CurrentUser);

        if (friend is not null)
        {
            var friendPerson = PersonManager.Read(friend.Id);
            friendPerson.Cash += amount;
            PersonManager.Update(friendPerson);
            HistoryManager.Add(ActionType.GetMoney, amount, friend);
        }

        AssetManager.Delete(asset);

        var notifyAll = users.Select(u => u.Notify(message));
        await Task.WhenAll(notifyAll);
        NextStage = New<Start>();
    }
}

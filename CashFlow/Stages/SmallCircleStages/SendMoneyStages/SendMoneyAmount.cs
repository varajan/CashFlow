using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.SendMoneyStages;

public class SendMoneyAmount(
    IPersonService personManager,
    ITermsRepository termsService,
    IAvailableAssetsRepository availableAssets,
    IUserRepository userRepository) : BaseStage(termsService, personManager, userRepository)
{
    private IAvailableAssetsRepository AvailableAssets { get; } = availableAssets;

    public override string Message => Terms.Get(21, CurrentUser, "How much?");

    public override IEnumerable<string> Buttons
    {
        get
        {
            var person = PersonManager.Read(CurrentUser);

            return
                person.BigCircle
                ? AvailableAssets.GetAsCurrency(AssetType.BigGiveMoney).Append(Cancel)
                : Enumerable.Range(1, 8).Select(x => (500 * x).AsCurrency()).Append(Cancel);
        }
    }

    public override async Task HandleMessage(string message)
    {
        var asset = PersonManager.ReadAllAssets(AssetType.Transfer, CurrentUser).First(x => x.IsDraft);
        var person = PersonManager.Read(CurrentUser);

        if (IsCanceled(message))
        {
            PersonManager.DeleteAsset(CurrentUser, asset);
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
        PersonManager.UpdateAsset(CurrentUser, asset);

        if (!person.BigCircle && person.Cash < amount)
        {
            NextStage = New<SendMoneyCredit>();
            return;
        }

        if (person.BigCircle && person.Cash < amount)
        {
            PersonManager.DeleteAsset(CurrentUser, asset);
            NextStage = New<Start>();
            await CurrentUser.Notify(Terms.Get(5, CurrentUser, "You don't have enough money."));
            return;
        }

        if (person.BigCircle)
        {
            AvailableAssets.Add(amount, AssetType.BigGiveMoney);
        }

        await Transfer(asset);
    }

    protected async Task Transfer(AssetDto asset)
    {
        var bank = Terms.Get(149, CurrentUser, "Bank");
        var amount = asset.Qtty;
        var friend = OtherUsers.FirstOrDefault(x => x.Name == asset.Title);
        var message = Terms.Get(146, CurrentUser, "{0} transferred {2} to {1}.", CurrentUser.Name , friend?.Name ?? bank, amount.AsCurrency(), Environment.NewLine);
        var users = OtherUsers
                .Where(x => x.IsActive())
                .Append(CurrentUser)
                .ToList();

        var currentUserPerson = PersonManager.Read(CurrentUser);
        currentUserPerson.Cash -= amount;
        PersonManager.Update(currentUserPerson);
        PersonManager.AddHistory(ActionType.PayMoney, amount, CurrentUser);

        if (friend is not null)
        {
            var friendPerson = PersonManager.Read(friend);
            friendPerson.Cash += amount;
            PersonManager.Update(friendPerson);
            PersonManager.AddHistory(ActionType.GetMoney, amount, friend);
        }

        PersonManager.DeleteAsset(CurrentUser, asset);

        var notifyAll = users.Select(u => u.Notify(message));
        await Task.WhenAll(notifyAll);
        NextStage = New<Start>();
    }
}

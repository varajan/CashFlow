using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Data;
using CashFlow.Extensions;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.StartCompanyStages;

public class StartCompany(ITermsService termsService, IAvailableAssets availableAssets) : BaseStage(termsService)
{
    protected IAvailableAssets AvailableAssets { get; } = availableAssets;

    protected Asset_OLD Asset => CurrentUser.Person_OBSOLETE.Assets.SmallBusinesses.First(a => a.IsDraft);

    public override string Message => Terms.Get(7, CurrentUser, "Title:");
    public override IEnumerable<string> Buttons => AvailableAssets.GetAsText(AssetType.SmallBusinessType, CurrentUser.Language).Append(Cancel);

    public override Task HandleMessage(string message)
    {
        if (IsCanceled(message)) return Task.CompletedTask;

        CurrentUser.Person_OBSOLETE.Assets.Add(message, AssetType.SmallBusinessType);
        NextStage = New<StartCompanyPrice>();
        return Task.CompletedTask;
    }
}

public class StartCompanyPrice(ITermsService termsService, IAvailableAssets assets) : StartCompany(termsService, assets)
{
    public override string Message => Terms.Get(8, CurrentUser, "What is the price?");
    public override IEnumerable<string> Buttons => AvailableAssets.GetAsText(AssetType.SmallBusinessBuyPrice, CurrentUser.Language).Append(Cancel);

    public async override Task HandleMessage(string message)
    {
        if (IsCanceled(message)) return;

        var number = message.AsCurrency();
        if (number <= 0)
        {
            await CurrentUser.Notify(Terms.Get(9, CurrentUser, "Invalid price value. Try again."));
            return;
        }

        Asset.Price = number;

        if (CurrentUser.Person_OBSOLETE.Cash < number)
        {
            NextStage = New<StartCompanyCredit>();
            return;
        }

        await CompleteTransaction();
        NextStage = New<Start>();
    }

    protected async Task CompleteTransaction()
    {
        CurrentUser.Person_OBSOLETE.Cash -= Asset.Price;
        CurrentUser.History.Add(ActionType.StartCompany, Asset.Id);
        Asset.IsDraft = false;

        await CurrentUser.Notify(Terms.Get(13, CurrentUser, "Done."));
    }
}

public class StartCompanyCredit(ITermsService termsService, IAvailableAssets assets) : StartCompanyPrice(termsService, assets)
{
    public override string Message
    {
        get
        {
            var value = Asset.Price.AsCurrency();
            var cash = CurrentUser.Person_OBSOLETE.Cash.AsCurrency();
            return Terms.Get(23, CurrentUser, "You don''t have {0}, but only {1}", value, cash);
        }
    }

    public override IEnumerable<string> Buttons => [Terms.Get(34, CurrentUser, "Get Credit"), Cancel];

    public override async Task HandleMessage(string message)
    {
        switch (message)
        {
            case var m when MessageEquals(m, 6, "Cancel"):
                NextStage = New<Start>();
                return;

            case var m when MessageEquals(m, 34, "Get Credit"):
                var delta = Asset.Price - CurrentUser.Person_OBSOLETE.Cash;
                var credit = (int)Math.Ceiling(delta / 1_000d) * 1_000;

                CurrentUser.GetCredit(credit);
                await CompleteTransaction();

                NextStage = New<Start>();
                return;
        }
    }
}


using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Data;
using CashFlow.Interfaces;

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

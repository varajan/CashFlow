using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.StartCompanyStages;

public class StartCompany(ITermsService termsService, IAvailableAssets availableAssets, IAssetManager assetManager) : BaseStage(termsService)
{
    protected IAvailableAssets AvailableAssets { get; } = availableAssets;
    protected IAssetManager AssetManager { get; } = assetManager;

    protected Asset_OLD Asset => CurrentUser.Person_OBSOLETE.Assets.SmallBusinesses.First(a => a.IsDraft);

    public override string Message => Terms.Get(7, CurrentUser, "Title:");
    public override IEnumerable<string> Buttons => AvailableAssets.GetAsText(AssetType.SmallBusinessType, CurrentUser.Language).Append(Cancel);

    public override Task HandleMessage(string message)
    {
        if (IsCanceled(message))
        {
            NextStage = New<Start>();
            return Task.CompletedTask;
        }

        var companyName = AvailableAssets
            .GetAsText(AssetType.SmallBusinessType, CurrentUser.Language)
            .FirstOrDefault(x => x.Equals(message, StringComparison.InvariantCultureIgnoreCase));

        if (companyName is not null)
        {
            var draftCompany = new AssetDto
            {
                Title = companyName,
                BigCircle = false,
                Type = AssetType.SmallBusinessType,
                UserId = CurrentUser.Id,
                IsDraft = true,
            };

            AssetManager.Create(draftCompany);
            NextStage = New<StartCompanyPrice>();
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}

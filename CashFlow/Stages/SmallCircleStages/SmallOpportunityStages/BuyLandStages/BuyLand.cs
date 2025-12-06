using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.BuyLandStages;

public class BuyLand(ITermsService termsService, IAvailableAssets availableAssets, IAssetManager assetManager) : BaseStage(termsService)
{
    protected IAvailableAssets AvailableAssets { get; } = availableAssets;
    protected IAssetManager AssetManager { get; } = assetManager;

    public override string Message => Terms.Get(7, CurrentUser, "Title:");
    public override IEnumerable<string> Buttons => AvailableAssets.GetAsText(AssetType.LandTitle, CurrentUser.Language).Append(Cancel);

    public override Task HandleMessage(string message)
    {
        var asset = AssetManager.ReadAll(AssetType.LandTitle, CurrentUser.Id).FirstOrDefault(x => x.IsDraft);
        AssetManager.Delete(asset);

        if (IsCanceled(message))
        {
            NextStage = New<Start>();
            return Task.CompletedTask;
        }

        var title = AvailableAssets
            .GetAsText(AssetType.LandTitle, CurrentUser.Language)
            .FirstOrDefault(x => x.Equals(message, StringComparison.InvariantCultureIgnoreCase));

        if (title is not null)
        {
            var draftAsset = new AssetDto
            {
                Title = title,
                BigCircle = false,
                Type = AssetType.LandTitle,
                UserId = CurrentUser.Id,
                IsDraft = true,
            };

            AssetManager.Create(draftAsset);
            NextStage = New<BuyLandPrice>();
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}

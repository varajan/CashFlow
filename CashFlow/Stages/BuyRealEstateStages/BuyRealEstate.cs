using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Interfaces;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages;
using System.Text;

namespace CashFlow.Stages.BuyRealEstateStages;

public abstract class BuyRealEstate(bool small, ITermsService termsService, IAvailableAssets availableAssets, IAssetManager assetManager) : BaseStage(termsService)
{
    protected bool IsSmall { get; } = small;
    protected IAvailableAssets AvailableAssets { get; } = availableAssets;
    protected IAssetManager AssetManager { get; } = assetManager;

    public override string Message => Terms.Get(7, CurrentUser, "Title:");
    public override IEnumerable<string> Buttons => AvailableAssets
        .GetAsText(IsSmall ? AssetType.RealEstateSmallType : AssetType.RealEstateBigType, CurrentUser.Language)
        .OrderBy(x => x.Length)
        .ThenBy(x => x)
        .Append(Cancel);

    public override Task HandleMessage(string message)
    {
        if (IsCanceled(message))
        {
            NextStage = New<Start>();
            return Task.CompletedTask;
        }

        var title = AvailableAssets
            .GetAsText(IsSmall ? AssetType.RealEstateSmallType : AssetType.RealEstateBigType, CurrentUser.Language)
            .FirstOrDefault(x => x.Equals(message, StringComparison.InvariantCultureIgnoreCase));

        if (title is not null)
        {
            var draftAsset = new AssetDto
            {
                Title = title,
                BigCircle = false,
                Type = AssetType.RealEstate,
                UserId = CurrentUser.Id,
                IsDraft = true,
            };

            AssetManager.Create(draftAsset);
            NextStage = IsSmall ? New<BuySmallRealEstatePrice>() : throw new NotImplementedException();
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}

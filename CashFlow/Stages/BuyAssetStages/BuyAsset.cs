using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Interfaces;
using System.Text;

namespace CashFlow.Stages.BuyAssetStages;

public abstract class BuyAsset<TNextStage>(
    AssetType assetName,
    AssetType assetType,
    ITermsRepository termsService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository) : BaseStage(termsService, personManager, userRepository) where TNextStage : BaseStage
{
    protected AssetType AssetName { get; } = assetName;
    protected AssetType AssetType { get; } = assetType;

    protected IAvailableAssetsRepository AvailableAssets { get; } = availableAssets;

    public override string Message => Terms.Get(7, CurrentUser, "Title:");
    public override IEnumerable<string> Buttons => AvailableAssets
        .GetAsText(AssetName, CurrentUser.Language)
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
            .GetAsText(AssetName, CurrentUser.Language)
            .FirstOrDefault(x => x.Equals(message, StringComparison.InvariantCultureIgnoreCase));

        if (title is not null)
        {
            var draftAsset = new AssetDto
            {
                Title = title,
                BigCircle = (AssetType == AssetType.BigBusinessType),
                Qtty = 1,
                Type = AssetType,
                UserId = CurrentUser.Id,
                IsDraft = true,
            };

            PersonService.CreateAsset(CurrentUser, draftAsset);
            NextStage = New<TNextStage>();
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}

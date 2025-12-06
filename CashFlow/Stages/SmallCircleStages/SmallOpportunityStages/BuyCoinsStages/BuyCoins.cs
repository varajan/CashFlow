using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Data;
using System.Text;
using CashFlow.Data.DTOs;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.BuyCoinsStages;

public class BuyCoins(ITermsService termsService, IAvailableAssets availableAssets, IAssetManager assetManager) : BaseStage(termsService)
{
    protected IAvailableAssets AvailableAssets { get; } = availableAssets;
    protected IAssetManager AssetManager { get; } = assetManager;

    public override string Message => Terms.Get(7, CurrentUser, "Title:");

    public override IEnumerable<string> Buttons => AvailableAssets.GetAsText(AssetType.CoinTitle, CurrentUser.Language).Append(Cancel);

    public override Task HandleMessage(string message)
    {
        var asset = AssetManager.ReadAll(AssetType.Coin, CurrentUser.Id).FirstOrDefault(x => x.IsDraft);
        AssetManager.Delete(asset);

        if (IsCanceled(message))
        {
            NextStage = New<Start>();
            return Task.CompletedTask;
        }

        var coinTitle = AvailableAssets
            .GetAsText(AssetType.CoinTitle, CurrentUser.Language)
            .FirstOrDefault(x => x.Equals(message, StringComparison.InvariantCultureIgnoreCase));

        if (coinTitle is not null)
        {
            var draftCoinAsset = new AssetDto
            {
                Title = coinTitle,
                BigCircle = false,
                Type = AssetType.Coin,
                UserId = CurrentUser.Id,
                IsDraft = true,
            };

            AssetManager.Create(draftCoinAsset);
            NextStage = New<BuyCoinsCount>();
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}

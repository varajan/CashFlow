using CashFlow.Data.Consts;
using CashFlow.Extensions;
using CashFlow.Interfaces;
using MoreLinq;

namespace CashFlow.Stages.SmallCircleStages.MarketStages;

public class IncreaseCashflow(
    ITranslationService termsService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository) : BaseStage(termsService, personManager, userRepository)
{
    protected IAvailableAssetsRepository AvailableAssets { get; } = availableAssets;

    public override string Message => Terms.Get("What is the cash flow?", CurrentUser);

    public override IEnumerable<string> Buttons => AvailableAssets.GetAsCurrency(AssetType.IncreaseCashFlow).Append(Cancel);

    public override async Task HandleMessage(string message)
    {
        if (IsCanceled(message))
        {
            NextStage = New<Start>();
            return;
        }

        var cashflow = message.AsCurrency();
        if (cashflow <= 0)
        {
            await CurrentUser.Notify(Terms.Get("Invalid value. Try again.", CurrentUser));
            return;
        }

        var assets = PersonService.ReadAllAssets(AssetType.SmallBusinessType, CurrentUser).Where(a => !a.IsDeleted);

        assets.ForEach(asset =>
        {
            asset.CashFlow += cashflow;
            PersonService.UpdateAsset(CurrentUser, asset);
            PersonService.AddHistory(ActionType.IncreaseCashFlow, cashflow, CurrentUser, asset.Id);
        });

        await CurrentUser.Notify(Terms.Get("Done.", CurrentUser));
        NextStage = New<Start>();
    }
}

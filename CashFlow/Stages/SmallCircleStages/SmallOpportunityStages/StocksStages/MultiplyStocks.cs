using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.StocksStages;

public class StocksMultiply(ITermsService termsService, IAssetManager assetManager, IHistoryManager historyManager, IPersonManager personManager)
    : MultiplyStocks(ActionType.Stocks1To2, termsService, assetManager, historyManager, personManager) { }

public class StocksReduce(ITermsService termsService, IAssetManager assetManager, IHistoryManager historyManager, IPersonManager personManager)
    : MultiplyStocks(ActionType.Stocks2To1, termsService, assetManager, historyManager, personManager) { }

public abstract class MultiplyStocks(ActionType actionType, ITermsService termsService, IAssetManager assetManager, IHistoryManager historyManager, IPersonManager personManager)
    : BaseStage(termsService, personManager)
{
    protected ActionType ActionType { get; } = actionType;
    protected IAssetManager AssetManager { get; } = assetManager;
    protected IHistoryManager HistoryManager { get; } = historyManager;

    public override string Message => Terms.Get(7, CurrentUser, "Title:");

    public override IEnumerable<string> Buttons =>
        AssetManager
            .ReadAll(AssetType.Stock, CurrentUser.Id)
            .Select(x => x.Title)
            .Distinct()
            .Append(Cancel);

    public override async Task HandleMessage(string message)
    {
        if (IsCanceled(message))
        {
            NextStage = New<Start>();
            return;
        }

        var stocks = AssetManager.ReadAll(AssetType.Stock, CurrentUser.Id)
            .Where(x => x.Title.Equals(message, StringComparison.InvariantCultureIgnoreCase))
            .ToList();

        if (stocks.Count == 0)
        {
            await CurrentUser.Notify(Terms.Get(124, CurrentUser, "Invalid stocks name."));
            return;
        }

        var k = ActionType == ActionType.Stocks1To2 ? 2.0 : 0.5;
        stocks.ForEach(asset =>
        {
            asset.Qtty = (int)(asset.Qtty * k);
            AssetManager.Update(asset);
            HistoryManager.Add(ActionType, asset.Id, CurrentUser);
        });

        NextStage = New<Start>();
    }
}

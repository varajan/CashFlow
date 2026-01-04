using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.StocksStages;

public class StocksMultiply(ITermsService termsService, IHistoryManager historyManager, IPersonManager personManager)
    : MultiplyStocks(ActionType.Stocks1To2, termsService, historyManager, personManager) { }

public class StocksReduce(ITermsService termsService, IHistoryManager historyManager, IPersonManager personManager)
    : MultiplyStocks(ActionType.Stocks2To1, termsService, historyManager, personManager) { }

public abstract class MultiplyStocks(ActionType actionType, ITermsService termsService, IHistoryManager historyManager, IPersonManager personManager)
    : BaseStage(termsService, personManager)
{
    protected ActionType ActionType { get; } = actionType;
    protected IHistoryManager HistoryManager { get; } = historyManager;

    public override string Message => Terms.Get(7, CurrentUser, "Title:");

    public override IEnumerable<string> Buttons =>
        PersonManager
            .ReadAllAssets(AssetType.Stock, CurrentUser)
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

        var stocks = PersonManager.ReadAllAssets(AssetType.Stock, CurrentUser)
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
            PersonManager.UpdateAsset(asset);
            PersonManager.AddHistory(ActionType, asset.Id, CurrentUser);
        });

        NextStage = New<Start>();
    }
}

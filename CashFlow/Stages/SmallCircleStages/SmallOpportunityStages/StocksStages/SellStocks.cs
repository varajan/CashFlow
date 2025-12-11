using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.StocksStages;

public class SellStocks(ITermsService termsService, IAssetManager assetManager) : BaseStage(termsService)
{
    protected IAssetManager AssetManager { get; } = assetManager;

    public override string Message => Terms.Get(27, CurrentUser, "What stocks do you want to sell?");

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

        stocks.ForEach(stock =>
        {
            stock.MarkedToSell = true;
            AssetManager.Update(stock);
        });

        NextStage = New<SellStocksPrice>();
    }
}

public class SellStocksPrice(
    ITermsService termsService,
    IAssetManager assetManager,
    IAvailableAssets availableAssets,
    IPersonManager personManager,
    IHistoryManager historyManager)
    : SellStocks(termsService, assetManager)
{
    protected IPersonManager PersonManager { get; } = personManager;
    protected IHistoryManager HistoryManager { get; } = historyManager;
    protected IAvailableAssets AvailableAssets { get; } = availableAssets;

    public override string Message => Terms.Get(8, CurrentUser, "What is the price?");

    public override IEnumerable<string> Buttons => AvailableAssets.GetAsCurrency(AssetType.StockPrice).Append(Cancel);

    public override Task HandleMessage(string message)
    {
        // on cancel : update all markedToSell stocks as false 

        return base.HandleMessage(message);
    }
}
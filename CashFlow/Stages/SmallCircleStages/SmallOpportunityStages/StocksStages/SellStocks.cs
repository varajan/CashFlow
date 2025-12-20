using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Extensions;
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
    protected IAvailableAssets AvailableAssets { get; } = availableAssets;
    protected IHistoryManager HistoryManager { get; } = historyManager;

    public override string Message => Terms.Get(8, CurrentUser, "What is the price?");

    public override IEnumerable<string> Buttons => AvailableAssets.GetAsCurrency(AssetType.StockPrice).Append(Cancel);

    public override async Task HandleMessage(string message)
    {
        var stocks = AssetManager.ReadAll(AssetType.Stock, CurrentUser.Id).Where(x => x.MarkedToSell).ToList();

        if (IsCanceled(message))
        {
            NextStage = New<Start>();
            stocks.ForEach(stock =>
            {
                stock.MarkedToSell = false;
                AssetManager.Update(stock);
            });
            return;
        }

        var price = message.AsCurrency();
        if (price <= 0)
        {
            await CurrentUser.Notify(Terms.Get(9, CurrentUser, "Invalid price value. Try again."));
            return;
        }

        var person = PersonManager.Read(CurrentUser.Id);
        var qtty = stocks.Sum(s => s.Qtty);
        person.Cash += qtty * price;
        PersonManager.Update(person);
        stocks.ForEach(stock =>
        {
            AssetManager.Sell(stock, ActionType.SellStocks, price, CurrentUser);
            HistoryManager.Add(ActionType.SellStocks, stock.Id, CurrentUser);
        });

        await CurrentUser.Notify(Terms.Get(13, CurrentUser, "Done."));
        NextStage = New<Start>();
    }
}
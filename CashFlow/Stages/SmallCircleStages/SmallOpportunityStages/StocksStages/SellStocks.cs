using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Interfaces;
using CashFlow.Stages.SmallCircleStages.MarketStages;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.StocksStages;

public class SellStocks(ITermsService termsService, IAssetManager assetManager) : SellAsset<SellStocksPrice>(AssetType.Stock, termsService, assetManager)
{
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
    IAvailableAssets availableAssets,
    IAssetManager assetManager,
    IPersonManager personManager,
    IHistoryManager historyManager) : SellAssetPrice(AssetType.Stock, termsService, availableAssets, assetManager, personManager, historyManager)
{ }

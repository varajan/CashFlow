using CashFlow.Data.Consts;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.StocksStages;

public class StocksMultiply(ITranslationService termsService, IPersonService personManager, IUserRepository userRepository)
    : MultiplyStocks(ActionType.Stocks1To2, termsService, personManager, userRepository) { }

public class StocksReduce(ITranslationService termsService, IPersonService personManager, IUserRepository userRepository)
    : MultiplyStocks(ActionType.Stocks2To1, termsService, personManager, userRepository) { }

public abstract class MultiplyStocks(ActionType actionType, ITranslationService termsService, IPersonService personManager, IUserRepository userRepository)
    : BaseStage(termsService, personManager, userRepository)
{
    protected ActionType ActionType { get; } = actionType;

    public override string Message => TranslationService.Get(Terms.Title, CurrentUser);

    public override IEnumerable<string> Buttons =>
        PersonService
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

        var stocks = PersonService.ReadAllAssets(AssetType.Stock, CurrentUser)
            .Where(x => x.Title.Equals(message, StringComparison.InvariantCultureIgnoreCase))
            .ToList();

        if (stocks.Count == 0)
        {
            await CurrentUser.Notify(TranslationService.Get("Invalid stocks name.", CurrentUser));
            return;
        }

        var k = ActionType == ActionType.Stocks1To2 ? 2.0 : 0.5;
        stocks.ForEach(asset =>
        {
            asset.Qtty = (int)(asset.Qtty * k);
            PersonService.UpdateAsset(CurrentUser, asset);
            PersonService.AddHistory(ActionType, asset.Qtty, CurrentUser, asset.Id);
        });

        NextStage = New<Start>();
    }
}

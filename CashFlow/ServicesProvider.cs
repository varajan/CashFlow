using CashFlow.Data;
using CashFlow.Data.DataBase;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Loggers;
using Microsoft.Extensions.DependencyInjection;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.SendMoneyStages;
using CashFlow.Stages.SmallCircleStages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.BuyCoinsStages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.StartCompanyStages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.StocksStages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.BuyRealEstateStages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.BuyLandStages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages;

namespace CashFlow;

public static class ServicesProvider
{
    public static IServiceProvider Instance { get; private set; }

    public static void Init()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILogger, FileLogger>();
        services.AddSingleton<IDataBase, SQLiteDataBase>();
        services.AddSingleton<ITermsService, TermsService>();
        services.AddSingleton<IAvailableAssets, AvailableAssets>();
        services.AddSingleton<IAssetManager, AssetManager>();
        services.AddSingleton<IPersonManager, PersonManager>();
        services.AddSingleton<IHistoryManager, HistoryManager>();

        services.AddTransient<Start>();
        services.AddTransient<SmallCircle>();
        services.AddTransient<ChooseLanguage>();
        services.AddTransient<ChooseProfession>();
        services.AddTransient<History>();

        services.AddTransient<SmallOpportunity>();

        services.AddTransient<BuyStocks>();
        services.AddTransient<SellStocks>();
        services.AddTransient<StocksMultiply>();
        services.AddTransient<StocksReduce>();
        services.AddTransient<BuyRealEstate>();
        services.AddTransient<BuyLand>();

        services.AddTransient<BuyCoins>();
        services.AddTransient<BuyCoinsCount>();
        services.AddTransient<BuyCoinsPrice>();
        services.AddTransient<BuyCoinsCredit>();

        services.AddTransient<StartCompany>();
        services.AddTransient<StartCompanyPrice>();
        services.AddTransient<StartCompanyCredit>();

        services.AddTransient<SendMoney>();
        services.AddTransient<SendMoneyAmount>();
        services.AddTransient<SendMoneyCredit>();

        Instance = services.BuildServiceProvider();
    }

    public static T Get<T>() => Instance.GetRequiredService<T>();
    public static object Get(Type type) => Instance.GetRequiredService(type);
}

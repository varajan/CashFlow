using CashFlow.Data;
using CashFlow.Data.DataBase;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Interfaces;
using CashFlow.Loggers;
using CashFlow.Stages;
using CashFlow.Stages.BigCircleStages;
using CashFlow.Stages.SmallCircleStages;
using CashFlow.Stages.SmallCircleStages.BigOpportunityStages;
using CashFlow.Stages.SmallCircleStages.SendMoneyStages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.BuyCoinsStages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.BuyLandStages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.StartCompanyStages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.StocksStages;
using Microsoft.Extensions.DependencyInjection;

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
        services.AddTransient<Bankruptcy>();

        services.AddTransient<ShowMyData>();
        services.AddTransient<Friends>();
        services.AddTransient<SmallOpportunity>();
        services.AddTransient<BigOpportunity>();

        services.AddTransient<BuyStocks>();
        services.AddTransient<SellStocks>();
        services.AddTransient<StocksMultiply>();
        services.AddTransient<StocksReduce>();

        services.AddTransient<BuySmallRealEstate>();
        services.AddTransient<BuySmallRealEstatePrice>();
        services.AddTransient<BuySmallRealEstateFirstPayment>();
        services.AddTransient<BuySmallRealEstateCredit>();

        services.AddTransient<BuyLand>();
        services.AddTransient<BuyLandPrice>();
        services.AddTransient<BuyLandCredit>();

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

        services.AddTransient<BigCircle>();

        Instance = services.BuildServiceProvider();
    }

    public static T Get<T>() => Instance.GetRequiredService<T>();
    public static object Get(Type type) => Instance.GetRequiredService(type);
}

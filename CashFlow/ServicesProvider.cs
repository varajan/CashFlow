using CashFlow.Data;
using CashFlow.Data.Repositories;
using CashFlow.Data.Services;
using CashFlow.Interfaces;
using CashFlow.Loggers;
using CashFlow.Stages;
using CashFlow.Stages.BigCircleStages;
using CashFlow.Stages.SmallCircleStages;
using CashFlow.Stages.SmallCircleStages.BankruptcyStages;
using CashFlow.Stages.SmallCircleStages.BigOpportunityStages;
using CashFlow.Stages.SmallCircleStages.DoodadsStages;
using CashFlow.Stages.SmallCircleStages.MarketStages;
using CashFlow.Stages.SmallCircleStages.SendMoneyStages;
using CashFlow.Stages.SmallCircleStages.ShowMyDataStages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.BuyCoinsStages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.StocksStages;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CashFlow;

public static class ServicesProvider
{
    public static IServiceProvider Instance { get; private set; }
    private static readonly ServiceCollection services = [];

    public static void AddApplicationServices()
    {
        services.AddSingleton<ILogger, FileLogger>();
        services.AddSingleton<IDataBase, SQLiteDataBase>();
        services.AddSingleton<ITranslationService, TranslationService>();
        services.AddSingleton<IAvailableAssetsRepository, AvailableAssetsRepository>();
        services.AddSingleton<IUserRepository, UserRepository>();
        services.AddSingleton<IPersonRepository, PersonRepository>();
        services.AddSingleton<IPersonService, PersonService>();
        services.AddSingleton<IUserService, UserService>();

        services.AddTransient<Start>();
        services.AddTransient<GameMenu>();
        services.AddTransient<SmallCircle>();
        services.AddTransient<ChooseLanguage>();
        services.AddTransient<ChooseProfession>();
        services.AddTransient<History>();
        services.AddTransient<Bankruptcy>();
        services.AddTransient<BankruptcySellAssets>();

        services.AddTransient<ShowMyData>();
        services.AddTransient<GetMoney>();
        services.AddTransient<GetCredit>();
        services.AddTransient<ReduceLiabilities>();
        services.AddTransient<ReduceLiabilitiesAmount>();
        services.AddTransient<ReduceLiabilitiesConfirm>();
        services.AddTransient<StopGame>();

        services.AddTransient<Friends>();
        services.AddTransient<Doodads>();
        services.AddTransient<PayWithCash>();
        services.AddTransient<PayWithCreditCard>();

        services.AddTransient<SmallOpportunity>();

        services.AddTransient<BuyStocks>();
        services.AddTransient<BuyStocksPrice>();
        services.AddTransient<BuyStocksCount>();
        services.AddTransient<BuyStocksCredit>();
        services.AddTransient<BuyStocksCashFlow>();

        services.AddTransient<SellStocks>();
        services.AddTransient<SellStocksPrice>();

        services.AddTransient<StocksMultiply>();
        services.AddTransient<StocksReduce>();

        services.AddTransient<BuySmallRealEstate>();
        services.AddTransient<BuySmallRealEstatePrice>();
        services.AddTransient<BuySmallRealEstateFirstPayment>();
        services.AddTransient<BuySmallRealEstateCredit>();
        services.AddTransient<BuySmallRealEstateCashFlow>();

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

        services.AddTransient<BigOpportunity>();

        services.AddTransient<BuyBigRealEstate>();
        services.AddTransient<BuyBigRealEstatePrice>();
        services.AddTransient<BuyBigRealEstateFirstPayment>();
        services.AddTransient<BuyBigRealEstateCredit>();
        services.AddTransient<BuyBigRealEstateCashFlow>();

        services.AddTransient<BuyBusiness>();
        services.AddTransient<BuyBusinessPrice>();
        services.AddTransient<BuyBusinessFirstPayment>();
        services.AddTransient<BuyBusinessCredit>();
        services.AddTransient<BuyBusinessCashFlow>();

        services.AddTransient<Market>();
        services.AddTransient<SellRealEstate>();
        services.AddTransient<SellRealEstatePrice>();
        services.AddTransient<SellLand>();
        services.AddTransient<SellLandPrice>();
        services.AddTransient<SellBusiness>();
        services.AddTransient<SellBusinessPrice>();
        services.AddTransient<SellCoins>();
        services.AddTransient<SellCoinsPrice>();
        services.AddTransient<IncreaseCashflow>();

        services.AddTransient<BigCircle>();
        services.AddTransient<BuyBigBusiness>();
        services.AddTransient<BuyBigBusinessPrice>();
        services.AddTransient<BuyBigBusinessFirstPayment>();
        services.AddTransient<BuyBigBusinessCashFlow>();
        services.AddTransient<BuyDream>();

        Instance = services.BuildServiceProvider();
    }

    public static void AddMock<T>(Mock<T> mock) where T : class
    {
        var implementation = mock.Object;
        services.AddSingleton(typeof(T), implementation);
        Instance = services.BuildServiceProvider();
    }

    public static void Add<TImplementation>(TImplementation implemenation) where TImplementation : class
    {
        services.AddSingleton(implemenation);
        Instance = services.BuildServiceProvider();
    }

    public static T Get<T>() => Instance.GetRequiredService<T>();
    public static object Get(Type type) => Instance.GetRequiredService(type);
}

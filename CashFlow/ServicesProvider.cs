using CashFlow.Data.DataBase;
using CashFlow.Data;
using CashFlow.Loggers;
using Microsoft.Extensions.DependencyInjection;
using CashFlow.Stages;
using CashFlow.Data.Users.UserData.PersonData;
using AvailableAssets = CashFlow.Data.AvailableAssets;

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

        services.AddTransient<BuyCoins>();
        services.AddTransient<BuyCoinsCount>();
        services.AddTransient<BuyCoinsPrice>();
        services.AddTransient<BuyCoinsCredit>();

        services.AddTransient<SendMoney>();
        services.AddTransient<SendMoneyAmount>();
        services.AddTransient<SendMoneyCredit>();

        Instance = services.BuildServiceProvider();
    }

    public static T Get<T>() => Instance.GetRequiredService<T>();
    public static object Get(Type type) => Instance.GetRequiredService(type);
}

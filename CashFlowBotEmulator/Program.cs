using CashFlow;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Interfaces;
using CashFlow.Stages;
using CashFlow.Data.Repositories;
using CashFlowBotEmulator;

ServicesProvider.AddApplicationServices();
ServicesProvider.Add<INotifyService>(new EmulationNotifyService());

var Logger = ServicesProvider.Get<ILogger>();
var DataBase = ServicesProvider.Get<IDataBase>();
var PersonRepository = ServicesProvider.Get<IPersonRepository>();
var PersonManager = ServicesProvider.Get<IPersonService>();
var TermsService = ServicesProvider.Get<ITranslationService>();
var UserRepository = new UserRepository(DataBase);

while (true)
{
    if (!TryReadFirstFile(out int chatId, out string message, out string file))
    {
        Thread.Sleep(100);
        continue;
    }

    File.Delete(file!);

    if (message == "EXIT") break;

    if (message == "RESET")
    {
        var lastActive = DateTime.Now.AddHours(-1);
        PersonRepository.GetAll().ForEach(x => PersonRepository.Save(x, lastActive));
        Thread.Sleep(300);
        continue;
    }

    HandleUpdateAsync(chatId, message!).GetAwaiter().GetResult();
}

async Task HandleUpdateAsync(long chatId, string message)
{
    try
    {
        var user = UserRepository.Get(chatId);
        var stage = user is null || user.StageName is null
            ? GetStartSage(chatId, message)
            : BaseStage.GetCurrentStage(user);

        await stage.HandleMessage(message);
        await stage.NextStage.BeforeStage();
        await stage.NextStage.SetButtons();
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        Logger.Log(e);
    }
}

IStage GetStartSage(long chatId, string userName)
{
    var user = new UserDto
    {
        Id = chatId,
        Name = userName,
    };

    UserRepository.Save(user);

    return ServicesProvider.Get<ChooseLanguage>().SetCurrentUser(user);
}

static bool TryReadFirstFile(out int chatId, out string message, out string file)
{
    chatId = default;
    message = default;
    file = default;

    try
    {
        file = Directory.GetFiles(AppContext.BaseDirectory, "*.cmd").OrderBy(x => x).FirstOrDefault()!;
        using var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.None);
        using var sr = new StreamReader(fs);
        chatId = file.SubString("_", ".").ToInt();
        message = sr.ReadToEnd().Trim();

        return true;
    }
    catch (Exception)
    {
        return false;
    }
}
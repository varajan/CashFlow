using CashFlow;
using CashFlow.Data.DTOs;
using CashFlow.Data.Repositories;
using CashFlow.Extensions;
using CashFlow.Interfaces;
using CashFlow.Stages;
using CashFlowBotEmulator;

ServicesProvider.AddApplicationServices();
ServicesProvider.Add<INotifyService>(new EmulationNotifyService());

var Logger = ServicesProvider.Get<ILogger>();
var DataBase = ServicesProvider.Get<IDataBase>();
var UserService = ServicesProvider.Get<IUserService>();
var PersonRepository = ServicesProvider.Get<IPersonRepository>();
var PersonManager = ServicesProvider.Get<IPersonService>();
var TermsService = ServicesProvider.Get<ITranslationService>();
var UserRepository = new UserRepository(DataBase);

Console.InputEncoding = System.Text.Encoding.UTF8;
Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("Bot emulator is running. Enter messages in format: <userId> <message>");
Console.WriteLine("Commands: EXIT, CHECK, RESET");

while (true)
{
    var command = Console.ReadLine().Trim().TrimStart('\uFEFF', '\u200B');
    Console.WriteLine($"-> {command}");

    if (string.IsNullOrWhiteSpace(command)) continue;

    if (command == "EXIT") break;

    if (command == "CHECK")
    {
        UserDto test = new() { Id = 0 };
        await UserService.Notify(test, "OK");
        continue;
    }

    if (command == "RESET")
    {
        var lastActive = DateTime.Now.AddHours(-1);
        PersonRepository.GetAll().ForEach(x => PersonRepository.Save(x, lastActive));
        Thread.Sleep(300);
        continue;
    }

    var chatId = command.SubStringTo(" ")?.ToLong();
    if (chatId is null) continue;

    var message = command.SubString($"{chatId} ");

    HandleUpdateAsync(chatId.Value, message!).GetAwaiter().GetResult();
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

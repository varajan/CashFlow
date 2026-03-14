using CashFlow;
using CashFlow.Data.Users;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Extensions;
using CashFlow.Interfaces;
using CashFlow.Stages;
using CashFlowBotEmulator;

ServicesProvider.Init();

var Logger = ServicesProvider.Get<ILogger>();
var DataBase = ServicesProvider.Get<IDataBase>();
var PersonRepository = ServicesProvider.Get<IPersonRepository>();
var PersonManager = ServicesProvider.Get<IPersonManager>();
var TermsService = ServicesProvider.Get<ITermsService>();

while (true)
{
    if (!TryReadFirstFile(out int chatId, out string message, out string file))
    {
        Thread.Sleep(100);
        continue;
    }

    File.Delete(file);

    if (message == "EXIT") break;

    if (message == "RESET")
    {
        var lastActive = DateTime.Now.AddHours(-1);
        PersonRepository.GetAll().ForEach(x => PersonRepository.Save(x, lastActive));
        Thread.Sleep(300);
        continue;
    }

    HandleUpdateAsync(chatId, message).GetAwaiter().GetResult();
}

async Task HandleUpdateAsync(int chatId, string message)
{
    try
    {
        var notifyService = new EmulationNotifyService(chatId);
        var user = new CashFlowUsersUser(DataBase, PersonManager, notifyService, chatId);
        var users = GetOtherUsers(user);
        var stage = user.Exists
            ? BaseStage.GetCurrentStage(users, user)
            : GetStartSage(message, user, users);

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

static IStage GetStartSage(string userName, IUser user, List<IUser> users)
{
    user.Create();
    user.Name = userName;

    var start = ServicesProvider.Get<ChooseLanguage>()
        .SetCurrentUser(user)
        .SetAllUsers(users);

    return start;
}

List<IUser> GetOtherUsers(IUser currentUser) =>
    DataBase
        .GetColumn("SELECT ID FROM Users")
        .ToLong()
        .Where(x => x != currentUser.Id)
        .Select(x => (IUser)new CashFlowUsersUser(DataBase, PersonManager, new EmulationNotifyService(x), x))
        .ToList();

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
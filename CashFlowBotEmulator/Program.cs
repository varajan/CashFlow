using CashFlow;
using CashFlow.Data.Users;
using CashFlow.Extensions;
using CashFlow.Interfaces;
using CashFlow.Stages;
using CashFlowBotEmulator;

ServicesProvider.Init();

ILogger Logger = ServicesProvider.Get<ILogger>();
IDataBase DataBase = ServicesProvider.Get<IDataBase>();

while (true)
{
    var file = "message.txt";
    if (!TryReadFile(file, out string message))
    {
        Thread.Sleep(1_000);
        continue;
    }

    //var message = File.ReadAllText(file);
    File.Delete(file);
    if (message == "exit") break;

    HandleUpdateAsync(message!).GetAwaiter().GetResult();
}

async Task HandleUpdateAsync(string update)
{
    try
    {
        var chatId = update.SubStringTo(":").ToLong();
        var message = update.SubString(":").Trim();
        var notifyService = new EmulationNotifyService(chatId);
        var user = new CashFlowUsersUser(DataBase, notifyService, chatId);
        var users = GetOtherUsers(user);
        var stage = user.Exists
            ? BaseStage.GetCurrentStage(users, user)
            : GetStartSage(message, user, users);

        await stage.HandleMessage(message);
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
        .Select(x => (IUser)new CashFlowUsersUser(DataBase, new EmulationNotifyService(x), x))
        .ToList();

static bool TryReadFile(string path, out string content)
{
    content = null!;
    try
    {
        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);
        using var sr = new StreamReader(fs);
        content = sr.ReadToEnd().Trim();
        return true;
    }
    catch (IOException)
    {
        return false;
    }
}
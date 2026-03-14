using CashFlow.Data.Consts;
using CashFlow.Stages;
using CashFlow.Interfaces;
using CashFlow.Data.Users.UserData.PersonData;

namespace CashFlow.Data.Users;

public class CashFlowUsersUser(IDataBase dataBase, IPersonManager personManager, INotifyService notifyService, long id)
    : BaseDataModel(dataBase, id, "Users"), IUser
{
    public bool Exists => DataBase.GetColumn($"SELECT ID FROM {Table} WHERE ID = {Id}").Any();
    public Stage Stage { get => throw new Exception(); set => throw new Exception(); }
    public string StageName { get => Get("Stage"); set => Set("Stage", value); }
    public string Name { get => Get("Name"); set => Set("Name", value); }
    public bool IsActive => personManager.Read(this)?.LastActive > DateTime.Now.AddMinutes(-15);

    public Language Language { get => (Language)GetInt("Language"); set => Set("Language", (int)value); }

    public string Description => string.Empty;

    public void Create() =>
        DataBase.Execute($"INSERT INTO {Table} " +
                   "(ID, Stage, Admin, Name, Language, LastActive, FirstLogin) " +
                   $"VALUES ({Id}, '', '', '', '', '', '')");

    public async Task SetButtons(IStage stage) => await notifyService.SetButtons(stage);
    public async Task Notify(string message) => await notifyService.Notify(message);
}

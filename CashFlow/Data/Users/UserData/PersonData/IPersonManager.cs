using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Data.Users.UserData.PersonData;

public interface IPersonManager
{
    bool Exists(long id);
    void Create(string profession, long userId);
    void Update(PersonDto person);
    PersonDto Read(long id);
    string GetDescription(long id);
    void Delete(long id);

    void Update(long id, LiabilityDto liability);
    void Update(AssetDto asset);

    void AddHistory(ActionType type, long value, IUser user);

    List<AssetDto> ReadAllAssets(AssetType type, long userId);
    void CreateAsset(AssetDto asset);
    void DeleteAsset(AssetDto asset);
    void DeleteAllAssets(long userId);
    void UpdateAsset(AssetDto asset);
    void SellAsset(AssetDto asset, ActionType action, int price, IUser user);
}

public class PersonManager(IDataBase dataBase, ITermsService terms) : IPersonManager
{
    public void Create(string profession, long userId)
    {
        throw new NotImplementedException();

        var defaultProfessionData = Persons.Get(profession);

        //Clear();
        dataBase.Execute($"INSERT INTO Persons " +
                   "(ID, Profession, Salary, Cash, SmallRealEstate, ReadyForBigCircle, BigCircle, InitialCashFlow, Bankruptcy, CreditsReduced) " +
                   $"VALUES ({userId}, '', '', '', '', '', '', '', 0, 0)");

        //Assets.Clear();
        //Profession = defaultProfessionData.Profession[User.Language];
        //Cash = defaultProfessionData.Cash;
        //Salary = defaultProfessionData.Salary;

        //Expenses.Clear();
        //Expenses.Create(defaultProfessionData.Expenses);

        //Liabilities.Clear();
        //Liabilities.Create(defaultProfessionData.Liabilities);
    }

    public void Update(PersonDto person)
    {
        var sql = $"" +
            $"UPDATE Persons SET " +
            $"Profession = '{person.Profession}'," +
            $"Salary = {person.Salary}," +
            $"Cash = {person.Cash}," +
            $"ReadyForBigCircle = {(person.ReadyForBigCircle ? 1 : 0)}," +
            $"BigCircle = {(person.BigCircle ? 1 : 0)}," +
            $"InitialCashFlow = {person.InitialCashFlow}," +
            $"Bankruptcy = {(person.Bankruptcy ? 1 : 0)}," +
            $"CreditsReduced = {(person.CreditsReduced ? 1 : 0)}," +
            $"WHERE ID = {person.Id}";
        dataBase.Execute(sql);
    }

    public bool Exists(long id)
    {
        var sql = $"SELECT * FROM Persons WHERE ID = {id}";
        var data = dataBase.GetRows(sql);

        return data.Any();
    }

    public PersonDto Read(long id)
    {
        var sql = $"SELECT * FROM Persons WHERE ID = {id}";
        var data = dataBase.GetRow(sql);

        return new PersonDto
        {
            Id = id,
            Profession = data["Profession"],
            Salary = data["Salary"].ToInt(),
            Cash = data["Cash"].ToInt(),
            ReadyForBigCircle = data["ReadyForBigCircle"].ToInt() == 1,
            BigCircle = data["BigCircle"].ToInt() == 1,
            InitialCashFlow = data["InitialCashFlow"].ToInt(),
            Bankruptcy = data["Bankruptcy"].ToInt() == 1,
            CreditsReduced = data["CreditsReduced"].ToInt() == 1,
        };
    }

    public string GetDescription(long id) => throw new NotImplementedException();

    public void Delete(long id) => throw new NotImplementedException();

    public void Update(long id, LiabilityDto liability) => throw new NotImplementedException();
    public void Update(AssetDto asset) => throw new NotImplementedException();

    public void AddHistory(ActionType type, long value, IUser user)
    {
        //var record = new HistoryDto { UserId = user.Id, Action = type, Value = value };
        //long newId = DataBase.GetValue("SELECT MAX(ID) FROM History").ToLong() + 1;
        //var text = GetDescription(record, user);
        //DataBase.Execute($@"INSERT INTO History VALUES ({newId}, {user.Id}, {(int)type}, {value}, '• {text}')");
    }

    public List<AssetDto> ReadAllAssets(AssetType type, long userId)
    {
        var ids = dataBase.GetColumn($"SELECT ID FROM Assets WHERE Type = {type} AND UserID = {userId}");
        return ids.Select(id => ReadAsset(id.ToLong(), userId)).ToList();
    }

    public AssetDto ReadAsset(long id, long userId)
    {
        var sql = $"SELECT * FROM Assets WHERE AssetID = {id} AND UserID = {userId}";
        var data = dataBase.GetRow(sql);

        return new AssetDto
        {
            Type = data["Type"].ParseEnum<AssetType>(),
            Title = data["Title"],
            Id = data["Id"].ToInt(),
            UserId = data["UserId"].ToInt(),
            Price = data["Price"].ToInt(),
            SellPrice = data["SellPrice"].ToInt(),
            Qtty = data["Qtty"].ToInt(),
            Mortgage = data["Mortgage"].ToInt(),
            TotalCashFlow = data["TotalCashFlow"].ToInt(),
            CashFlow = data["CashFlow"].ToInt(),
            BigCircle = data["BigCircle"].ToInt() == 1,
            IsDraft = data["IsDraft"].ToInt() == 1,
            IsDeleted = data["IsDeleted"].ToInt() == 1,
        };
    }

    public void CreateAsset(AssetDto asset)
    {
        int newId = dataBase.GetValue("SELECT MAX(AssetID) FROM Assets").ToInt() + 1;
        var sql = $@"
            INSERT INTO Assets (Id, UserId, Type, Title, Price, SellPrice, Qtty, Mortgage, CashFlow, BigCircle, IsDraft, IsDeleted)
            VALUES
            (
                {newId},
                {asset.UserId},
                {(int)asset.Type},
                '{asset.Title.Replace("'", "''")}',
                {asset.Price},
                {asset.SellPrice},
                {asset.Qtty},
                {asset.Mortgage},
                {asset.CashFlow},
                {asset.BigCircle},
                {(asset.IsDraft ? 1 : 0)},
                {(asset.IsDeleted ? 1 : 0)}
            );";
        dataBase.Execute(sql);

        //return Read(newId, asset.UserId);
    }

    public void DeleteAsset(AssetDto asset)
    {
        asset.IsDeleted = true;
        asset.Title = asset.Title.SubStringTo("*");
        UpdateAsset(asset);
    }

    public void DeleteAllAssets(long userId) => dataBase.Execute($"DELETE FROM Assets WHERE UserID = {userId}");

    public void UpdateAsset(AssetDto asset)
    {
        var sql = $"" +
            $"UPDATE Assets SET " +
            $"Type = {(int)asset.Type}," +
            $"Title = '{asset.Title}'," +
            $"Price = {asset.Price}," +
            $"SellPrice = {asset.SellPrice}," +
            $"Qtty = {asset.Qtty}," +
            $"Mortgage = {asset.Mortgage}," +
            $"CashFlow = {asset.CashFlow}," +
            $"BigCircle = {asset.BigCircle}," +
            $"CashFlow = {asset.CashFlow}," +
            $"IsDraft = {(asset.IsDraft ? 1 : 0)}," +
            $"IsDeleted = {(asset.IsDeleted ? 1 : 0)}," +
            $"WHERE AssetID = {asset.Id} AND UserID = {asset.UserId}";
        dataBase.Execute(sql);
    }

    public void SellAsset(AssetDto asset, ActionType action, int price, IUser user)
    {
        asset.SellPrice = price;
        DeleteAsset(asset);
        AddHistory(action, asset.Id, user);
    }
}

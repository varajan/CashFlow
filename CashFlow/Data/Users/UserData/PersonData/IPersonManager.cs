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

    void AddHistory(ActionType type, long value, IUser user);

    List<AssetDto> ReadAllAssets(AssetType type, long userId);
    void CreateAsset(AssetDto asset);
    void DeleteAsset(AssetDto asset);
    void DeleteAllAssets(long userId);
    void UpdateAsset(AssetDto asset);
    void SellAsset(AssetDto asset, ActionType action, int price, IUser user);
}

//public class PersonManager(IDataBase dataBase, ITermsService terms, IUser user) : IPersonManager
public class PersonManager(IDataBase dataBase, ITermsService terms) : IPersonManager
{
    //private ITermsService Terms { get; } = terms;
    //private IUser User { get; } = user;

    public void Create(string profession, long userId)
    {
        var defaults = Persons.Get(profession);

        //Clear();
        //dataBase.Execute($"INSERT INTO Persons " +
        //           "(ID, Profession, Salary, Cash, SmallRealEstate, ReadyForBigCircle, BigCircle, InitialCashFlow, Bankruptcy, CreditsReduced) " +
        //           $"VALUES ({userId}, '', '', '', '', '', '', '', 0, 0)");

        Delete(userId);
        DeleteAllAssets(userId);

        var person = new PersonDto
        {
            Id = userId,
            Profession = defaults.Profession[Language.EN],
            Cash = defaults.Cash,
            Salary = defaults.Salary,
        };

    person.Liabilities =
        [
            new() { Name = "Taxes", FullAmount = 0 /*defaults.Liabilities.Taxes*/, Cashflow = -defaults.Expenses.Taxes, },
            new() { Name = "Mortgage", FullAmount = defaults.Liabilities.Mortgage, Cashflow = -defaults.Expenses.Mortgage, },
            new() { Name = "School Loan", FullAmount = defaults.Liabilities.SchoolLoan, Cashflow = -defaults.Expenses.SchoolLoan, },
            new() { Name = "Car Loan", FullAmount = defaults.Liabilities.CarLoan, Cashflow = -defaults.Expenses.CarLoan, },
            new() { Name = "Credit Card", FullAmount = defaults.Liabilities.CreditCard, Cashflow = -defaults.Expenses.CreditCard, },
            new() { Name = "Bank Loan", FullAmount = defaults.Liabilities.BankLoan, Cashflow = -defaults.Expenses.BankLoan, },
            new() { Name = "Others", FullAmount = 0 /*defaults.Liabilities.Others*/, Cashflow = -defaults.Expenses.Others, },
            new() { Name = "Small Credits", FullAmount = defaults.Liabilities.SmallCredits, Cashflow = -defaults.Expenses.SmallCredits, },
        ];

        dataBase.Execute($"INSERT INTO Persons (ID, PersonData) VALUES ({userId}, '{person.Serialize()}')");

        //Assets.Clear();
        //Profession = defaultProfessionData.Profession[User.Language];
        //Cash = defaultProfessionData.Cash;
        //Salary = defaultProfessionData.Salary;

        //Expenses.Clear();
        //Expenses.Create(defaultProfessionData.Expenses);

        //Liabilities.Clear();
        //Liabilities.Create(defaultProfessionData.Liabilities);
    }

    public void Update(PersonDto person) => dataBase.Execute($"UPDATE Persons SET PersonData = '{person.Serialize()}' WHERE ID = {person.Id}");
    //{
    //    var sql = $"" +
    //        $"UPDATE Persons SET " +
    //        $"Profession = '{person.Profession}'," +
    //        $"Salary = {person.Salary}," +
    //        $"Cash = {person.Cash}," +
    //        $"ReadyForBigCircle = {(person.ReadyForBigCircle ? 1 : 0)}," +
    //        $"BigCircle = {(person.BigCircle ? 1 : 0)}," +
    //        $"InitialCashFlow = {person.InitialCashFlow}," +
    //        $"Bankruptcy = {(person.Bankruptcy ? 1 : 0)}," +
    //        $"CreditsReduced = {(person.CreditsReduced ? 1 : 0)}," +
    //        $"WHERE ID = {person.Id}";
    //    dataBase.Execute(sql);
    //}

    public bool Exists(long id)
    {
        var sql = $"SELECT * FROM Persons WHERE ID = {id}";
        var data = dataBase.GetRows(sql);

        return data.Any();
    }

    public PersonDto Read(long id) => dataBase
        .GetValue($"SELECT PersonData FROM Persons WHERE ID = {id}")
        .Deserialize<PersonDto>();
    //{
        //var sql = $"SELECT * FROM Persons WHERE ID = {id}";
        //var data = dataBase.GetRow(sql);

        //return new PersonDto
        //{
        //    Id = id,
        //    Profession = data["Profession"],
        //    Salary = data["Salary"].ToInt(),
        //    Cash = data["Cash"].ToInt(),
        //    ReadyForBigCircle = data["ReadyForBigCircle"].ToInt() == 1,
        //    BigCircle = data["BigCircle"].ToInt() == 1,
        //    InitialCashFlow = data["InitialCashFlow"].ToInt(),
        //    Bankruptcy = data["Bankruptcy"].ToInt() == 1,
        //    CreditsReduced = data["CreditsReduced"].ToInt() == 1,
        //};
    //}

    public string GetDescription(long id)
    {
        var person = Read(id);
        //User.LastActive = DateTime.Now;
        return person.BigCircle ? BigCircleDescription(person) : SmallCircleDescription(person);
    }

    private string ProfessionTerm => "Profession";
    private string CashTerm => "Cash";
    private string SalaryTerm => "Salary";
    private string IncomeTerm => "Income";
    private string ExpensesTerm => "Expenses";
    private string CashFlowTerm => "Cash Flow";
    private string InitialTerm => "Initial";
    private string CurrentTerm => "Current";
    private string TargetTerm => "Target";

    private string SmallCircleDescription(PersonDto person) =>
    $"*{ProfessionTerm}:* {person.Profession}{Environment.NewLine}" +
    $"*{CashTerm}:* {person.Cash.AsCurrency()}{Environment.NewLine}" +
    $"*{SalaryTerm}:* {person.Salary.AsCurrency()}{Environment.NewLine}" +
    $"*{IncomeTerm}:* {person.Assets.Sum(a => a.CashFlow).AsCurrency()}{Environment.NewLine}" +
    //$"*{ExpensesTerm}:* {Expenses.Total.AsCurrency()}{Environment.NewLine}" +
    $"*{ExpensesTerm}:* {person.Liabilities.Sum(l => l.Cashflow).AsCurrency()}{Environment.NewLine}" +
    $"*{CashFlowTerm}*: {person.CashFlow.AsCurrency()}";

    private string BigCircleDescription(PersonDto person) =>
        $"*{ProfessionTerm}:* {person.Profession}{Environment.NewLine}" +
        $"*{CashTerm}:* {person.Cash.AsCurrency()}{Environment.NewLine}" +
        $"{InitialTerm} {CashFlowTerm}: {person.InitialCashFlow.AsCurrency()}{Environment.NewLine}" +
        $"{CurrentTerm} {CashFlowTerm}: {person.CurrentCashFlow.AsCurrency()}{Environment.NewLine}" +
        $"{TargetTerm} {CashFlowTerm}: {person.TargetCashFlow.AsCurrency()}{Environment.NewLine}" +
        //$"{person.Assets.BigCircleDescription}";
        $"";

    public void Delete(long id)
    {
        dataBase.Execute($"DELETE FROM Persons WHERE ID = {id}");
        DeleteAllAssets(id);
        // assets?
        // liabilities?
        // history?
    }

    public void Update(long id, LiabilityDto liability) => throw new NotImplementedException();

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

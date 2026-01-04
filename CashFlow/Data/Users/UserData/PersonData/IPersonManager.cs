using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Data.Users.UserData.PersonData;

public interface IPersonManager
{
    bool Exists(IUser user);
    void Create(string profession, IUser user);
    void Update(PersonDto person);
    PersonDto Read(IUser user);
    string GetDescription(IUser user);
    void Delete(IUser user);
    void Update(IUser user, LiabilityDto liability);

    void AddHistory(ActionType type, long value, IUser user);

    List<AssetDto> ReadAllAssets(AssetType type, IUser user);
    void CreateAsset(AssetDto asset);
    void DeleteAsset(AssetDto asset);
    void DeleteAllAssets(IUser user);
    void UpdateAsset(AssetDto asset);
    void SellAsset(AssetDto asset, ActionType action, int price, IUser user);
}

public class PersonManager(IDataBase dataBase, ITermsService terms) : IPersonManager
{
    private ITermsService Terms { get; } = terms;
    //private IUser User { get; } = user;

    public void Create(string profession, IUser user)
    {
        var defaults = Persons.Get(profession);

        //Clear();
        //dataBase.Execute($"INSERT INTO Persons " +
        //           "(ID, Profession, Salary, Cash, SmallRealEstate, ReadyForBigCircle, BigCircle, InitialCashFlow, Bankruptcy, CreditsReduced) " +
        //           $"VALUES ({userId}, '', '', '', '', '', '', '', 0, 0)");

        Delete(user);
        DeleteAllAssets(user);

        var person = new PersonDto
        {
            Id = user.Id,
            Profession = defaults.Profession[user.Language],
            Cash = defaults.Cash,
            Salary = defaults.Salary,
            PerChild = defaults.Expenses.PerChild,
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

        person.Cash += person.CashFlow;
        dataBase.Execute($"INSERT INTO Persons (ID, PersonData) VALUES ({user.Id}, '{person.Serialize()}')");
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

    public bool Exists(IUser user)
    {
        var sql = $"SELECT * FROM Persons WHERE ID = {user.Id}";
        var data = dataBase.GetRows(sql);

        return data.Any();
    }

    public PersonDto Read(IUser user) => dataBase.GetValue($"SELECT PersonData FROM Persons WHERE ID = {user.Id}").Deserialize<PersonDto>();
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

    public string GetDescription(IUser user)
    {
        var person = Read(user);
        user.LastActive = DateTime.Now;
        return person.BigCircle ? BigCircleDescription(person, user) : SmallCircleDescription(person, user);
    }

    private string SmallCircleDescription(PersonDto person, IUser user)
    {
        var professionTerm = Terms.Get(50, user, "Profession");
        var cashTerm = Terms.Get(51, user, "Cash");
        var salaryTerm = Terms.Get(52, user, "Salary");
        var incomeTerm = Terms.Get(53, user, "Income");
        var expensesTerm = Terms.Get(54, user, "Expenses");
        var cashFlowTerm = Terms.Get(55, user, "Cash Flow");

        return
            $"*{professionTerm}:* {person.Profession}{Environment.NewLine}" +
            $"*{cashTerm}:* {person.Cash.AsCurrency()}{Environment.NewLine}" +
            $"*{salaryTerm}:* {person.Salary.AsCurrency()}{Environment.NewLine}" +
            $"*{incomeTerm}:* {person.Assets.Sum(a => a.CashFlow).AsCurrency()}{Environment.NewLine}" +
            //$"*{ExpensesTerm}:* {Expenses.Total.AsCurrency()}{Environment.NewLine}" +
            $"*{expensesTerm}:* {person.TotalExpenses.AsCurrency()}{Environment.NewLine}" +
            $"*{cashFlowTerm}*: {person.CashFlow.AsCurrency()}";
    }

    private string BigCircleDescription(PersonDto person, IUser user)
    {
        var professionTerm = Terms.Get(50, user, "Profession");
        var cashTerm = Terms.Get(51, user, "Cash");
        var cashFlowTerm = Terms.Get(55, user, "Cash Flow");
        var initialTerm = Terms.Get(65, user, "Initial");
        var currentTerm = Terms.Get(66, user, "Current");
        var targetTerm = Terms.Get(67, user, "Target");

        return
            $"*{professionTerm}:* {person.Profession}{Environment.NewLine}" +
            $"*{cashTerm}:* {person.Cash.AsCurrency()}{Environment.NewLine}" +
            $"{initialTerm} {cashFlowTerm}: {person.InitialCashFlow.AsCurrency()}{Environment.NewLine}" +
            $"{currentTerm} {cashFlowTerm}: {person.CurrentCashFlow.AsCurrency()}{Environment.NewLine}" +
            $"{targetTerm} {cashFlowTerm}: {person.TargetCashFlow.AsCurrency()}{Environment.NewLine}" +
            //$"{person.Assets.BigCircleDescription}";
            $"";
    }

    public void Delete(IUser user)
    {
        dataBase.Execute($"DELETE FROM Persons WHERE ID = {user.Id}");
        DeleteAllAssets(user);
        // assets?
        // liabilities?
        // history?
    }

    public void Update(IUser user, LiabilityDto liability) => throw new NotImplementedException();

    public void AddHistory(ActionType type, long value, IUser user)
    {
        //var record = new HistoryDto { UserId = user.Id, Action = type, Value = value };
        //long newId = DataBase.GetValue("SELECT MAX(ID) FROM History").ToLong() + 1;
        //var text = GetDescription(record, user);
        //DataBase.Execute($@"INSERT INTO History VALUES ({newId}, {user.Id}, {(int)type}, {value}, '• {text}')");
    }

    public List<AssetDto> ReadAllAssets(AssetType type, IUser user)
    {
        var ids = dataBase.GetColumn($"SELECT ID FROM Assets WHERE Type = {type} AND UserID = {user.Id}");
        return ids.Select(id => ReadAsset(id.ToLong(), user)).ToList();
    }

    public AssetDto ReadAsset(long id, IUser user)
    {
        var sql = $"SELECT * FROM Assets WHERE AssetID = {id} AND UserID = {user.Id}";
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

    public void DeleteAllAssets(IUser user) => dataBase.Execute($"DELETE FROM Assets WHERE UserID = {user.Id}");

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

using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Interfaces;
using MoreLinq;

namespace CashFlow.Data.Users.UserData.PersonData;

public interface IPersonManager
{
    bool Exists(IUser user);
    void Create(string profession, IUser user);
    void Update(PersonDto person);
    PersonDto Read(IUser user);
    string GetDescription(IUser user, bool compact = true);
    void Delete(IUser user);
    void Update(IUser user, LiabilityDto liability);

    void AddHistory(ActionType type, long value, IUser user);
    List<HistoryDto> ReadHistory(IUser user);
    bool IsHistoryEmpty(IUser user);
    string HistoryTopFive(IUser user, IUser currentUser);
    void RollbackHistory(PersonDto person, HistoryDto record);
    void ClearHistory(IUser user);

    List<AssetDto> ReadAllAssets(AssetType type, IUser user);
    void CreateAsset(IUser user, AssetDto asset);
    void DeleteAsset(IUser user, AssetDto asset);
    void UpdateAsset(IUser user, AssetDto asset);
    void SellAsset(AssetDto asset, ActionType action, int price, IUser user);
    string GetAssetDescription(AssetDto asset, IUser user);
}

public class PersonManager(IDataBase dataBase, ITermsService terms) : IPersonManager
{
    private ITermsService Terms { get; } = terms;
    private IDataBase DataBase { get; }  = dataBase;

    public void Create(string profession, IUser user)
    {
        var defaults = Persons.Get(profession);

        //Clear();
        //DataBase.Execute($"INSERT INTO Persons " +
        //           "(ID, Profession, Salary, Cash, SmallRealEstate, ReadyForBigCircle, BigCircle, InitialCashFlow, Bankruptcy, CreditsReduced) " +
        //           $"VALUES ({userId}, '', '', '', '', '', '', '', 0, 0)");

        Delete(user);

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
            new() { Type = Liability.Taxes, Name = Liability.Taxes.AsString(), FullAmount = 0 /*defaults.Liabilities.Taxes*/, Cashflow = -defaults.Expenses.Taxes, },
            new() { Type = Liability.Mortgage, Name = Liability.Mortgage.AsString(), FullAmount = defaults.Liabilities.Mortgage, Cashflow = -defaults.Expenses.Mortgage, },
            new() { Type = Liability.School_Loan, Name = Liability.School_Loan.AsString(), FullAmount = defaults.Liabilities.SchoolLoan, Cashflow = -defaults.Expenses.SchoolLoan, },
            new() { Type = Liability.Car_Loan, Name = Liability.Car_Loan.AsString(), FullAmount = defaults.Liabilities.CarLoan, Cashflow = -defaults.Expenses.CarLoan, },
            new() { Type = Liability.Credit_Card, Name = Liability.Credit_Card.AsString(), FullAmount = defaults.Liabilities.CreditCard, Cashflow = -defaults.Expenses.CreditCard, },
            new() { Type = Liability.Bank_Loan, Name = Liability.Bank_Loan.AsString(), FullAmount = defaults.Liabilities.BankLoan, Cashflow = -defaults.Expenses.BankLoan, },
            new() { Type = Liability.Others, Name = Liability.Others.AsString(), FullAmount = 0 /*defaults.Liabilities.Others*/, Cashflow = -defaults.Expenses.Others, },
            new() { Type = Liability.Small_Credit, Name = Liability.Small_Credit.AsString(), FullAmount = defaults.Liabilities.SmallCredits, Cashflow = -defaults.Expenses.SmallCredits, },
        ];

        person.Cash += person.CashFlow;
        DataBase.Execute($"INSERT INTO Persons (ID, PersonData) VALUES ({user.Id}, '{person.Serialize()}')");
    }

    public void Update(PersonDto person) => DataBase.Execute($"UPDATE Persons SET PersonData = '{person.Serialize()}' WHERE ID = {person.Id}");

    public bool Exists(IUser user)
    {
        var sql = $"SELECT * FROM Persons WHERE ID = {user.Id}";
        var data = DataBase.GetRows(sql);

        return data.Any();
    }

    public PersonDto Read(IUser user) => DataBase.GetValue($"SELECT PersonData FROM Persons WHERE ID = {user.Id}").Deserialize<PersonDto>();

    public string GetDescription(IUser user, bool compact = true)
    {
        var person = Read(user);
        user.LastActive = DateTime.Now;
        var description = person.BigCircle
            ? BigCircleDescription(person, user)
            : SmallCircleDescription(person, user);

        if (!compact)
        {
            description += AssetsDescription(person, user);
            description += ExpensesDescription(person, user);
        }

        return description;
    }

    private string AssetsDescription(PersonDto person, IUser user)
    {
        if (!person.Assets.Any(a => !a.IsDeleted))
            return string.Empty;

        var assetsTerm = Terms.Get(56, user, "Assets");
        var assets = $"{Environment.NewLine}{Environment.NewLine}*{assetsTerm}:*{Environment.NewLine}";
        assets += string.Join(Environment.NewLine, person.Assets
            .Where(a => !a.IsDeleted)
            .OrderBy(a => a.Type)
            .Select(a => $"• {GetAssetDescription(a, user)}"));

        return assets;
    }

    private string ExpensesDescription(PersonDto person, IUser user)
    {
        var expensesTerm = Terms.Get(54, user, "Expenses");
        var taxesTerm = Terms.Get(58, user, "Taxes");
        var mortgageTerm = Terms.Get(59, user, "Mortgage/Rent Pay");
        var schoolLoanTerm = Terms.Get(44, user, "School Loan");
        var carLoanTerm = Terms.Get(45, user, "Car Loan");
        var creditCardTerm = Terms.Get(46, user, "Credit Card");
        var smallCreditsTerm = Terms.Get(92, user, "Small Credit");
        var bankLoanTerm = Terms.Get(47, user, "Bank Loan");
        var boatLoanTerm = Terms.Get(114, user, "Boat Loan");
        var otherPaymentTerm = Terms.Get(60, user, "Other Payments");
        var childrenTerm = Terms.Get(61, user, "Children");
        var childrenExpensesTerm = Terms.Get(62, user, "Children Expenses");
        var perChildTerm = Terms.Get(63, user, "per child");

        var mortgage = GetLiability(person, Liability.Mortgage);
        var schoolLoan = GetLiability(person, Liability.School_Loan);
        var carLoan = GetLiability(person, Liability.Car_Loan);
        var creditCard = GetLiability(person, Liability.Credit_Card);
        var smallCredits = GetLiability(person, Liability.Small_Credit);
        var bankLoan = GetLiability(person, Liability.Bank_Loan);
        var boatLoan = GetLiability(person, Liability.Boat_Loan);
        var taxes = GetLiability(person, Liability.Taxes);
        var others = GetLiability(person, Liability.Others);
        var childrenExpenses = person.Children * person.PerChild;
        var children = person.Children;

        var expenses = $"{Environment.NewLine}{Environment.NewLine}*{expensesTerm}:*{Environment.NewLine}";
        expenses += $"*{taxesTerm}:* {taxes.AsCurrency()}{Environment.NewLine}";

        if (mortgage > 0) expenses += $"*{mortgageTerm}:* {mortgage.AsCurrency()}{Environment.NewLine}";
        if (schoolLoan > 0) expenses += $"*{schoolLoanTerm}:* {schoolLoan.AsCurrency()}{Environment.NewLine}";
        if (carLoan > 0) expenses += $"*{carLoanTerm}:* {carLoan.AsCurrency()}{Environment.NewLine}";
        if (creditCard > 0) expenses += $"*{creditCardTerm}:* {creditCard.AsCurrency()}{Environment.NewLine}";
        if (smallCredits > 0) expenses += $"*{smallCreditsTerm}:* {smallCredits.AsCurrency()}{Environment.NewLine}";
        if (bankLoan > 0) expenses += $"*{bankLoanTerm}:* {bankLoan.AsCurrency()}{Environment.NewLine}";
        if (boatLoan > 0) expenses += $"*{boatLoanTerm}:* {boatLoan.AsCurrency()}{Environment.NewLine}";
        expenses += $"*{otherPaymentTerm}:* {others.AsCurrency()}{Environment.NewLine}";
        if (childrenExpenses > 0) expenses += $"*{childrenTerm}:* {children} ({person.PerChild.AsCurrency()} {perChildTerm}){Environment.NewLine}";
        if (childrenExpenses > 0) expenses += $"*{childrenExpensesTerm}:* {childrenExpenses.AsCurrency()}{Environment.NewLine}";

        return expenses;
    }

    private static int GetLiability(PersonDto person, Liability name)
    {
        var liability = person.Liabilities.FirstOrDefault(l => l.Type == name)?.Cashflow ?? 0;
        return Math.Abs(liability);
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
            $"*{incomeTerm}:* {(person.Assets.Sum(a => a.CashFlow - person.BoatPayment)).AsCurrency()}{Environment.NewLine}" +
            $"*{expensesTerm}:* {(-person.TotalExpenses).AsCurrency()}{Environment.NewLine}" +
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
        DataBase.Execute($"DELETE FROM Persons WHERE ID = {user.Id}");
        // assets?
        // liabilities?
        // history?
    }

    public void Update(IUser user, LiabilityDto liability) => throw new NotImplementedException();

    #region History

    public void AddHistory(ActionType type, long value, IUser user)
    {
        var record = new HistoryDto
        {
            UserId = user.Id,
            Date = DateTime.UtcNow,
            Action = type,
            Value = value,
            Description = $"• {GetDescription(type, value, user)}"
        };

        DataBase.Execute($@"INSERT INTO History (UserId, Id, HistoryRecord) VALUES ({user.Id}, {record.Date.Ticks}, '{record.Serialize()}')");
    }

    public bool IsHistoryEmpty(IUser user) => DataBase.GetValue($"SELECT COUNT(*) FROM History WHERE UserID = {user.Id}").ToInt() == 0;

    public List<HistoryDto> ReadHistory(IUser user)
    {
        var sql = $"SELECT * FROM History WHERE UserID = {user.Id}";
        var data = DataBase.GetRows(sql);
        var result = data.Select(row => row["HistoryRecord"].Deserialize<HistoryDto>()).OrderBy(r => r.Date).ToList();
        return result;
    }

    public string HistoryTopFive(IUser user, IUser currentUser)
    {
        var records = ReadHistory(user);
        records.Reverse();

        return records.Any() ?
            Terms.Get(111, currentUser, "No records found.") :
            string.Join(Environment.NewLine, records.Take(5).Select(x => x.Description));
    }

    public void RollbackHistory(PersonDto person, HistoryDto record)
    {
        var boat = person.Assets.FirstOrDefault(a => a.Type == AssetType.Boat);
        var asset = person.Assets.Find(a => a.Id == (int)record.Value);
        var amount = (int)record.Value;
        var defaults = Persons.Get(person.Profession);

        decimal percent;
        int expenses;

        switch (record.Action)
        {
            case ActionType.PayMoney:
            case ActionType.Downsize:
            case ActionType.Charity:
                person.Cash += amount;
                break;

            case ActionType.GetMoney:
                person.Cash -= amount;
                break;

            case ActionType.Child:
                person.Children--;
                break;

            case ActionType.Credit:
                person.Cash -= amount;
                person.UpdateLiability(Liability.Bank_Loan, amount / 10, -amount);
                break;

            case ActionType.Mortgage:
                person.Cash += amount;
                person.UpdateLiability(Liability.Mortgage, -defaults.Expenses.Mortgage, amount);
                break;

            case ActionType.SchoolLoan:
                person.Cash += amount;
                person.UpdateLiability(Liability.School_Loan, -defaults.Expenses.SchoolLoan, amount);
                break;

            case ActionType.CarLoan:
                person.Cash += amount;
                person.UpdateLiability(Liability.Car_Loan, -defaults.Expenses.CarLoan, amount);
                break;

            case ActionType.CreditCard:
                percent = (decimal)defaults.Expenses.CreditCard / defaults.Liabilities.CreditCard;
                expenses = (int)(amount * percent);

                person.Cash += amount;
                person.UpdateLiability(Liability.Credit_Card, expenses, amount);
                break;

            case ActionType.SmallCredit:
                percent = (decimal)defaults.Expenses.SmallCredits / defaults.Liabilities.SmallCredits;
                expenses = (int)(amount * percent);

                person.Cash += amount;
                person.UpdateLiability(Liability.Small_Credit, expenses, amount);
                break;

            case ActionType.BankLoan:
                person.Cash += amount;
                person.UpdateLiability(Liability.Bank_Loan, amount / 10, -amount);
                break;

            case ActionType.BankruptcyBankLoan:
                percent = 0.1m;
                expenses = (int)(amount * percent);

                person.Cash += amount;
                person.UpdateLiability(Liability.Bank_Loan, -amount / 10, amount);
                person.Bankruptcy = true;
                break;

            case ActionType.BuyRealEstate:
            case ActionType.BuyBusiness:
            case ActionType.BuyLand:
            case ActionType.StartCompany:
                person.Cash += asset.Price - asset.Mortgage;
                DeleteAsset(person, asset);
                break;

            case ActionType.IncreaseCashFlow:
                person.Assets.Where(a => a.Type == AssetType.SmallBusiness).ForEach(x => x.CashFlow -= (int)record.Value);
                break;

            case ActionType.SellRealEstate:
            case ActionType.SellBusiness:
            case ActionType.SellLand:
                person.Cash -= asset.SellPrice - asset.Mortgage;
                RestoreAsset(person, asset);
                break;

            case ActionType.BuyStocks:
            case ActionType.BuyCoins:
                person.Cash += asset.Price * asset.Qtty;
                DeleteAsset(person, asset);
                break;

            case ActionType.SellStocks:
            case ActionType.SellCoins:
                person.Cash -= asset.Qtty * asset.SellPrice;
                RestoreAsset(person, asset);
                break;

            case ActionType.Stocks1To2:
                asset.Qtty /= 2;
                break;

            case ActionType.Stocks2To1:
                asset.Qtty *= 2;
                break;

            case ActionType.MicroCredit:
                person.UpdateLiability(Liability.Credit_Card, (int)(amount * 0.03), -amount);
                break;

            case ActionType.BuyBoat:
                person.Cash += 1_000;
                DeleteAsset(person, boat);
                break;

            case ActionType.PayOffBoat:
                person.Cash += amount;
                RestoreAsset(person, boat);
                break;

            case ActionType.Bankruptcy:
                person.Bankruptcy = false;
                break;

            case ActionType.BankruptcySellAsset:
                person.Cash -= asset.BancrupcySellPrice;
                person.Bankruptcy = true;
                RestoreAsset(person, asset);
                break;

            case ActionType.BankruptcyDebtRestructuring:
                ReduceCreditsRollback();
                break;

            case ActionType.GoToBigCircle:
                person.Cash -= person.InitialCashFlow;
                person.InitialCashFlow = 0;
                person.BigCircle = false;
                break;

            case ActionType.Divorce:
            case ActionType.TaxAudit:
            case ActionType.Lawsuit:
                person.Cash += amount;
                break;

            default:
                throw new Exception($"<{record.Action}> ???");
        }

        Update(person);
        DataBase.Execute($"DELETE FROM History WHERE UserId = {record.UserId} AND Id = {record.Date.Ticks}");
    }

    private void ReduceCreditsRollback()
    {
        throw new Exception("Not implemented rollback for BankruptcyDebtRestructuring");

        //var person = Persons.Get(User.Person_OBSOLETE.Profession);
        //var count = User.History_OBSOLETE.Count(ActionType.BankruptcyDebtRestructuring);

        //Expenses.CarLoan = person.Expenses.CarLoan;
        //Expenses.CreditCard = person.Expenses.CreditCard;
        //Expenses.SmallCredits = person.Expenses.SmallCredits;
        //Liabilities.CarLoan = person.Liabilities.CarLoan;
        //Liabilities.CreditCard = person.Liabilities.CreditCard;
        //Liabilities.SmallCredits = person.Liabilities.SmallCredits;

        //for (var i = 0; i < count; i++)
        //{
        //    Expenses.CarLoan /= 2;
        //    Expenses.CreditCard /= 2;
        //    Expenses.SmallCredits /= 2;
        //    Liabilities.CarLoan /= 2;
        //    Liabilities.CreditCard /= 2;
        //    Liabilities.SmallCredits /= 2;
        //}

        //CreditsReduced = false;
        //Bankruptcy = CashFlow < 0;
    }

    public void ClearHistory(IUser user) => DataBase.Execute($"DELETE FROM History WHERE UserID = {user.Id}");

    private string GetDescription(ActionType Action, long Value, IUser user)
    {
        switch (Action)
        {
            case ActionType.PayMoney:
                return Terms.Get(103, user, "Pay {0}", Value.AsCurrency());

            case ActionType.GetMoney:
                return Terms.Get(104, user, "Get {0}", Value.AsCurrency());

            case ActionType.Child:
                return Terms.Get(105, user, "Get a child");

            case ActionType.Downsize:
                return Terms.Get(106, user, "Downsize and paying {0}", Value.AsCurrency());

            case ActionType.Credit:
                return Terms.Get(107, user, "Get credit: {0}", Value.AsCurrency());

            case ActionType.Charity:
                return Terms.Get(108, user, "Charity: {0}", Value.AsCurrency());

            case ActionType.Mortgage:
            case ActionType.SchoolLoan:
            case ActionType.CarLoan:
            case ActionType.CreditCard:
            case ActionType.SmallCredit:
            case ActionType.BankLoan:
            case ActionType.PayOffBoat:
            case ActionType.BankruptcyBankLoan:
                var reduceLiabilities = Terms.Get(40, user, "Reduce Liabilities");
                var type = Terms.Get((int)Action, user, "Liability");
                var amount = Value.AsCurrency();
                return $"{reduceLiabilities}. {type}: {amount}";

            case ActionType.BuyRealEstate:
            case ActionType.BuyBusiness:
            case ActionType.BuyStocks:
            case ActionType.BuyLand:
            case ActionType.StartCompany:
            case ActionType.BuyCoins:
                var buyAsset = Terms.Get((int)Action, user, "Buy Asset");
                var asset = ReadAsset(Value, user);
                var description = GetAssetDescription(asset, user);

                return $"{buyAsset}. {description}";

            case ActionType.IncreaseCashFlow:
                var increaseCashFlow = Terms.Get((int)Action, user, "Increase Cash Flow");
                return $"{increaseCashFlow}. {Value.AsCurrency()}";

            case ActionType.SellRealEstate:
            case ActionType.SellBusiness:
            case ActionType.SellStocks:
            case ActionType.SellLand:
            case ActionType.SellCoins:
            case ActionType.BankruptcySellAsset:
                var sellAsset = Terms.Get((int)Action, user, "Sell Asset");
                var assetToSell = ReadAsset(Value, user);
                var sellDescription = GetAssetDescription(assetToSell, user);

                return $"{sellAsset}. {sellDescription}";

            case ActionType.Stocks1To2:
            case ActionType.Stocks2To1:
                var multiply = Terms.Get((int)Action, user, "Multiply Stocks");
                var stock = ReadAsset(Value, user);
                var stockDescription = GetAssetDescription(stock, user);

                return $"{multiply}. {stockDescription}";

            case ActionType.MicroCredit:
                return Terms.Get(96, user, "Pay with Credit Card") + " - " + Value.AsCurrency();

            case ActionType.BuyBoat:
                var buyBoat = Terms.Get(112, user, "Buy a boat");
                return $"{buyBoat}: {Value.AsCurrency()}";

            case ActionType.BankruptcyDebtRestructuring:
            case ActionType.Bankruptcy:
                return Terms.Get((int)Action, user, "Bankruptcy");

            case ActionType.GoToBigCircle:
            case ActionType.Divorce:
            case ActionType.TaxAudit:
            case ActionType.Lawsuit:
                return Terms.Get((int)Action, user, "BigCircle");

            default:
                return $"<{Action}> - {Value}";
        }
    }

    #endregion

    #region Assets

    public List<AssetDto> ReadAllAssets(AssetType type, IUser user) => Read(user).Assets;

    public AssetDto ReadAsset(long id, IUser user) => Read(user).Assets.First(a => a.Id == id);

    public void CreateAsset(IUser user, AssetDto asset)
    {
        var person = Read(user);
        asset.Id = person.Assets.Any() ? person.Assets.Max(a => a.Id) + 1 : 1;
        person.Assets.Add(asset);
        Update(person);


        //int newId = DataBase.GetValue("SELECT MAX(AssetID) FROM Assets").ToInt() + 1;
        //var sql = $@"
        //    INSERT INTO Assets (Id, UserId, Type, Title, Price, SellPrice, Qtty, Mortgage, CashFlow, BigCircle, IsDraft, IsDeleted)
        //    VALUES
        //    (
        //        {newId},
        //        {asset.UserId},
        //        {(int)asset.Type},
        //        '{asset.Title.Replace("'", "''")}',
        //        {asset.Price},
        //        {asset.SellPrice},
        //        {asset.Qtty},
        //        {asset.Mortgage},
        //        {asset.CashFlow},
        //        {asset.BigCircle},
        //        {(asset.IsDraft ? 1 : 0)},
        //        {(asset.IsDeleted ? 1 : 0)}
        //    );";
        //DataBase.Execute(sql);

        ////return Read(newId, asset.UserId);
    }

    public void DeleteAsset(PersonDto person, AssetDto asset)
    {
        var index = person.Assets.FindIndex(a => a.Id == asset.Id);
        person.Assets[index].IsDeleted = true;
        Update(person);
    }

    public void DeleteAsset(IUser user, AssetDto asset)
    {
        var person = Read(user);
        var index = person.Assets.FindIndex(a => a.Id == asset.Id);
        person.Assets[index].IsDeleted = true;
        Update(person);
    }

    public void RestoreAsset(PersonDto person, AssetDto asset)
    {
        var index = person.Assets.FindIndex(a => a.Id == asset.Id);
        person.Assets[index].IsDeleted = false;
        Update(person);
    }

    public void UpdateAsset(IUser user, AssetDto asset)
    {
        var person = Read(user);
        var index = person.Assets.FindIndex(a => a.Id == asset.Id);
        person.Assets[index] = asset;

        Update(person);
    }

    public void SellAsset(AssetDto asset, ActionType action, int price, IUser user)
    {
        asset.SellPrice = price;
        asset.MarkedToSell = false;

        UpdateAsset(user, asset);
        DeleteAsset(user, asset);
        //AddHistory(action, asset.Id, user);
    }

    public string GetAssetDescription(AssetDto asset, IUser user)
    {
        var mortgage = Terms.Get(43, user, "Mortgage");
        var price = Terms.Get(64, user, "Price");
        var cashFlow = Terms.Get(55, user, "Cash Flow");

        return asset.Type switch
        {
            AssetType.Stock => asset.IsDeleted
                            ? $"*{asset.Title}* - {asset.Qtty} @ {asset.SellPrice.AsCurrency()}"
                            : asset.CashFlow == 0
                                ? $"*{asset.Title}* - {asset.Qtty} @ {asset.Price.AsCurrency()}"
                                : $"*{asset.Title}* - {asset.Qtty} @ {asset.Price.AsCurrency()}, {cashFlow}: {asset.CashFlow.AsCurrency()} x {asset.Qtty} = {(asset.CashFlow * asset.Qtty).AsCurrency()}",

            AssetType.RealEstate => asset.IsDeleted
                            ? $"*{asset.Title}* - {price}: {asset.SellPrice.AsCurrency()}"
                            : $"*{asset.Title}* - {price}: {asset.Price.AsCurrency()}, {mortgage}: {asset.Mortgage.AsCurrency()}, {cashFlow}: {asset.CashFlow.AsCurrency()}",

            AssetType.Land => asset.IsDeleted
                            ? $"*{asset.Title}* - {price}: {asset.SellPrice.AsCurrency()}"
                            : $"*{asset.Title}* - {price}: {asset.Price.AsCurrency()}",

            AssetType.Business => asset.IsDeleted
                            ? $"*{asset.Title}* - {price}: {asset.SellPrice.AsCurrency()}"
                            : asset.Mortgage > 0
                                ? $"*{asset.Title}* - {price}: {asset.Price.AsCurrency()}, {mortgage}: {asset.Mortgage.AsCurrency()}, {cashFlow}: {asset.CashFlow.AsCurrency()}"
                                : $"*{asset.Title}* - {price}: {asset.Price.AsCurrency()}, {cashFlow}: {asset.CashFlow.AsCurrency()}",

            AssetType.Boat => asset.CashFlow == 0
                            ? $"*{asset.Title}* - {price}: {asset.Price.AsCurrency()}"
                            : $"*{asset.Title}* - {price}: {asset.Price.AsCurrency()}, {Terms.Get(42, user, "monthly")}: {(-asset.CashFlow).AsCurrency()}",

            AssetType.SmallBusinessType => asset.CashFlow == 0
                            ? $"*{asset.Title}* - {price}: {asset.Price.AsCurrency()}"
                            : $"*{asset.Title}* - {price}: {asset.Price.AsCurrency()}, {Terms.Get(42, user, "monthly")}: {asset.CashFlow.AsCurrency()}",

            AssetType.Coin => asset.IsDeleted
                            ? $"*{asset.Title}* - {asset.Qtty} @ {asset.SellPrice.AsCurrency()}"
                            : $"*{asset.Title}* - {asset.Qtty} @ {asset.Price.AsCurrency()}",

            _ => string.Empty,
        };
    }

    #endregion
}

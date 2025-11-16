using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Interfaces;

namespace CashFlow.Data.Users.UserData.HistoryData;

public class History(IDataBase dataBase, IUser user) : IHistory
{
    private IDataBase DataBase { get; } = dataBase;
    private IUser User { get; } = user;

    private ITermsService Terms => new TermsService(DataBase);
    public int Count(ActionType type) => Records.Count(x => x.Action == type);
    public bool IsEmpty => !Records.Any();
    private IEnumerable<HistoryRecord> Records
    {
        get
        {
            var result = new List<HistoryRecord>();
            var records = DataBase.GetRows_OLD($"SELECT ID, UserID, ActionType, Value, Description FROM History WHERE UserID = {User.Id}");

            foreach (var item in records)
            {
                var record = new HistoryRecord(DataBase).Init(item);
                result.Add(record);
            }

            return result;
        }
    }

    public string Description => IsEmpty
        ? Terms.Get(111, User, "No records found.")
        : string.Join(Environment.NewLine, Records.Select(x => x.Description));

    public string TopFive => IsEmpty
        ? Terms.Get(111, User, "No records found.")
        : string.Join(Environment.NewLine, Records.Reverse().Take(5).Select(x => x.Description));

    public void Clear() => DataBase.Execute($"DELETE FROM History WHERE UserID = {User.Id}");

    public void Add(ActionType action, long value = 0) => new HistoryRecord(DataBase).Init(User, action, value).Add();

    public void Rollback()
    {
        if (IsEmpty) return;

        var record = Records.Last();
        var asset = new Asset_OLD(DataBase, User, (int)record.Value);
        var amount = (int)record.Value;
        var person = Persons.Get(User.Person_OBSOLETE.Profession);

        decimal percent;
        int expenses;

        switch (record.Action)
        {
            case ActionType.PayMoney:
            case ActionType.Downsize:
            case ActionType.Charity:
                User.Person_OBSOLETE.Cash += amount;
                break;

            case ActionType.GetMoney:
                User.Person_OBSOLETE.Cash -= amount;
                break;

            case ActionType.Child:
                User.Person_OBSOLETE.Expenses.Children--;
                break;

            case ActionType.Credit:
                User.Person_OBSOLETE.Cash -= amount;
                User.Person_OBSOLETE.Expenses.BankLoan -= amount / 10;
                User.Person_OBSOLETE.Liabilities.BankLoan -= amount;
                break;

            case ActionType.Mortgage:
                User.Person_OBSOLETE.Cash += amount;
                User.Person_OBSOLETE.Expenses.Mortgage = person.Expenses.Mortgage;
                User.Person_OBSOLETE.Liabilities.Mortgage = person.Liabilities.Mortgage;
                break;

            case ActionType.SchoolLoan:
                User.Person_OBSOLETE.Cash += amount;
                User.Person_OBSOLETE.Expenses.SchoolLoan = person.Expenses.SchoolLoan;
                User.Person_OBSOLETE.Liabilities.SchoolLoan = person.Liabilities.SchoolLoan;
                break;

            case ActionType.CarLoan:
                User.Person_OBSOLETE.Cash += amount;
                User.Person_OBSOLETE.Expenses.CarLoan = person.Expenses.CarLoan;
                User.Person_OBSOLETE.Liabilities.CarLoan = person.Liabilities.CarLoan;
                break;

            case ActionType.CreditCard:
                percent = (decimal)person.Expenses.CreditCard / person.Liabilities.CreditCard;
                expenses = (int)(amount * percent);

                User.Person_OBSOLETE.Cash += amount;
                User.Person_OBSOLETE.Expenses.CreditCard += expenses;
                User.Person_OBSOLETE.Liabilities.CreditCard += amount;
                break;

            case ActionType.SmallCredit:
                percent = (decimal)person.Expenses.SmallCredits / person.Liabilities.SmallCredits;
                expenses = (int)(amount * percent);

                User.Person_OBSOLETE.Cash += amount;
                User.Person_OBSOLETE.Expenses.SmallCredits += expenses;
                User.Person_OBSOLETE.Liabilities.SmallCredits += amount;
                break;

            case ActionType.BankLoan:
                percent = 0.1m;
                expenses = (int)(amount * percent);

                User.Person_OBSOLETE.Cash += amount;
                User.Person_OBSOLETE.Expenses.BankLoan += expenses;
                User.Person_OBSOLETE.Liabilities.BankLoan += amount;
                break;

            case ActionType.BankruptcyBankLoan:
                percent = 0.1m;
                expenses = (int)(amount * percent);

                User.Person_OBSOLETE.Cash += amount;
                User.Person_OBSOLETE.Expenses.BankLoan += expenses;
                User.Person_OBSOLETE.Liabilities.BankLoan += amount;
                User.Person_OBSOLETE.Bankruptcy = true;
                break;

            case ActionType.BuyRealEstate:
            case ActionType.BuyBusiness:
            case ActionType.BuyLand:
            case ActionType.StartCompany:
                User.Person_OBSOLETE.Cash += asset.Price - asset.Mortgage;
                asset.Delete();
                break;

            case ActionType.IncreaseCashFlow:
                User.Person_OBSOLETE.Assets.SmallBusinesses.ForEach(x => x.CashFlow -= (int)record.Value);
                break;

            case ActionType.SellRealEstate:
            case ActionType.SellBusiness:
            case ActionType.SellLand:
                User.Person_OBSOLETE.Cash -= asset.SellPrice - asset.Mortgage;
                asset.Restore();
                break;

            case ActionType.BuyStocks:
            case ActionType.BuyCoins:
                User.Person_OBSOLETE.Cash += asset.Price * asset.Qtty;
                asset.Delete();
                break;

            case ActionType.SellStocks:
            case ActionType.SellCoins:
                User.Person_OBSOLETE.Cash -= asset.Qtty * asset.SellPrice;
                asset.Restore();
                break;

            case ActionType.Stocks1To2:
                asset.Qtty /= 2;
                break;

            case ActionType.Stocks2To1:
                asset.Qtty *= 2;
                break;

            case ActionType.MicroCredit:
                User.Person_OBSOLETE.Liabilities.CreditCard -= amount;
                User.Person_OBSOLETE.Expenses.CreditCard -= (int)(amount * 0.03);
                break;

            case ActionType.BuyBoat:
                User.Person_OBSOLETE.Cash += 1_000;
                User.Person_OBSOLETE.Assets.Boat.Delete();
                break;

            case ActionType.PayOffBoat:
                User.Person_OBSOLETE.Cash += amount;
                User.Person_OBSOLETE.Assets.Boat.CashFlow = 340;
                break;

            case ActionType.Bankruptcy:
                User.Person_OBSOLETE.Bankruptcy = false;
                break;

            case ActionType.BankruptcySellAsset:
                User.Person_OBSOLETE.Cash -= asset.BancrupcySellPrice;
                User.Person_OBSOLETE.Bankruptcy = true;
                asset.Restore();
                break;

            case ActionType.BankruptcyDebtRestructuring:
                User.Person_OBSOLETE.ReduceCreditsRollback();
                break;

            case ActionType.GoToBigCircle:
                User.Person_OBSOLETE.Cash -= User.Person_OBSOLETE.InitialCashFlow;
                User.Person_OBSOLETE.InitialCashFlow = 0;
                User.Person_OBSOLETE.Circle = Circle.Small;
                break;

            case ActionType.Divorce:
            case ActionType.TaxAudit:
            case ActionType.Lawsuit:
                User.Person_OBSOLETE.Cash += amount;
                break;

            default:
                throw new Exception($"<{record.Action}> ???");
        }

        record.Delete();
    }
}

using CashFlowBot.DataBase;
using CashFlowBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Terms = CashFlowBot.Data.Terms;

namespace CashFlowBot.Data.Users;

public class History(IDataBase dataBase, IUser user) : IHistory
{
    private IDataBase DataBase { get; } = dataBase;
    private IUser User { get; } = user;

    private Terms Terms => new Terms(DataBase);
    public int Count(ActionType type) => Records.Count(x => x.Action == type);
    public bool IsEmpty => !Records.Any();
    private IEnumerable<HistoryRecord> Records
    {
        get
        {
            var result = new List<HistoryRecord>();
            var records = DataBase.GetRows($"SELECT ID, UserID, ActionType, Value, Description FROM History WHERE UserID = {User.Id}");

            foreach (var item in records)
            {
                var record = new HistoryRecord(DataBase).Init(item);
                result.Add(record);
            }

            return result;
        }
    }

    public string Description => IsEmpty
        ? Terms.Get(111, User.Id, "No records found.")
        : string.Join(Environment.NewLine, Records.Select(x => x.Description));

    public string TopFive => IsEmpty
        ? Terms.Get(111, User.Id, "No records found.")
        : string.Join(Environment.NewLine, Records.Reverse().Take(5).Select(x => x.Description));

    public void Clear() => DataBase.Execute($"DELETE FROM History WHERE UserID = {User.Id}");

    public void Add(ActionType action, long value = 0) => new HistoryRecord(DataBase).Init(User, action, value).Add();

    public void Rollback()
    {
        if (IsEmpty) return;

        var user = new User(DataBase, User.Id);
        var record = Records.Last();
        var asset = new Asset(DataBase, user, (int)record.Value);
        var amount = (int)record.Value;
        var person = Persons.Get(user.Id, user.Person.Profession);

        decimal percent;
        int expenses;

        switch (record.Action)
        {
            case ActionType.PayMoney:
            case ActionType.Downsize:
            case ActionType.Charity:
                user.Person.Cash += amount;
                break;

            case ActionType.GetMoney:
                user.Person.Cash -= amount;
                break;

            case ActionType.Child:
                user.Person.Expenses.Children--;
                break;

            case ActionType.Credit:
                user.Person.Cash -= amount;
                user.Person.Expenses.BankLoan -= amount / 10;
                user.Person.Liabilities.BankLoan -= amount;
                break;

            case ActionType.Mortgage:
                user.Person.Cash += amount;
                user.Person.Expenses.Mortgage = person.Expenses.Mortgage;
                user.Person.Liabilities.Mortgage = person.Liabilities.Mortgage;
                break;

            case ActionType.SchoolLoan:
                user.Person.Cash += amount;
                user.Person.Expenses.SchoolLoan = person.Expenses.SchoolLoan;
                user.Person.Liabilities.SchoolLoan = person.Liabilities.SchoolLoan;
                break;

            case ActionType.CarLoan:
                user.Person.Cash += amount;
                user.Person.Expenses.CarLoan = person.Expenses.CarLoan;
                user.Person.Liabilities.CarLoan = person.Liabilities.CarLoan;
                break;

            case ActionType.CreditCard:
                percent = (decimal)person.Expenses.CreditCard / person.Liabilities.CreditCard;
                expenses = (int)(amount * percent);

                user.Person.Cash += amount;
                user.Person.Expenses.CreditCard += expenses;
                user.Person.Liabilities.CreditCard += amount;
                break;

            case ActionType.SmallCredit:
                percent = (decimal)person.Expenses.SmallCredits / person.Liabilities.SmallCredits;
                expenses = (int)(amount * percent);

                user.Person.Cash += amount;
                user.Person.Expenses.SmallCredits += expenses;
                user.Person.Liabilities.SmallCredits += amount;
                break;

            case ActionType.BankLoan:
                percent = 0.1m;
                expenses = (int)(amount * percent);

                user.Person.Cash += amount;
                user.Person.Expenses.BankLoan += expenses;
                user.Person.Liabilities.BankLoan += amount;
                break;

            case ActionType.BankruptcyBankLoan:
                percent = 0.1m;
                expenses = (int)(amount * percent);

                user.Person.Cash += amount;
                user.Person.Expenses.BankLoan += expenses;
                user.Person.Liabilities.BankLoan += amount;
                user.Person.Bankruptcy = true;
                break;

            case ActionType.BuyRealEstate:
            case ActionType.BuyBusiness:
            case ActionType.BuyLand:
            case ActionType.StartCompany:
                user.Person.Cash += asset.Price - asset.Mortgage;
                asset.Delete();
                break;

            case ActionType.IncreaseCashFlow:
                user.Person.Assets.SmallBusinesses.ForEach(x => x.CashFlow -= (int)record.Value);
                break;

            case ActionType.SellRealEstate:
            case ActionType.SellBusiness:
            case ActionType.SellLand:
                user.Person.Cash -= asset.SellPrice - asset.Mortgage;
                asset.Restore();
                break;

            case ActionType.BuyStocks:
            case ActionType.BuyCoins:
                user.Person.Cash += asset.Price * asset.Qtty;
                asset.Delete();
                break;

            case ActionType.SellStocks:
            case ActionType.SellCoins:
                user.Person.Cash -= asset.Qtty * asset.SellPrice;
                asset.Restore();
                break;

            case ActionType.Stocks1To2:
                asset.Qtty /= 2;
                break;

            case ActionType.Stocks2To1:
                asset.Qtty *= 2;
                break;

            case ActionType.MicroCredit:
                user.Person.Liabilities.CreditCard -= amount;
                user.Person.Expenses.CreditCard -= (int)(amount * 0.03);
                break;

            case ActionType.BuyBoat:
                user.Person.Cash += 1_000;
                user.Person.Assets.Boat.Delete();
                break;

            case ActionType.PayOffBoat:
                user.Person.Cash += amount;
                user.Person.Assets.Boat.CashFlow = 340;
                break;

            case ActionType.Bankruptcy:
                user.Person.Bankruptcy = false;
                break;

            case ActionType.BankruptcySellAsset:
                user.Person.Cash -= asset.BancrupcySellPrice;
                user.Person.Bankruptcy = true;
                asset.Restore();
                break;

            case ActionType.BankruptcyDebtRestructuring:
                user.Person.ReduceCreditsRollback();
                break;

            case ActionType.GoToBigCircle:
                user.Person.Cash -= user.Person.InitialCashFlow;
                user.Person.InitialCashFlow = 0;
                user.Person.Circle = Circle.Small;
                break;

            case ActionType.Divorce:
            case ActionType.TaxAudit:
            case ActionType.Lawsuit:
                user.Person.Cash += amount;
                break;

            default:
                throw new Exception($"<{record.Action}> ???");
        }

        record.Delete();
    }
}

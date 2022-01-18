using System;
using System.Collections.Generic;
using System.Linq;
using CashFlowBot.Data;
using CashFlowBot.DataBase;
using CashFlowBot.Extensions;
using Terms = CashFlowBot.DataBase.Terms;

namespace CashFlowBot.Models
{
    public class History
    {
        private static string Table => DB.Tables.History;
        private readonly long _userId;

        public History(long userId) => _userId = userId;

        public bool IsEmpty => !Records.Any();
        private IEnumerable<HistoryRecord> Records
        {
            get
            {
                var result = new List<HistoryRecord>();
                var records = DB.GetRows($"SELECT {DB.ColumnNames.History} FROM {Table} WHERE UserID = {_userId}");

                foreach (var item in records)
                {
                    var record = new HistoryRecord
                    {
                        Id          = item[0].ToLong(),
                        UserId      = item[1].ToLong(),
                        Action      = item[2].ParseEnum<ActionType>(),
                        Value       = item[3].ToLong(),
                        Description = item[4]
                    };

                    result.Add(record);
                }

                return result;
            }
        }

        public string Description => IsEmpty
            ? Terms.Get(111, _userId, "No records found.")
            : string.Join(Environment.NewLine, Records.Select(x => x.Description));

        public void Clear() => DB.Execute($"DELETE FROM {Table} WHERE UserID = {_userId}");

        public void Add(ActionType action, long value) => new HistoryRecord { UserId = _userId, Action = action, Value = value }.Add();

        public void Rollback()
        {
            if (IsEmpty) return;

            var user = new User(_userId);
            var record = Records.Last();
            var asset = new Asset(_userId, (int) record.Value);
            var amount = (int) record.Value;
            var person = Persons.Get(_userId).First(x => x.Profession == user.Person.Profession);
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
                    percent = (decimal) person.Expenses.Mortgage / person.Liabilities.Mortgage;
                    expenses = (int) (amount * percent);

                    user.Person.Cash += amount;
                    user.Person.Expenses.Mortgage += expenses;
                    user.Person.Liabilities.Mortgage += amount;
                    break;

                case ActionType.SchoolLoan:
                    percent = (decimal) person.Expenses.SchoolLoan / person.Liabilities.SchoolLoan;
                    expenses = (int) (amount * percent);

                    user.Person.Cash += amount;
                    user.Person.Expenses.SchoolLoan += expenses;
                    user.Person.Liabilities.SchoolLoan += amount;
                    break;

                case ActionType.CarLoan:
                    percent = (decimal) person.Expenses.CarLoan / person.Liabilities.CarLoan;
                    expenses = (int) (amount * percent);

                    user.Person.Cash += amount;
                    user.Person.Expenses.CarLoan += expenses;
                    user.Person.Liabilities.CarLoan += amount;
                    break;

                case ActionType.CreditCard:
                    percent = (decimal) person.Expenses.CreditCard / person.Liabilities.CreditCard;
                    expenses = (int) (amount * percent);

                    user.Person.Cash += amount;
                    user.Person.Expenses.CreditCard += expenses;
                    user.Person.Liabilities.CreditCard += amount;
                    break;

                case ActionType.SmallCredit:
                    percent = (decimal) person.Expenses.SmallCredits / person.Liabilities.SmallCredits;
                    expenses = (int) (amount * percent);

                    user.Person.Cash += amount;
                    user.Person.Expenses.SmallCredits += expenses;
                    user.Person.Liabilities.SmallCredits += amount;
                    break;

                case ActionType.BankLoan:
                    percent = 0.1m;
                    expenses = (int) (amount * percent);

                    user.Person.Cash += amount;
                    user.Person.Expenses.BankLoan += expenses;
                    user.Person.Liabilities.BankLoan += amount;
                    break;

                case ActionType.BuyRealEstate:
                case ActionType.BuyBusiness:
                case ActionType.BuyLand:
                    user.Person.Cash += asset.Price - asset.Mortgage;
                    asset.Delete();
                    break;

                case ActionType.SellRealEstate:
                case ActionType.SellBusiness:
                case ActionType.SellLand:
                    user.Person.Cash -= asset.SellPrice - asset.Mortgage;
                    asset.Restore();
                    break;

                case ActionType.BuyStocks:
                    user.Person.Cash += asset.Qtty * asset.Price;
                    asset.Delete();
                    break;

                case ActionType.SellStocks:
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
                    user.Person.Liabilities.SmallCredits -= amount;
                    user.Person.Expenses.SmallCredits -= (int) (amount * 0.05);
                    break;

                default:
                    throw new Exception($"<{record.Action}> ???");
            }

            record.Delete();
        }
    }

    public class HistoryRecord
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public ActionType Action { get; set; }
        public long Value { get; set; }
        public string Description { get; set; }

        public void Add()
        {
            long newId = DB.GetValue($"SELECT MAX(ID) FROM {DB.Tables.History}").ToLong() + 1;
            DB.Execute($@"INSERT INTO {DB.Tables.History} VALUES ({newId}, {UserId}, {(int)Action}, {Value}, '• {Text}')");
        }

        public void Delete() => DB.Execute($"DELETE FROM {DB.Tables.History} WHERE ID = {Id}");

        private Asset Asset => new (UserId, (int)Value);
        private string Text
        {
            get
            {
                switch (Action)
                {
                    case ActionType.PayMoney:
                        return Terms.Get(103, UserId, "Pay {0}", Value.AsCurrency());

                    case ActionType.GetMoney:
                        return Terms.Get(104, UserId, "Get {0}", Value.AsCurrency());

                    case ActionType.Child:
                        return Terms.Get(105, UserId, "Get a child");

                    case ActionType.Downsize:
                        return Terms.Get(106, UserId, "Downsize and paying {0}", Value.AsCurrency());

                    case ActionType.Credit:
                        return Terms.Get(107, UserId, "Get credit: {0}", Value.AsCurrency());

                    case ActionType.Charity:
                        return Terms.Get(108, UserId, "Charity: {0}", Value.AsCurrency());

                    case ActionType.Mortgage:
                    case ActionType.SchoolLoan:
                    case ActionType.CarLoan:
                    case ActionType.CreditCard:
                    case ActionType.SmallCredit:
                    case ActionType.BankLoan:
                        var reduceLiabilities = Terms.Get(40, UserId, "Reduce Liabilities");
                        var type = Terms.Get((int)Action, UserId, "Liability");
                        var amount = Value.AsCurrency();
                        return $"{reduceLiabilities}. {type}: {amount}";

                    case ActionType.BuyRealEstate:
                    case ActionType.BuyBusiness:
                    case ActionType.BuyStocks:
                    case ActionType.BuyLand:
                        var buyAsset = Terms.Get((int) Action, UserId, "Buy Asset");
                        return $"{buyAsset}. {Asset.Description}";

                    case ActionType.SellRealEstate:
                    case ActionType.SellBusiness:
                    case ActionType.SellStocks:
                    case ActionType.SellLand:
                        var sellAsset = Terms.Get((int) Action, UserId, "Sell Asset");
                        return $"{sellAsset}. {Asset.Description}";

                    case ActionType.Stocks1To2:
                    case ActionType.Stocks2To1:
                        var multiply = Terms.Get((int) Action, UserId, "Multiply Stocks");
                        return $"{multiply}. {Asset.Description}";

                    case ActionType.MicroCredit:
                        return Terms.Get(96, UserId, "Pay with Credit Card") + " - " + Value.AsCurrency();

                    default:
                        return $"<{Action}> - {Value}";
                }
            }
        }
    }
}

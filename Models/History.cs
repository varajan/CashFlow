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
                        UserId = item[0].ToLong(),
                        Action = item[1].ParseEnum<ActionType>(),
                        Value = item[2].ToLong(),
                        Description = item[3]
                    };

                    result.Add(record);
                }

                return result;
            }
        }

        public string Description => string.Join(Environment.NewLine, Records.Select(x => x.Description));

        public void Clear() => DB.Execute($"DELETE FROM {Table} WHERE ID = {_userId}");

        public void Add(ActionType action, long value) => new HistoryRecord { UserId = _userId, Action = action, Value = value }.Save();
    }

    public class HistoryRecord
    {
        public long UserId { get; set; }
        public ActionType Action { get; set; }
        public long Value { get; set; }
        public string Description { get; set; }

        public void Save() => DB.Execute($@"INSERT INTO {DB.Tables.History} VALUES ({UserId}, {(int) Action}, {Value}, '{Text}')");

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

                    default:
                        return $"<{Action}> - {Value}";

                    //var person = Persons.Get(user.Id).First(x => x.Profession == user.Person.Profession);
                    //var perZent = (decimal) person.Expenses.SmallCredits / person.Liabilities.SmallCredits;
                }
            }
        }
    }
}

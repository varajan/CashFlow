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

        private List<HistoryRecord> Records
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
                        Amount = item[2].ToInt(),
                        Percent = item[3].ToDecimal(),
                    };

                    result.Add(record);
                }

                return result;
            }
        }

        public string Description => string.Join(Environment.NewLine, Records.Select(x => x.Description));

        public void Clear() => DB.Execute($"DELETE FROM {Table} WHERE ID = {_userId}");

        public void Add(ActionType action, int amount, decimal percent = 0)
        {
            DB.Execute($@"INSERT INTO {Table} VALUES ({_userId}, {(int)action}, {amount}, '{percent}')");
        }
    }

    public class HistoryRecord
    {
        public long UserId { get; set; }
        public ActionType Action { get; set; }
        public int Amount { get; set; }
        public decimal Percent { get; set; }

        public string Description
        {
            get
            {
                switch (Action)
                {
                    case ActionType.PayMoney:
                        return Terms.Get(103, UserId, "Pay {0}", Amount.AsCurrency());

                    case ActionType.GetMoney:
                        return Terms.Get(104, UserId, "Get {0}", Amount.AsCurrency());

                    case ActionType.Child:
                        return Terms.Get(105, UserId, "Get a child");

                    case ActionType.Downsize:
                        return Terms.Get(106, UserId, "Downsize and paying {0}", Amount.AsCurrency());

                    case ActionType.Credit:
                        return Terms.Get(107, UserId, "Get credit: {0}", Amount.AsCurrency());

                    case ActionType.Charity:
                        return Terms.Get(108, UserId, "Charity: {0}", Amount.AsCurrency());

                    case ActionType.Mortgage:
                    case ActionType.SchoolLoan:
                    case ActionType.CarLoan:
                    case ActionType.CreditCard:
                    case ActionType.SmallCredit:
                    case ActionType.BankLoan:
                        var reduceLiabilities = Terms.Get(40, UserId, "Reduce Liabilities");
                        var type = Terms.Get((int)Action, UserId, "Liability");
                        var amount = Amount.AsCurrency();
                        return $"{reduceLiabilities}. {type}: {amount}";

                    default:
                        return $"<{Action}> - {Amount}";
                }
            }
        }
    }
}

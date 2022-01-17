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
                        Amount = item[2].ToInt()
                    };

                    result.Add(record);
                }

                return result;
            }
        }

        public string Description => string.Join(Environment.NewLine, Records.Select(x => x.Description));

        public void Clear() => DB.Execute($"DELETE FROM {Table} WHERE ID = {_userId}");

        public void Add(ActionType action, int amount)
        {
            DB.Execute($@"INSERT INTO {Table} VALUES ({_userId}, {(int)action}, {amount})");
        }
    }

    public class HistoryRecord
    {
        public long UserId { get; set; }
        public ActionType Action { get; set; }
        public int Amount { get; set; }

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

                    default:
                        return $"<{Action}> - {Amount}";
                }
            }
        }
    }
}

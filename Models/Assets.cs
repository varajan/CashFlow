using System;
using System.Collections.Generic;
using System.Linq;
using CashFlowBot.Data;
using CashFlowBot.DataBase;
using CashFlowBot.Extensions;

namespace CashFlowBot.Models
{
    public class Assets
    {
        private long Id { get; }
        public Assets(long id) => Id = id;

        public List<Asset> Items =>
            DB.GetColumn($"SELECT AssetID FROM {DB.Tables.Assets} WHERE UserID = {Id}")
                .Select(id => new Asset(userId: Id, id: id.ToInt()))
                .ToList();

        public string Description => Items.Any()
            ? $"{Environment.NewLine}{Environment.NewLine}*Assets:*{Environment.NewLine}{string.Join(Environment.NewLine, Items.Select(x => x.Description))}"
            : string.Empty;

        public void Add(string title, AssetType type)
        {
            int newId = DB.GetValue($"SELECT MAX(AssetID) FROM {DB.Tables.Assets}").ToInt() + 1;
            DB.Execute($"INSERT INTO {DB.Tables.Assets} ({DB.ColumnNames.Assets}) VALUES ({newId}, {Id}, {(int)type}, '{title}', 0, 0)");
        }
    }
}

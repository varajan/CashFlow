using System;
using System.Collections.Generic;
using System.Linq;
using CashFlowBot.DataBase;
using CashFlowBot.Extensions;

namespace CashFlowBot.Models
{
    public class Assets
    {
        private long Id { get; }
        public Assets(long id) => Id = id;

        public List<Asset> Items =>
            DB.GetColumn($"SELECT ID FROM {DB.Tables.Assets} WHERE UserId = {Id}")
                .Select(id => new Asset(userId: Id, id: id.ToInt()))
                .ToList();

        public string Description => Items.Any()
            ? $"*Assets:*{Environment.NewLine}{string.Join(Environment.NewLine, Items.Select(x => x.Description))}"
            : string.Empty;

        public void Add(string title)
        {
            int newId = DB.GetValue($"SELECT MAX(ID) FROM {DB.Tables.Assets}").ToInt() + 1;
            DB.Execute($"INSERT INTO {DB.Tables.Assets} ({DB.ColumnNames.Assets}) VALUES ({newId}, {Id}, '{title}', 0, 0)");
        }
    }
}

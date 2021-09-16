﻿using System;
using System.Collections.Generic;
using System.Linq;
using CashFlowBot.Data;
using CashFlowBot.DataBase;
using CashFlowBot.Extensions;
using MoreLinq;
using Terms = CashFlowBot.DataBase.Terms;

namespace CashFlowBot.Models
{
    public class Assets
    {
        private long Id { get; }
        public Assets(long id) => Id = id;

        public void CleanUp()
        {
            Properties.ForEach(x => x.Title = x.Title.SubStringTo("*"));

            Stocks.ForEach(x => x.Title = x.Title.SubStringTo("*"));
            Stocks.Where(x => x.Price ==0 || x.Qtty == 0).ForEach(x => x.Delete());
        }

        public List<Asset> Stocks => Items.Where(x => x.Type == AssetType.Stock).ToList();
        public List<Asset> Properties => Items.Where(x => x.Type == AssetType.Property).ToList();

        public List<Asset> Items =>
            DB.GetColumn($"SELECT AssetID FROM {DB.Tables.Assets} WHERE UserID = {Id}")
                .Select(id => new Asset(userId: Id, id: id.ToInt()))
                .Where(x => x.BigCircle == ThisUser.Person.BigCircle)
                .ToList();

        private User ThisUser => new (Id);

        public int Income => Properties.Sum(x => x.CashFlow);

        public string Description => Items.Any()
            ? $"{Environment.NewLine}{Environment.NewLine}*{Terms.Get(56, Id, "Assets")}:*{Environment.NewLine}{string.Join(Environment.NewLine, Items.OrderBy(x => x.Type).Select(x => x.Description))}"
            : string.Empty;

        public void Add(string title, AssetType type, bool bigCircle = false)
        {
            int newId = DB.GetValue($"SELECT MAX(AssetID) FROM {DB.Tables.Assets}").ToInt() + 1;
            DB.Execute($"INSERT INTO {DB.Tables.Assets} ({DB.ColumnNames.Assets}) VALUES ({newId}, {Id}, {(int)type}, {(bigCircle ? 1 : 0)}, '{title}', 0, 0, 0, 0)");
        }
    }
}

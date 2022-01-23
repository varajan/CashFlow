using System;
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
            Items.Where(x => x.IsDraft).ForEach(x => x.Delete());
            Items.ForEach(x => x.Title = x.Title.SubStringTo("*"));
        }

        public List<Asset> Stocks => Items.Where(x => x.Type == AssetType.Stock).ToList();
        public List<Asset> RealEstates => Items.Where(x => x.Type == AssetType.RealEstate).ToList();
        public List<Asset> Businesses => Items.Where(x => x.Type == AssetType.Business && x.BigCircle == ThisUser.Person.BigCircle).ToList();
        public List<Asset> Lands => Items.Where(x => x.Type == AssetType.LandTitle).ToList();
        public Asset Boat => Items.LastOrDefault(i => i.Type == AssetType.Boat);

        public List<Asset> Items =>
            DB.GetColumn($"SELECT AssetID FROM Assets WHERE UserID = {Id}")
                .Select(id => new Asset(userId: Id, id: id.ToInt()))
                .Where(x => !x.IsDeleted)
                .ToList();

        public void Clear() => Items.ForEach(x => x.Delete());

        private User ThisUser => new (Id);

        public int Income => RealEstates.Sum(x => x.CashFlow) + Businesses.Sum(x => x.CashFlow);

        public string Description => Items.Any()
            ? $"{Environment.NewLine}{Environment.NewLine}*{Terms.Get(56, Id, "Assets")}:*{Environment.NewLine}{string.Join(Environment.NewLine, Items.OrderBy(x => x.Type).Select(x => x.Description))}"
            : string.Empty;

        public Asset Add(string title, AssetType type, bool bigCircle = false)
        {
            int newId = DB.GetValue("SELECT MAX(AssetID) FROM Assets").ToInt() + 1;
            DB.Execute($"INSERT INTO Assets " +
                       "(AssetID, UserID, Type, Deleted, Draft, BigCircle, Title, Price, Qtty, Mortgage, CashFlow, SellPrice) " +
                       $"VALUES ({newId}, {Id}, {(int)type}, 0, 1, {(bigCircle ? 1 : 0)}, '{title}', 0, 0, 0, 0, 0)");

            return Items.First(i => i.IsDraft);
        }
    }
}

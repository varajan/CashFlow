using CashFlowBot.Data.Users;
using CashFlowBot.DataBase;
using CashFlowBot.Extensions;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using Terms = CashFlowBot.Data.Terms;

namespace CashFlowBot.Data;

public class Assets(IDataBase dataBase, long id)
{
    private IDataBase DataBase { get; } = dataBase;
    private long Id { get; } = id;

    public void CleanUp()
    {
        Items.Where(x => x.IsDraft).ForEach(x => x.Delete());
        Items.ForEach(x => x.Title = x.Title.SubStringTo("*"));
    }

    private Terms Terms => new Terms(DataBase);

    public List<Asset> Stocks => Items.Where(x => x.Type == AssetType.Stock).ToList();
    public List<Asset> RealEstates => Items.Where(x => x.Type == AssetType.RealEstate).ToList();
    public List<Asset> SmallBusinesses => Items.Where(x => x.Type == AssetType.SmallBusinessType).ToList();
    public List<Asset> Coins => Items.Where(x => x.Type == AssetType.Coin).ToList();
    public List<Asset> Businesses => Items.Where(x => x.Type == AssetType.Business && x.BigCircle == ThisUser.Person.BigCircle).ToList();
    public List<Asset> Lands => Items.Where(x => x.Type == AssetType.LandTitle).ToList();
    public Asset Boat => Items.LastOrDefault(i => i.Type == AssetType.Boat);

    public List<Asset> Items =>
        DataBase.GetColumn($"SELECT AssetID FROM Assets WHERE UserID = {Id}")
            .Select(id => new Asset(DataBase, ThisUser, id: id.ToInt()))
            .Where(x => !x.IsDeleted)
            .ToList();

    public void Clear() => Items.ForEach(x => x.Delete());

    public Asset Transfer => Items.SingleOrDefault(x => x.Type == AssetType.Transfer);

    private IUser ThisUser => new User(DataBase, Id);

    public int Income => Items.Where(x => x.Type != AssetType.Boat).Sum(x => x.TotalCashFlow);

    public string BigCircleDescription => Businesses.Any()
        ? $"{Environment.NewLine}{Environment.NewLine}*{Terms.Get(56, Id, "Assets")}:*{Environment.NewLine}{string.Join(Environment.NewLine, Businesses.Select(x => $"• {x.Description}"))}"
        : string.Empty;

    public string Description => Items.Any()
        ? $"{Environment.NewLine}{Environment.NewLine}*{Terms.Get(56, Id, "Assets")}:*{Environment.NewLine}{string.Join(Environment.NewLine, Items.OrderBy(x => x.Type).Select(x => $"• {x.Description}"))}"
        : string.Empty;

    public Asset Add(string title, AssetType type, bool bigCircle = false)
    {
        int newId = DataBase.GetValue("SELECT MAX(AssetID) FROM Assets").ToInt() + 1;
        DataBase.Execute(@"
            INSERT INTO Assets " +
            "(AssetID, UserID, Type, Deleted, Draft, BigCircle, Title, Price, Qtty, Mortgage, CashFlow, SellPrice) " +
            $"VALUES ({newId}, {Id}, {(int)type}, 0, 1, {(bigCircle ? 1 : 0)}, '{title}', 0, 1, 0, 0, 0)");

        return Items.First(i => i.IsDraft);
    }
}
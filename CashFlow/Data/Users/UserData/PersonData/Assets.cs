using CashFlow.Data.Consts;
using CashFlow.Data.DataBase;
using CashFlow.Extensions;

namespace CashFlow.Data.Users.UserData.PersonData;

public class Assets(IDataBase dataBase, IUser user) : IAssets
{
    private IDataBase DataBase { get; } = dataBase;
    private IUser User { get; } = user;

    public void CleanUp()
    {
        Items.Where(x => x.IsDraft).ToList().ForEach(x => x.Delete());
        Items.ForEach(x => x.Title = x.Title.SubStringTo("*"));
    }

    private ITermsService Terms => new TermsService(DataBase);

    public List<Asset_OLD> Stocks => Items.Where(x => x.Type == AssetType.Stock).ToList();
    public List<Asset_OLD> RealEstates => Items.Where(x => x.Type == AssetType.RealEstate).ToList();
    public List<Asset_OLD> SmallBusinesses => Items.Where(x => x.Type == AssetType.SmallBusinessType).ToList();
    public List<Asset_OLD> Coins => Items.Where(x => x.Type == AssetType.Coin).ToList();
    public List<Asset_OLD> Businesses => Items.Where(x => x.Type == AssetType.Business && x.BigCircle == User.Person_OBSOLETE.BigCircle).ToList();
    public List<Asset_OLD> Lands => Items.Where(x => x.Type == AssetType.LandTitle).ToList();
    public Asset_OLD Boat => Items.LastOrDefault(i => i.Type == AssetType.Boat);

    public List<Asset_OLD> Items =>
        DataBase.GetColumn($"SELECT AssetID FROM Assets WHERE UserID = {User.Id}")
            .Select(id => new Asset_OLD(DataBase, User, id: id.ToInt()))
            .Where(x => !x.IsDeleted)
            .ToList();

    public void Clear() => Items.ForEach(x => x.Delete());

    public Asset_OLD Transfer => Items.SingleOrDefault(x => x.Type == AssetType.Transfer);

    public int Income => Items.Where(x => x.Type != AssetType.Boat).Sum(x => x.TotalCashFlow);

    public string BigCircleDescription => Businesses.Any()
        ? $"{Environment.NewLine}{Environment.NewLine}*{Terms.Get(56, User, "Assets")}:*{Environment.NewLine}{string.Join(Environment.NewLine, Businesses.Select(x => $"• {x.Description}"))}"
        : string.Empty;

    public string Description => Items.Any()
        ? $"{Environment.NewLine}{Environment.NewLine}*{Terms.Get(56, User, "Assets")}:*{Environment.NewLine}{string.Join(Environment.NewLine, Items.OrderBy(x => x.Type).Select(x => $"• {x.Description}"))}"
        : string.Empty;

    public Asset_OLD Get(AssetType type) => Items.SingleOrDefault(x => x.Type == type);

    public Asset_OLD Add(string title, AssetType type, bool bigCircle = false)
    {
        int newId = DataBase.GetValue("SELECT MAX(AssetID) FROM Assets").ToInt() + 1;
        DataBase.Execute(@"
            INSERT INTO Assets " +
            "(AssetID, UserID, Type, Deleted, Draft, BigCircle, Title, Price, Qtty, Mortgage, CashFlow, SellPrice) " +
            $"VALUES ({newId}, {User.Id}, {(int)type}, 0, 1, {(bigCircle ? 1 : 0)}, '{title}', 0, 1, 0, 0, 0)");

        return Items.First(i => i.IsDraft);
    }
}
using CashFlow.Data.Consts;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Data.Users.UserData.PersonData;

public class Asset_OLD(IDataBase dataBase, IUser user, int id)
{
    private IDataBase DataBase { get; } = dataBase;
    private IUser User { get; } = user;
    public long Id { get; } = id;

    private ITermsService Terms => new TermsService(DataBase);
    private string Get(string column) => DataBase.GetValue($"SELECT {column} FROM Assets WHERE AssetID = {Id} AND UserID = {User.Id}");
    private int GetInt(string column) => Get(column).ToInt();
    private void Set(string column, int value) => DataBase.Execute($"UPDATE Assets SET {column} = {value} WHERE AssetID = {Id} AND UserID = {User.Id}");
    private void Set(string column, string value) => DataBase.Execute($"UPDATE Assets SET {column} = '{value}' WHERE AssetID = {Id} AND UserID = {User.Id}");

    public string Description
    {
        get
        {
            var mortgage = Terms.Get(43, User, "Mortgage");
            var price = Terms.Get(64, User, "Price");
            var cashFlow = Terms.Get(55, User, "Cashflow");

            switch (Type)
            {
                case AssetType.Stock:
                    return IsDeleted
                        ? $"*{Title}* - {Qtty} @ {SellPrice.AsCurrency()}"
                        : CashFlow == 0
                            ? $"*{Title}* - {Qtty} @ {Price.AsCurrency()}"
                            : $"*{Title}* - {Qtty} @ {Price.AsCurrency()}, {cashFlow}: {CashFlow.AsCurrency()} x {Qtty} = {(CashFlow * Qtty).AsCurrency()}";

                case AssetType.RealEstate:
                    return IsDeleted
                    ? $"*{Title}* - {price}: {SellPrice.AsCurrency()}"
                    : $"*{Title}* - {price}: {Price.AsCurrency()}, {mortgage}: {Mortgage.AsCurrency()}, {cashFlow}: {CashFlow.AsCurrency()}";

                case AssetType.LandTitle:
                    return IsDeleted
                        ? $"*{Title}* - {price}: {SellPrice.AsCurrency()}"
                        : $"*{Title}* - {price}: {Price.AsCurrency()}";

                case AssetType.Business:
                    return IsDeleted
                        ? $"*{Title}* - {price}: {SellPrice.AsCurrency()}"
                        : Mortgage > 0
                            ? $"*{Title}* - {price}: {Price.AsCurrency()}, {mortgage}: {Mortgage.AsCurrency()}, {cashFlow}: {CashFlow.AsCurrency()}"
                            : $"*{Title}* - {price}: {Price.AsCurrency()}, {cashFlow}: {CashFlow.AsCurrency()}";

                case AssetType.Boat:
                    return CashFlow == 0
                    ? $"*{Title}* - {price}: {Price.AsCurrency()}"
                    : $"*{Title}* - {price}: {Price.AsCurrency()}, {Terms.Get(42, User, "monthly")}: {(-CashFlow).AsCurrency()}";

                case AssetType.SmallBusinessType:
                    return CashFlow == 0
                    ? $"*{Title}* - {price}: {Price.AsCurrency()}"
                    : $"*{Title}* - {price}: {Price.AsCurrency()}, {Terms.Get(42, User, "monthly")}: {CashFlow.AsCurrency()}";

                case AssetType.Coin:
                    return IsDeleted
                        ? $"*{Title}* - {Qtty} @ {SellPrice.AsCurrency()}"
                        : $"*{Title}* - {Qtty} @ {Price.AsCurrency()}";

                default:
                    return string.Empty;
            }
        }
    }

    public int BancrupcySellPrice
    {
        get
        {
            switch (Type)
            {
                case AssetType.Coin:
                case AssetType.Stock:
                    return Qtty * Price / 2;

                case AssetType.LandTitle:
                case AssetType.SmallBusinessType:
                    return Price / 2;

                case AssetType.RealEstate:
                case AssetType.Business:
                    return (Price - Mortgage) / 2;

                case AssetType.Boat:
                    return CashFlow == 0 ? Price / 2 : (Price - Mortgage) / 2;

                default:
                    return 0;
            }
        }
    }

    public AssetType Type { get => Get("Type").ParseEnum<AssetType>(); set => Set("Type", (int)value); }
    public string Title { get => Get("Title"); set => Set("Title", value); }
    public int Price { get => GetInt("Price"); set => Set("Price", value); }
    public int SellPrice { get => GetInt("SellPrice"); set => Set("SellPrice", value); }
    public int Qtty { get => GetInt("Qtty"); set => Set("Qtty", value); }
    public int Mortgage { get => GetInt("Mortgage"); set => Set("Mortgage", value); }
    public int TotalCashFlow => Type == AssetType.Boat ? -CashFlow : Qtty * CashFlow;
    public int CashFlow { get => GetInt("CashFlow"); set => Set("CashFlow", value); }
    public bool BigCircle { get => GetInt("BigCircle") == 1; set => Set("BigCircle", value ? 1 : 0); }
    public bool IsDraft { get => GetInt("Draft") == 1; set => Set("Draft", value ? 1 : 0); }
    public bool IsDeleted { get => GetInt("Deleted") == 1; set => Set("Deleted", value ? 1 : 0); }

    public void Sell(ActionType action, int price)
    {
        SellPrice = price;
        Delete();
        User.History_OBSOLETE.Add(action, Id);
    }

    public void Restore()
    {
        IsDeleted = false;
        Title = Title.SubStringTo("*");
    }

    public void Delete()
    {
        IsDeleted = true;
        Title = Title.SubStringTo("*");
    }
}
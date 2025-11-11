using CashFlow.Data.Consts;
using CashFlow.Data.DataBase;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;

namespace CashFlow.Data.Users.UserData.PersonData;

public interface IAssetManager
{
    void Create(AssetDto asset);
    void Write(AssetDto assetDto);
    AssetDto Read(long id);
    AssetDto Read(AssetType type);
    string GetDescription(AssetDto asset);
    int GetBancrupcySellPrice(AssetDto asset);
    void Sell(AssetDto asset, ActionType action, int price);
    void Restore(AssetDto asset);
    void Delete(AssetDto asset);
}

public class AssetManager(IDataBase dataBase, ITermsService Terms, IUser user) : IAssetManager
{
    public void Create(AssetDto asset)
    {
        throw new NotImplementedException();
    }

    public void Write(AssetDto asset)
    {
        var sql = $"" +
            $"UPDATE Assets SET " +
            $"Type = {(int)asset.Type}," +
            $"Title = '{asset.Title}'," +
            $"Price = {asset.Price}," +
            $"SellPrice = {asset.SellPrice}," +
            $"Qtty = {asset.Qtty}," +
            $"Mortgage = {asset.Mortgage}," +
            $"CashFlow = {asset.CashFlow}," +
            $"BigCircle = {asset.BigCircle}," +
            $"CashFlow = {asset.CashFlow}," +
            $"IsDraft = {(asset.IsDraft ? 1 : 0)}," +
            $"IsDeleted = {(asset.IsDeleted ? 1 : 0)}," +
            $"WHERE AssetID = {asset.Id} AND UserID = {user.Id}";

        dataBase.Execute(sql);
    }

    public AssetDto Read(long id)
    {
        var sql = $"SELECT * FROM Assets WHERE AssetID = {id} AND UserID = {user.Id}";
        var data = dataBase.GetRow(sql);

        return new AssetDto
        {
            Type = data["Type"].ParseEnum<AssetType>(),
            Title = data["Title"],
            Price = data["Price"].ToInt(),
            SellPrice = data["SellPrice"].ToInt(),
            Qtty = data["Qtty"].ToInt(),
            Mortgage = data["Mortgage"].ToInt(),
            TotalCashFlow = data["TotalCashFlow"].ToInt(),
            CashFlow = data["CashFlow"].ToInt(),
            BigCircle = data["BigCircle"].ToInt() == 1,
            IsDraft = data["IsDraft"].ToInt() == 1,
            IsDeleted = data["IsDeleted"].ToInt() == 1,
        };
    }

    public AssetDto Read(AssetType type)
    {
        var id = dataBase.GetValue($"SELECT * FROM Assets WHERE Type = {type} AND UserID = {user.Id}").ToLong();
        return Read(id);
    }

    public string GetDescription(AssetDto asset)
    {
        var mortgage = Terms.Get(43, user, "Mortgage");
        var price = Terms.Get(64, user, "Price");
        var cashFlow = Terms.Get(55, user, "Cash Flow");

        return asset.Type switch
        {
            AssetType.Stock => asset.IsDeleted
                                ? $"*{asset.Title}* - {asset.Qtty} @ {asset.SellPrice.AsCurrency()}"
                            : asset.CashFlow == 0
                            ? $"*{asset.Title}* - {asset.Qtty} @ {asset.Price.AsCurrency()}"
                                    : $"*{asset.Title}* - {asset.Qtty} @ {asset.Price.AsCurrency()}, {cashFlow}: {asset.CashFlow.AsCurrency()} x {asset.Qtty} = {(asset.CashFlow * asset.Qtty).AsCurrency()}",

            AssetType.RealEstate => asset.IsDeleted
                            ? $"*{asset.Title}* - {price}: {asset.SellPrice.AsCurrency()}"
                            : $"*{asset.Title}* - {price}: {asset.Price.AsCurrency()}, {mortgage}: {asset.Mortgage.AsCurrency()}, {cashFlow}: {asset.CashFlow.AsCurrency()}",

            AssetType.LandTitle => asset.IsDeleted
                                ? $"*{asset.Title}* - {price}: {asset.SellPrice.AsCurrency()}"
                                : $"*{asset.Title}* - {price}: {asset.Price.AsCurrency()}",

            AssetType.Business => asset.IsDeleted
                                ? $"*{asset.Title}* - {price}: {asset.SellPrice.AsCurrency()}"
                            : asset.Mortgage > 0
                                    ? $"*{asset.Title}* - {price}: {asset.Price.AsCurrency()}, {mortgage}: {asset.Mortgage.AsCurrency()}, {cashFlow}: {asset.CashFlow.AsCurrency()}"
                                    : $"*{asset.Title}* - {price}: {asset.Price.AsCurrency()}, {cashFlow}: {asset.CashFlow.AsCurrency()}",

            AssetType.Boat => asset.CashFlow == 0
                            ? $"*{asset.Title}* - {price}: {asset.Price.AsCurrency()}"
                            : $"*{asset.Title}* - {price}: {asset.Price.AsCurrency()}, {Terms.Get(42, user, "monthly")}: {(-asset.CashFlow).AsCurrency()}",

            AssetType.SmallBusinessType => asset.CashFlow == 0
                            ? $"*{asset.Title}* - {price}: {asset.Price.AsCurrency()}"
                            : $"*{asset.Title}* - {price}: {asset.Price.AsCurrency()}, {Terms.Get(42, user, "monthly")}: {asset.CashFlow.AsCurrency()}",

            AssetType.Coin => asset.IsDeleted
                                ? $"*{asset.Title}* - {asset.Qtty} @ {asset.SellPrice.AsCurrency()}"
                                : $"*{asset.Title}* - {asset.Qtty} @ {asset.Price.AsCurrency()}",

            _ => string.Empty,
        };
    }

    public int GetBancrupcySellPrice(AssetDto asset)
    {
        return asset.Type switch
        {
            AssetType.Coin or AssetType.Stock => asset.Qtty * asset.Price / 2,
            AssetType.LandTitle or AssetType.SmallBusinessType => asset.Price / 2,
            AssetType.RealEstate or AssetType.Business => (asset.Price - asset.Mortgage) / 2,
            AssetType.Boat => asset.CashFlow == 0 ? asset.Price / 2 : (asset.Price - asset.Mortgage) / 2,
            _ => 0,
        };
    }

    public void Sell(AssetDto asset, ActionType action, int price)
    {
        asset.SellPrice = price;
        Delete(asset);
        user.History.Add(action, asset.Id);
    }

    public void Restore(AssetDto asset)
    {
        asset.IsDeleted = false;
        asset.Title = asset.Title.SubStringTo("*");
        Write(asset);
    }

    public void Delete(AssetDto asset)
    {
        asset.IsDeleted = true;
        asset.Title = asset.Title.SubStringTo("*");
        Write(asset);
    }
}
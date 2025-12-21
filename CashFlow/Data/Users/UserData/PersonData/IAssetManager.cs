using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Data.Users.UserData.PersonData;

public interface IAssetManager
{
    AssetDto Create(AssetDto asset);
    void Update(AssetDto asset);
    AssetDto Read(long id, long userId);
    List<AssetDto> ReadAll(AssetType type, long userId);
    string GetDescription(AssetDto asset, IUser user);
    int GetBancrupcySellPrice(AssetDto asset);
    void Sell(AssetDto asset, ActionType action, int price, IUser user);
    void Restore(AssetDto asset);
    void Delete(AssetDto asset);
    void DeleteAll(long userId);
}

public class AssetManager(IDataBase dataBase, ITermsService terms) : IAssetManager
{
    public AssetDto Create(AssetDto asset)
    {
        int newId = dataBase.GetValue("SELECT MAX(AssetID) FROM Assets").ToInt() + 1;
        var sql = $@"
            INSERT INTO Assets ( Id, UserId, Type, Title, Price, SellPrice, Qtty, Mortgage, CashFlow, BigCircle, IsDraft, IsDeleted)
            VALUES
            (
                {newId},
                {asset.UserId},
                {(int)asset.Type},
                '{asset.Title.Replace("'", "''")}',
                {asset.Price},
                {asset.SellPrice},
                {asset.Qtty},
                {asset.Mortgage},
                {asset.CashFlow},
                {asset.BigCircle},
                {(asset.IsDraft ? 1 : 0)},
                {(asset.IsDeleted ? 1 : 0)}
            );";
        dataBase.Execute(sql);

        return Read(newId, asset.UserId);
    }

    public void Update(AssetDto asset)
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
            $"WHERE AssetID = {asset.Id} AND UserID = {asset.UserId}";
        dataBase.Execute(sql);
    }

    public AssetDto Read(long id, long userId)
    {
        var sql = $"SELECT * FROM Assets WHERE AssetID = {id} AND UserID = {userId}";
        var data = dataBase.GetRow(sql);

        return new AssetDto
        {
            Type = data["Type"].ParseEnum<AssetType>(),
            Title = data["Title"],
            Id = data["Id"].ToInt(),
            UserId = data["UserId"].ToInt(),
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

    public List<AssetDto> ReadAll(AssetType type, long userId)
    {
        var ids = dataBase.GetColumn($"SELECT ID FROM Assets WHERE Type = {type} AND UserID = {userId}");
        return ids.Select(id => Read(id.ToLong(), userId)).ToList();
    }

    public string GetDescription(AssetDto asset, IUser user)
    {
        var mortgage = terms.Get(43, user, "Mortgage");
        var price = terms.Get(64, user, "Price");
        var cashFlow = terms.Get(55, user, "Cash Flow");

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
                            : $"*{asset.Title}* - {price}: {asset.Price.AsCurrency()}, {terms.Get(42, user, "monthly")}: {(-asset.CashFlow).AsCurrency()}",

            AssetType.SmallBusinessType => asset.CashFlow == 0
                            ? $"*{asset.Title}* - {price}: {asset.Price.AsCurrency()}"
                            : $"*{asset.Title}* - {price}: {asset.Price.AsCurrency()}, {terms.Get(42, user, "monthly")}: {asset.CashFlow.AsCurrency()}",

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

    public void Sell(AssetDto asset, ActionType action, int price, IUser user)
    {
        asset.SellPrice = price;
        Delete(asset);
        user.History_OBSOLETE.Add(action, asset.Id);
    }

    public void Restore(AssetDto asset)
    {
        asset.IsDeleted = false;
        asset.Title = asset.Title.SubStringTo("*");
        Update(asset);
    }

    public void Delete(AssetDto asset)
    {
        asset.IsDeleted = true;
        asset.Title = asset.Title.SubStringTo("*");
        Update(asset);
    }

    public void DeleteAll(long userId) => dataBase.Execute($"DELETE FROM Assets WHERE UserID = {userId}");
}
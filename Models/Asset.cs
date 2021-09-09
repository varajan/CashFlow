using CashFlowBot.Data;
using CashFlowBot.DataBase;
using CashFlowBot.Extensions;

namespace CashFlowBot.Models
{
    public class Asset
    {
        private long UserId { get; }
        private long Id { get; }
        private string Table { get; }

        private string Get(string column) => DB.GetValue($"SELECT {column} FROM {Table} WHERE AssetID = {Id} AND UserID = {UserId}");
        private int GetInt(string column) => Get(column).ToInt();
        private void Set(string column, int value) => DB.Execute($"UPDATE {Table} SET {column} = {value} WHERE AssetID = {Id} AND UserID = {UserId}");
        private void Set(string column, string value) => DB.Execute($"UPDATE {Table} SET {column} = '{value}' WHERE AssetID = {Id} AND UserID = {UserId}");

        public Asset(long userId, int id) => (UserId, Id, Table) = (userId, id, DB.Tables.Assets);

        public string Description => $"*{Title}* - {Qtty} @ ${Price}";

        public AssetType Type { get => Get("Type").ParseEnum<AssetType>(); set => Set("Type", (int)value);}
        public string Title { get => Get("Title"); set => Set("Title", value); }
        public int Price { get => GetInt("Price"); set => Set("Price", value); }
        public int Qtty { get => GetInt("Qtty"); set => Set("Qtty", value); }

        public int Cost { get => GetInt("Cost"); set => Set("Cost", value); }
        public int Mortgage { get => GetInt("Mortgage"); set => Set("Mortgage", value); }
        public int CashFlow { get => GetInt("CashFlow"); set => Set("CashFlow", value); }

        public void Delete() => DB.Execute($"DELETE FROM {Table} WHERE AssetID = {Id} AND UserID = {UserId}");
    }
}

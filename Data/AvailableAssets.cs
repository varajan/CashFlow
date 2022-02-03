using System.Collections.Generic;
using System.Linq;
using CashFlowBot.DataBase;
using CashFlowBot.Extensions;

namespace CashFlowBot.Data
{
    public class AvailableAssets
    {
        public static IEnumerable<string> Get(AssetType type) =>
            DB.GetColumn($"SELECT Value FROM AvailableAssets WHERE Type = {(int) type} ORDER BY CAST(Value as Number)").Distinct();

        public static IEnumerable<string> GetAsCurrency(AssetType type) => Get(type).ToInt().Distinct().OrderBy(x => x).AsCurrency();

        public static IEnumerable<string> GetAsText(AssetType type) => Get(type).Distinct().OrderBy(x => x);

        public static void Clear(AssetType type) =>
            DB.Execute($"DELETE FROM AvailableAssets WHERE Type = {(int) type}");

        public static void ClearAll() => DB.Execute("DROP TABLE AvailableAssets");
    }
}

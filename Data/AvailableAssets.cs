using System.Collections.Generic;
using System.Linq;
using CashFlowBot.DataBase;
using CashFlowBot.Extensions;

namespace CashFlowBot.Data
{
    public class AvailableAssets
    {
        public const string DefaultLanguage = "-";

        public static void Add(int value, AssetType type) => Add(value.ToString(), type, DefaultLanguage);
        public static void Add(string value, AssetType type, string language)
        {
            if (Get(type).Contains(value)) return;

            DB.Execute("INSERT INTO AvailableAssets (Type, Language, Value) " +
                       $"VALUES ({(int)type}, '{language}', '{value}')");
        }

        public static IEnumerable<string> Get(AssetType type, string language = DefaultLanguage)
        {
            var select = "SELECT Value FROM AvailableAssets WHERE Type = {0} AND Language = '{1}' ORDER BY CAST(Value as Number)";

            var assetsByLanguage = DB.GetColumn(string.Format(select, (int)type, language)).Distinct();
            var assetsDefault = DB.GetColumn(string.Format(select, (int)type, DefaultLanguage)).Distinct();

            return assetsByLanguage.Any() ? assetsByLanguage : assetsDefault;
        }

        public static IEnumerable<string> GetAsCurrency(AssetType type) =>
            Get(type).ToInt().Distinct().OrderBy(x => x).AsCurrency();

        public static IEnumerable<string> GetAsText(AssetType type, Language language) =>
            Get(type, language.ToString()).Distinct().OrderBy(x => x);

        public static void Clear(AssetType type) =>
            DB.Execute($"DELETE FROM AvailableAssets WHERE Type = {(int) type}");

        public static void ClearAll() => DB.Execute("DROP TABLE AvailableAssets");
    }
}

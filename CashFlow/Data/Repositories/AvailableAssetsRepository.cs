using CashFlow.Data.Consts;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Data.Repositories;

public class AvailableAssetsRepository(IDataBase dataBase) : IAvailableAssetsRepository
{
    private IDataBase DataBase { get; } = dataBase;

    public const string DefaultLanguage = "-";

    public IEnumerable<string> GetAsText(AssetType type, Language language) =>
        Get(type, language.ToString()).Distinct().OrderBy(x => x);

    public IEnumerable<string> GetAsCurrency(AssetType type) =>
        Get(type).ToInt().Distinct().OrderBy(x => x).AsCurrency();

    public IEnumerable<string> Get(AssetType type, string language = DefaultLanguage)
    {
        var select = "SELECT Value FROM AvailableAssets WHERE Type = {0} AND Language = '{1}' ORDER BY CAST(Value as Number)";

        var assetsByLanguage = DataBase.GetColumn(string.Format(select, (int)type, language)).Distinct();
        var assetsDefault = DataBase.GetColumn(string.Format(select, (int)type, DefaultLanguage)).Distinct();

        return assetsByLanguage.Any() ? assetsByLanguage : assetsDefault;
    }

    public void Add(int value, AssetType type) => Add(value.ToString(), type, DefaultLanguage);
    public void Add(string value, AssetType type, string language)
    {
        if (Get(type).Contains(value)) return;

        DataBase.Execute(@"
            INSERT INTO AvailableAssets (Type, Language, Value) " +
            $"VALUES ({(int)type}, '{language}', '{value}')");
    }
}

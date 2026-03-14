using CashFlow.Data.Consts;

namespace CashFlow.Interfaces;

public interface IAvailableAssetsRepository
{
    IEnumerable<string> GetAsText(AssetType type, Language language);
    IEnumerable<string> GetAsCurrency(AssetType type);
    void Add(int value, AssetType type);
}
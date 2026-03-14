using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;

namespace CashFlow.Interfaces;

public interface IPersonService
{
    bool Exists(ICashFlowUser user);
    void Create(string profession, ICashFlowUser user);
    void Update(PersonDto person);
    PersonDto Read(ICashFlowUser user);

    string GetDescription(ICashFlowUser user, bool compact = true);
    void Delete(ICashFlowUser user);
    void Update(ICashFlowUser user, LiabilityDto liability);

    void AddHistory(ActionType type, long value, ICashFlowUser user);
    void AddHistory(ActionType type, long value, ICashFlowUser user, long assetId);
    List<HistoryDto> ReadHistory(ICashFlowUser user);
    bool IsHistoryEmpty(ICashFlowUser user);
    string HistoryTopFive(ICashFlowUser user, ICashFlowUser currentUser);
    void RollbackHistory(PersonDto person, HistoryDto record);

    List<AssetDto> ReadAllAssets(AssetType type, ICashFlowUser user);
    void CreateAsset(ICashFlowUser user, AssetDto asset);
    void DeleteAsset(ICashFlowUser user, AssetDto asset);
    void UpdateAsset(ICashFlowUser user, AssetDto asset);
    void SellAsset(AssetDto asset, ActionType action, int price, ICashFlowUser user);
    string GetAssetDescription(AssetDto asset, ICashFlowUser user);
}

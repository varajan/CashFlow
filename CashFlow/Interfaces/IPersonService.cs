using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;

namespace CashFlow.Interfaces;

public interface IPersonService
{
    bool Exists(UserDto user);
    void Create(string profession, UserDto user);
    void Update(PersonDto person);
    PersonDto Read(UserDto user);

    List<string> GetAllProfessions();
    string GetDescription(UserDto user, bool compact = true);
    void Delete(UserDto user);
    void Update(UserDto user, LiabilityDto liability);

    void AddHistory(ActionType type, long value, UserDto user);
    void AddHistory(ActionType type, long value, UserDto user, long assetId);
    List<HistoryDto> ReadHistory(UserDto user);
    bool IsHistoryEmpty(UserDto user);
    string HistoryTopFive(UserDto user, UserDto currentUser);
    void RollbackHistory(PersonDto person, HistoryDto record);

    List<AssetDto> ReadAllAssets(AssetType type, UserDto user);
    void CreateAsset(UserDto user, AssetDto asset);
    void DeleteAsset(UserDto user, AssetDto asset);
    void UpdateAsset(UserDto user, AssetDto asset);
    void SellAsset(AssetDto asset, int price, UserDto user);
    string GetAssetDescription(AssetDto asset, UserDto user);
}

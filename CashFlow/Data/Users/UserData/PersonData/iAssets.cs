using CashFlow.Data.Consts;

namespace CashFlow.Data.Users.UserData.PersonData;

public interface IAssets
{
    int Income { get; }

    List<Asset_OLD> Stocks { get; }
    List<Asset_OLD> RealEstates  { get; }
    List<Asset_OLD> SmallBusinesses  { get; }
    List<Asset_OLD> Coins  { get; }
    List<Asset_OLD> Businesses { get; }
    List<Asset_OLD> Lands  { get; }
    Asset_OLD Boat { get; }
    //Asset Transfer { get; }

    List<Asset_OLD> Items { get; }

    string Description { get; }
    string BigCircleDescription { get; }

    void Clear();
    void CleanUp();
    Asset_OLD Add(string title, AssetType type, bool bigCircle = false);
    Asset_OLD Get(AssetType type);
}

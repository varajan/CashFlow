using System.Collections.Generic;
using CashFlow.Data.Consts;

namespace CashFlow.Data.Users.UserData.PersonData;

public interface IAssets
{
    int Income { get; }

    List<Asset> Stocks { get; }
    List<Asset> RealEstates  { get; }
    List<Asset> SmallBusinesses  { get; }
    List<Asset> Coins  { get; }
    List<Asset> Businesses { get; }
    List<Asset> Lands  { get; }
    Asset Boat { get; }
    Asset Transfer { get; }

    List<Asset> Items { get; }

    string Description { get; }
    string BigCircleDescription { get; }

    void Clear();
    void CleanUp();
    Asset Add(string title, AssetType type, bool bigCircle = false);
}

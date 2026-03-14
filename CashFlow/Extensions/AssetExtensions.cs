using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;

namespace CashFlow.Extensions;

public static class AssetExtensions
{
    public static int GetBancrupcySellPrice(this AssetDto asset)
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
}

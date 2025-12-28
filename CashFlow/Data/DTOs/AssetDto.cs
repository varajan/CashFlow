using CashFlow.Data.Consts;

namespace CashFlow.Data.DTOs;

public class AssetDto
{
    public long UserId { get; set; }
    public long Id { get; set; }
    public AssetType Type { get; set; }
    public string Title { get; set; }
    public int Price { get; set; }
    public int SellPrice { get; set; }
    public int Qtty { get; set; }
    public int Mortgage { get; set; }
    public int TotalCashFlow { get; set; }
    public int CashFlow { get; set; }
    public bool BigCircle { get; set; }
    public bool IsDraft { get; set; }
    public bool MarkedToSell { get; set; }
    public bool IsDeleted { get; set; }

    public int BancrupcySellPrice
    {
        get
        {
            switch (Type)
            {
                case AssetType.Coin:
                case AssetType.Stock:
                    return Qtty * Price / 2;

                case AssetType.LandTitle:
                case AssetType.SmallBusinessType:
                    return Price / 2;

                case AssetType.RealEstate:
                case AssetType.Business:
                    return (Price - Mortgage) / 2;

                case AssetType.Boat:
                    return CashFlow == 0 ? Price / 2 : (Price - Mortgage) / 2;

                default:
                    return 0;
            }
        }
    }
}

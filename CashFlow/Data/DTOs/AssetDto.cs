using CashFlow.Data.Consts;

namespace CashFlow.Data.DTOs;

public class AssetDto
{
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
    public bool IsDeleted { get; set; }
}

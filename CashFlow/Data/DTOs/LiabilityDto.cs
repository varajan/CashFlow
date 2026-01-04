using CashFlow.Data.Consts;

namespace CashFlow.Data.DTOs;

public class LiabilityDto
{
    public bool IsBankruptcyDivisible { get; set; }
    public bool AllowsPartialPayment { get; set; }
    public bool MarkedForReduction { get; set; }
    public bool Deleted { get; set; }
    public int FullAmount { get; set; }
    public int Cashflow { get; set; }
    public Liability Name { get; set; }
}
namespace CashFlow.Data.DTOs;

public class PersonDto
{
    public long Id { get; set; }
    public string Profession { get; set; }
    public int Salary { get; set; }
    public int Cash { get; set; }
    //public bool SmallRealEstate { get; set; }
    public bool ReadyForBigCircle { get; set; }
    //public Circle Circle { get; set; } // { get => BigCircle ? Circle.Big : Circle.Small; set => throw new NotImplementedException(); }
    public bool BigCircle { get; set; }
    public int InitialCashFlow { get; set; }
    public bool Bankruptcy { get; set; }
    public bool CreditsReduced { get; set; }

    public int CashFlow { get; set; }

    public List<AssetDto> Assets { get; set; } = [];
    public ExpensesDto Expenses { get; set; } = new();
    public LiabilitiesDto Liabilities { get; set; } = new();
}

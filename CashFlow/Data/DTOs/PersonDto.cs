namespace CashFlow.Data.DTOs;

public class PersonDto
{
    public long Id { get; set; }
    public DateTime LastActive { get; set; }
    public string Profession { get; set; }
    public int Salary { get; set; }
    public int Cash { get; set; }
    public bool BigCircle { get; set; }
    public bool IsWinning { get; set; }
    public int InitialCashFlow { get; set; }
    public bool Bankruptcy { get; set; }
    public bool CreditsReduced { get; set; }

    public int PerChild { get; set; }
    public int Children { get; set; }

    public int TargetCashFlow { get; set; }

    public List<AssetDto> Assets { get; set; } = [];
    public List<LiabilityDto> Liabilities { get; set; } = [];
}

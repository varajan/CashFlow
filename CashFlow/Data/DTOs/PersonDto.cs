using CashFlow.Data.Users.UserData.PersonData;

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
    public bool IsWinning { get; set; }
    public int InitialCashFlow { get; set; }
    public bool Bankruptcy { get; set; }
    public bool CreditsReduced { get; set; }

    public int CashFlow => Salary + Assets.Sum(a => a.CashFlow) + Liabilities.Sum(l => l.Cashflow);
    public int CurrentCashFlow { get; set; }
    public int TargetCashFlow { get; set; }

    public List<AssetDto> Assets { get; set; } = [];
    public ExpensesDto Expenses { get; set; } = new();
    public LiabilitiesDto Liabilities_OBSOLETE { get; set; } = new();
    public List<LiabilityDto> Liabilities { get; set; } = [];

    public void GetCredit(int amount)
    {
        Cash += amount;
        UpdateLiability("Bank Loan", -amount / 10, amount);
    }

    private void UpdateLiability(string name, int cashFlow, int ammount)
    {
        var idx = Liabilities.FindIndex(l => l.Name == name);

        if (idx >= 0)
        {
            var liability = Liabilities[idx];
            liability.Cashflow += cashFlow;
            liability.FullAmount += ammount;
            Liabilities[idx] = liability;
        }
        else
        {
            var liability = new LiabilityDto { Name = name, Cashflow = cashFlow, FullAmount = ammount };
            Liabilities.Add(liability);
        }
    }
}

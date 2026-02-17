using CashFlow.Data.Consts;

namespace CashFlow.Data.DTOs;

public class PersonDto
{
    public long Id { get; set; }
    public string Profession { get; set; }
    public int Salary { get; set; }
    public int Cash { get; set; }
    public bool ReadyForBigCircle => Income > TotalExpenses;
    public bool BigCircle { get; set; }
    public bool IsWinning { get; set; }
    public int InitialCashFlow { get; set; }
    public bool Bankruptcy { get; set; }
    public bool CreditsReduced { get; set; }

    public int PerChild { get; set; }
    public int Children { get; set; }

    public int BoatPayment => Assets.Where(a => !a.IsDeleted).FirstOrDefault(a => a.Type == AssetType.Boat)?.CashFlow ?? 0;
    public int Income => Assets.Where(a => !a.IsDeleted).Sum(a => a.Qtty * a.CashFlow) - BoatPayment;
    public int CashFlow => Salary + Income + TotalExpenses;
    public int TotalExpenses => Liabilities.Sum(l => l.Cashflow) - Children * PerChild;

    public int CurrentCashFlow { get; set; }
    public int TargetCashFlow { get; set; }

    public List<AssetDto> Assets { get; set; } = [];
    public List<LiabilityDto> Liabilities { get; set; } = [];

    public void GetCredit(int amount)
    {
        Cash += amount;
        UpdateLiability(Liability.Bank_Loan, -amount / 10, amount);
    }

    public void DeleteLiability(Liability type)
    {
        Liabilities = [.. Liabilities.Where(l => l.Type != type)];
    }

    public void UpdateLiability(Liability type, int cashFlow, int ammount)
    {
        var idx = Liabilities.FindIndex(l => l.Type == type);

        if (idx >= 0)
        {
            var liability = Liabilities[idx];
            liability.Cashflow += cashFlow;
            liability.FullAmount += ammount;
            liability.Deleted = false;
            Liabilities[idx] = liability;
        }
        else
        {
            var liability = new LiabilityDto { Type = type, Cashflow = cashFlow, FullAmount = ammount };
            Liabilities.Add(liability);
        }
    }
}

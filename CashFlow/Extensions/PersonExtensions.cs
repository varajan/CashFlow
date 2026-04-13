using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;

namespace CashFlow.Extensions;

public static class PersonExtensions
{
    public static bool IsReadyForBigCircle(this PersonDto person) => person.GetIncome() > Math.Abs(person.GetTotalExpenses());
    public static int GetBoatPayment(this PersonDto person) => person.Assets.Where(a => !a.IsDeleted).FirstOrDefault(a => a.Type == AssetType.Boat)?.CashFlow ?? 0;
    public static int GetIncome(this PersonDto person) => person.Assets.Where(a => !a.IsDeleted).Sum(a => a.Qtty * a.CashFlow) - person.GetBoatPayment();
    public static int GetTotalExpenses(this PersonDto person) => person.Liabilities.Sum(l => l.Cashflow) - (person.Children * person.PerChild);
    public static int GetSmallCircleCashflow(this PersonDto person) => person.Salary + person.GetIncome() + person.GetTotalExpenses();
    public static int GetBigCircleCashflow(this PersonDto person) => person.InitialCashFlow + person.Assets.Where(a => a.BigCircle && !a.IsDeleted).Sum(a => a.CashFlow);

    public static void GetCredit(this PersonDto person, int amount)
    {
        person.Cash += amount;
        person.UpdateLiability(Liability.BankLoan, -amount / 10, amount);
    }

    public static void DeleteLiability(this PersonDto person, Liability type)
    {
        person.Liabilities = [.. person.Liabilities.Where(l => l.Type != type)];
    }

    public static void UpdateLiability(this PersonDto person, Liability type, int cashFlow, int amount)
    {
        var idx = person.Liabilities.FindIndex(l => l.Type == type);

        if (idx >= 0)
        {
            var liability = person.Liabilities[idx];
            liability.Cashflow += cashFlow;
            liability.FullAmount += amount;
            liability.Deleted = false;
            person.Liabilities[idx] = liability;
        }
        else
        {
            var liability = new LiabilityDto { Type = type, Cashflow = cashFlow, FullAmount = amount };
            person.Liabilities.Add(liability);
        }
    }

    public static void UpdateLiability(this PersonDto person, LiabilityDto liability)
    {
        var idx = person.Liabilities.FindIndex(l => l.Type == liability.Type);

        if (idx >= 0)
        {
            person.Liabilities[idx] = liability;
        }
        else
        {
            person.Liabilities.Add(liability);
        }
    }

    public static LiabilityDto GetLiability(this PersonDto person, Liability type) => person.Liabilities.FirstOrDefault(l => l.Type == type);
}

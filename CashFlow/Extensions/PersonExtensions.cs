using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;

namespace CashFlow.Extensions;

public static class PersonExtensions
{
    public static bool IsReadyForBigCircle(this PersonDto person) => person.GetIncome() > Math.Abs(person.GetTotalExpenses());
    public static int GetBoatPayment(this PersonDto person) => person.Assets.Where(a => !a.IsDeleted).FirstOrDefault(a => a.Type == AssetType.Boat)?.CashFlow ?? 0;
    public static int GetIncome(this PersonDto person) => person.Assets.Where(a => !a.IsDeleted).Sum(a => a.Qtty * a.CashFlow) - person.GetBoatPayment();
    public static int GetTotalExpenses(this PersonDto person) => person.Liabilities.Sum(l => l.Cashflow) - person.Children * person.PerChild;
    public static int GetSmallCircleCashflow(this PersonDto person) => person.Salary + person.GetIncome() + person.GetTotalExpenses();
    public static int GetBigCircleCashflow(this PersonDto person) => person.InitialCashFlow + person.Assets.Where(a => a.BigCircle && !a.IsDeleted).Sum(a => a.CashFlow);
}

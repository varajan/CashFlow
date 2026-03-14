using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;

namespace CashFlow.Data.Users.UserData.PersonData;

public static class PersonExtensions
{
    public static bool IsReadyForBigCircle(this PersonDto person) => GetIncome(person) > Math.Abs(GetTotalExpenses(person));
    public static int GetBoatPayment(this PersonDto person) => person.Assets.Where(a => !a.IsDeleted).FirstOrDefault(a => a.Type == AssetType.Boat)?.CashFlow ?? 0;
    public static int GetIncome(this PersonDto person) => person.Assets.Where(a => !a.IsDeleted).Sum(a => a.Qtty * a.CashFlow) - GetBoatPayment(person);
    public static int GetTotalExpenses(this PersonDto person) => person.Liabilities.Sum(l => l.Cashflow) - person.Children * person.PerChild;
    public static int GetSmallCircleCashflow(this PersonDto person) => person.Salary + GetIncome(person) + GetTotalExpenses(person);
    public static int GetBigCircleCashflow(this PersonDto person) => person.InitialCashFlow + person.Assets.Where(a => a.BigCircle && !a.IsDeleted).Sum(a => a.CashFlow);
}

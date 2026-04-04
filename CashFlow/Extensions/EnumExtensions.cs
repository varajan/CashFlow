using CashFlow.Data.Consts;
using System.ComponentModel;

namespace CashFlow.Extensions;

public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        var info = value.GetType().GetField(value.ToString());
        var attributes = (DescriptionAttribute[])info.GetCustomAttributes(typeof(DescriptionAttribute), false);

        if (attributes != null && attributes.Length > 0)
            return attributes[0].Description;
        else
            return value.ToString();
    }

    public static ActionType AsActionType(this Liability liability) => liability switch
        {
            Liability.Mortgage => ActionType.Mortgage,
            Liability.School_Loan => ActionType.SchoolLoan,
            Liability.Car_Loan => ActionType.CarLoan,
            Liability.Credit_Card => ActionType.CreditCard,
            Liability.Small_Credit => ActionType.SmallCredit,
            Liability.Bank_Loan => ActionType.BankLoan,
            Liability.Boat_Loan => ActionType.PayOffBoat,
            _ => throw new InvalidOperationException($"Unknown liability type: {liability}")
        };
}
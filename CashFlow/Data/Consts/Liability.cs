using System.ComponentModel;

namespace CashFlow.Data.Consts;

public enum Liability
{
    [Description("Taxes")]
    Taxes,

    [Description("Other Payments")]
    OtherPayments,

    [Description("Mortgage")]
    Mortgage,

    [Description("School Loan")]
    SchoolLoan,

    [Description("Car Loan")]
    CarLoan,

    [Description("Credit Card")]
    CreditCard,

    [Description("Small Credit")]
    SmallCredit,

    [Description("Bank Loan")]
    BankLoan,

    [Description("Boat Loan")]
    BoatLoan
}

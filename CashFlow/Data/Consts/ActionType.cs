using System.ComponentModel;

namespace CashFlow.Data.Consts;

public enum ActionType
{
    [Description("Pay with Cash")]
    PayMoney,

    [Description("Get Money")]
    GetMoney,

    [Description("Get a child")]
    Child,

    Downsize,

    [Description("Get Credit")]
    Credit,

    [Description("Reduce Liabilities")]
    ReduceLiability,

    [Description("Charity - Pay 10%")]
    Charity,

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

    [Description("Buy Real Estate")]
    BuyRealEstate,

    [Description("Buy Business")]
    BuyBusiness,

    [Description("Buy Land")]
    BuyLand,

    [Description("Buy Stocks")]
    BuyStocks,

    [Description("Sell Real Estate")]
    SellRealEstate,

    [Description("Sell Business")]
    SellBusiness,

    [Description("Sell Land")]
    SellLand,

    [Description("Sell Stocks")]
    SellStocks,

    [Description("Stocks x2")]
    Stocks1To2,

    [Description("Stocks ÷2")]
    Stocks2To1,

    [Description("Pay with Credit Card")]
    MicroCredit,

    [Description("Buy a boat")]
    BuyBoat,

    [Description("Boat Loan")]
    PayOffBoat,

    [Description("Start a company")]
    StartCompany,

    [Description("Increase cashflow")]
    IncreaseCashFlow,

    [Description("Buy Coins")]
    BuyCoins,

    [Description("Sell Coins")]
    SellCoins,

    Bankruptcy,

    [Description("Sale for debts")]
    BankruptcySellAsset,

    [Description("Debt restructuring")]
    BankruptcyDebtRestructuring,

    [Description("Credit repayment")]
    BankruptcyBankLoan,

    [Description("Go to Big Circle")]
    GoToBigCircle,

    Divorce,

    [Description("Tax Audit")]
    TaxAudit,

    Lawsuit,
}
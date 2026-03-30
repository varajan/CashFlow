using System.ComponentModel;

namespace CashFlow.Data.Consts;

public enum ActionType
{
    PayMoney = -20,
    GetMoney,
    Child,
    Downsize,
    Credit,
    ReduceLiability,
    Charity,
    Mortgage = 43,

    [Description("School Loan")]
    SchoolLoan = 44,

    [Description("Car Loan")]
    CarLoan = 45,

    [Description("Credit Card")]
    CreditCard = 46,

    [Description("Small Credit")]
    SmallCredit = 92,

    [Description("Bank Loan")]
    BankLoan = 47,

    [Description("Buy Real Estate")]
    BuyRealEstate = 37,

    [Description("Buy Business")]
    BuyBusiness = 74,

    [Description("Buy Land")]
    BuyLand = 94,

    [Description("Buy Stocks")]
    BuyStocks = 35,

    [Description("Sell Real Estate")]
    SellRealEstate = 38,

    [Description("Sell Business")]
    SellBusiness = 75,

    [Description("Sell Land")]
    SellLand = 98,

    [Description("Sell Stocks")]
    SellStocks = 36,

    [Description("Stocks x2")]
    Stocks1To2 = 82,

    [Description("Stocks ÷2")]
    Stocks2To1 = 83,

    MicroCredit,

    [Description("Buy a boat")]
    BuyBoat = 112,

    [Description("Boat Loan")]
    PayOffBoat = 114,

    [Description("Start a company")]
    StartCompany = 115,

    [Description("Increase cashflow")]
    IncreaseCashFlow = 118,

    [Description("Buy Coins")]
    BuyCoins = 119,

    [Description("Sell Coins")]
    SellCoins = 120,

    Bankruptcy = 125,

    [Description("Sale for debts")]
    BankruptcySellAsset = 131,

    [Description("Debt restructuring")]
    BankruptcyDebtRestructuring = 132,

    [Description("Credit repayment")]
    BankruptcyBankLoan = 135,

    [Description("Go to Big Circle")]
    GoToBigCircle = 1,

    Divorce = 69,

    [Description("Tax Audit")]
    TaxAudit = 70,

    Lawsuit = 71,
}
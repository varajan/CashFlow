﻿namespace CashFlowBot.Data
{
    public enum Stage
    {
        Admin,
        AdminBringDown,
        AdminLogs,
        AdminAvailableAssets,
        AdminAvailableAssetsClear,
        Nothing,
        StopGame,
        GetProfession,
        GetChild,
        GetMoney,
        GiveMoney,
        GetCredit,
        MicroCreditAmount,
        MicroCreditMonthly,
        PayCredit,
        ReduceMortgage = 43,
        ReduceSchoolLoan = 44,
        ReduceCarLoan = 45,
        ReduceCreditCard = 46,
        ReduceSmallCredit = 92,
        ReduceBankLoan,
        ReduceBoatLoan = 114,
        Stocks2to1,
        Stocks1to2,
        BuyStocksTitle,
        BuyStocksQtty,
        BuyStocksPrice,
        StartCompanyTitle,
        StartCompanyPrice,
        StartCompanyCredit,
        BuyRealEstateTitle,
        BuyRealEstatePrice,
        BuyRealEstateFirstPayment,
        BuyRealEstateCredit,
        BuyRealEstateCashFlow,
        BuyBusinessTitle,
        BuyBusinessPrice,
        BuyBusinessFirstPayment,
        BuyBusinessCashFlow,
        BuyBusinessCredit,
        BuyLandTitle,
        BuyLandPrice,
        BuyLandCredit,
        Sell,
        SellStocksTitle,
        SellStocksPrice,
        SellRealEstateTitle,
        SellRealEstatePrice,
        SellBusinessTitle,
        SellBusinessPrice,
        SellLandTitle,
        SellLandPrice,
        Rollback,
    }
}

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
        ReduceMortgage,
        ReduceSchoolLoan,
        ReduceCarLoan,
        ReduceCreditCard,
        ReduceSmallCredit,
        ReduceBankLoan,
        Stocks2to1,
        Stocks1to2,
        BuyStocksTitle,
        BuyStocksQtty,
        BuyStocksPrice,
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

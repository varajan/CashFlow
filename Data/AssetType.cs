namespace CashFlowBot.Data;

public enum AssetType
{
    Stock = 1,
    StockPrice = 2,
    StockCashFlow = 35,
    SmallGiveMoney = 3,
    BigGiveMoney = 4,

    LandTitle = 5,
    LandBuyPrice = 6,
    LandSellPrice = 7,

    MicroCreditAmount = 8,
    Business = 9,

    SmallBusinessType = 28,
    SmallBusinessBuyPrice = 29,
    IncreaseCashFlow = 36,

    BusinessType = 10,
    BusinessBuyPrice = 11,
    BusinessFirstPayment = 12,
    BusinessCashFlow = 13,
    BusinessSellPrice = 16,

    BigBusinessType = 14,
    BigBusinessBuyPrice  = 15,
    BigBusinessCashFlow  = 17,

    RealEstate = 17,
    RealEstateSmallType = 18,
    RealEstateSmallBuyPrice = 19,
    RealEstateSellPrice = 20,
    RealEstateSmallFirstPayment = 21,
    RealEstateSmallCashFlow = 22,
    RealEstateBigType = 23,
    RealEstateBigBuyPrice = 24,
    RealEstateBigFirstPayment = 25,
    RealEstateBigCashFlow = 26,

    Boat = 27,
    Coin = 30,
    CoinTitle = 31,
    CoinBuyPrice = 32,
    CoinSellPrice = 33,
    CoinCount = 34,

    Transfer = 144,
}
namespace CashFlow.Data.Consts;

public class Prices
{
    public static readonly int[] SmallGiveMoney = [10, 20, 40, 50, 70, 80, 100, 120, 150, 180, 200, 220, 250, 300, 350, 450, 500, 600, 700, 1500, 2000, 4000];
    public static readonly int[] BigGiveMoney = [25_000, 50_000, 100_000, 200_000];
    public static readonly int[] MicroCreditAmount = [1000, 2000, 3000, 4000, 5000, 6000, 7000, 8000, 9000, 10000];

    public static readonly int[] CoinCount = [1, 10];
    public static readonly int[] DreamPrice = [250000, 400000];
}

public class BuyPrice
{
    public static readonly int[] Business = [20000, 25000, 30000, 50000, 100000, 125000, 150000, 200000, 350000, 500000];
    public static readonly int[] BusinessSmall = [3000, 5000];
    public static readonly int[] BusinessBig = [100000, 120000, 150000, 200000, 225000, 250000, 300000];
    public static readonly int[] RealEstateSmall = [35000, 40000, 45000, 50000, 55000, 60000, 65000];
    public static readonly int[] RealEstateBig = [45000, 50000, 60000, 65000, 67000, 70000, 75000, 80000, 90000, 100000, 115000, 125000, 140000, 150000, 160000, 200000, 220000, 240000, 350000, 550000, 575000, 1200000];
    public static readonly int[] Stock = [1, 5, 10, 20, 30, 40, 50, 1200, 4000, 5000];
    public static readonly int[] Land = [5000, 10000, 20000];
    public static readonly int[] Coin = [300, 500];
}

public class SellPrice
{
    public static readonly int[] Business = [50000, 100000, 250000];
    public static readonly int[] RealEstate = [25000, 30000, 35000, 40000, 45000, 55000, 65000, 90000, 100000, 110000, 135000, 250000];
    public static readonly int[] Land = [150000, 200000];
    public static readonly int[] Coin = [600, 3000, 5000];
}

public class FirstPayment
{
    public static readonly int[] Business = [20000, 25000, 30000, 40000, 50000, 100000];
    public static readonly int[] RealEstateSmall = [0, 2000, 3000, 4000, 5000];
    public static readonly int[] RealEstateBig = [6000, 7000, 8000, 9000, 10000, 12000, 15000, 16000, 20000, 30000, 32000, 40000, 50000, 75000, 200000];
}

public class Cashflow
{
    public static readonly int[] SmallBusinessIncrease = [250, 400];
    public static readonly int[] Business = [800, 1000, 1500, 1600, 1800, 2500, 2700, 5000];
    public static readonly int[] BusinessAtBigCircle = [1000, 1500, 2000, 2500, 3000, 4000, 5000, 6000, 7500, 8000, 9500, 10000, 16000, 17000, 20000, 24000, 28000, 34000, 110000];
    public static readonly int[] RealEstateSmall = [-100, 100, 140, 160, 200, 220, 250];
    public static readonly int[] RealEstateBig = [-100, 140, 150, 240, 300, 320, 400, 500, 600, 750, 800, 950, 1000, 1600, 1700, 2000, 2400, 2800, 3400, 11000];
    public static readonly int[] Stock = [10, 20];
}
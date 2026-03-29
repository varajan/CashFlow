using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Data.Services;

public class DescriptionService(ITranslationService terms, AssetService assetService)
{
    private ITranslationService Terms { get; } = terms;
    private AssetService AssetService { get; } = assetService;

    public string GetDescription(UserDto user, PersonDto person, bool compact = true)
    {
        var description = person.BigCircle
            ? BigCircleDescription(person, user)
            : SmallCircleDescription(person, user);

        if (!compact)
        {
            description += AssetsDescription(person.Assets, user);
            description += ExpensesDescription(person, user);
        }

        return description.Trim();
    }

    private string AssetsDescription(List<AssetDto> personAssets, UserDto user)
    {
        if (!personAssets.Any(a => !a.IsDeleted))
            return string.Empty;

        var assetsTerm = Terms.Get("Assets", user);
        var assets = $"{Environment.NewLine}{Environment.NewLine}*{assetsTerm}:*{Environment.NewLine}";
        assets += string.Join(Environment.NewLine, personAssets
            .Where(a => !a.IsDeleted)
            .OrderBy(a => a.Type)
            .Select(a => $"• {GetAssetDescription(a, user)}"));

        return assets;
    }

    private string ExpensesDescription(PersonDto person, UserDto user)
    {
        var expensesTerm = Terms.Get("Expenses", user);
        var taxesTerm = Terms.Get("Taxes", user);
        var mortgageTerm = Terms.Get("Mortgage/Rent Pay", user);
        var schoolLoanTerm = Terms.Get("School Loan", user);
        var carLoanTerm = Terms.Get("Car Loan", user);
        var creditCardTerm = Terms.Get("Credit Card", user);
        var smallCreditsTerm = Terms.Get("Small Credit", user);
        var bankLoanTerm = Terms.Get("Bank Loan", user);
        var boatLoanTerm = Terms.Get("Boat Loan", user);
        var otherPaymentTerm = Terms.Get("Other Payments", user);
        var childrenTerm = Terms.Get("Children", user);
        var childrenExpensesTerm = Terms.Get("Children Expenses", user);
        var perChildTerm = Terms.Get("per child", user);

        var mortgage = GetLiability(person, Liability.Mortgage);
        var schoolLoan = GetLiability(person, Liability.School_Loan);
        var carLoan = GetLiability(person, Liability.Car_Loan);
        var creditCard = GetLiability(person, Liability.Credit_Card);
        var smallCredits = GetLiability(person, Liability.Small_Credit);
        var bankLoan = GetLiability(person, Liability.Bank_Loan);
        var boatLoan = GetLiability(person, Liability.Boat_Loan);
        var taxes = GetLiability(person, Liability.Taxes);
        var others = GetLiability(person, Liability.Others);
        var childrenExpenses = person.Children * person.PerChild;
        var children = person.Children;

        var expenses = $"{Environment.NewLine}{Environment.NewLine}*{expensesTerm}:*{Environment.NewLine}";
        expenses += $"*{taxesTerm}:* {taxes.AsCurrency()}{Environment.NewLine}";

        if (mortgage > 0) expenses += $"*{mortgageTerm}:* {mortgage.AsCurrency()}{Environment.NewLine}";
        if (schoolLoan > 0) expenses += $"*{schoolLoanTerm}:* {schoolLoan.AsCurrency()}{Environment.NewLine}";
        if (carLoan > 0) expenses += $"*{carLoanTerm}:* {carLoan.AsCurrency()}{Environment.NewLine}";
        if (creditCard > 0) expenses += $"*{creditCardTerm}:* {creditCard.AsCurrency()}{Environment.NewLine}";
        if (smallCredits > 0) expenses += $"*{smallCreditsTerm}:* {smallCredits.AsCurrency()}{Environment.NewLine}";
        if (bankLoan > 0) expenses += $"*{bankLoanTerm}:* {bankLoan.AsCurrency()}{Environment.NewLine}";
        if (boatLoan > 0) expenses += $"*{boatLoanTerm}:* {boatLoan.AsCurrency()}{Environment.NewLine}";
        expenses += $"*{otherPaymentTerm}:* {others.AsCurrency()}{Environment.NewLine}";
        if (childrenExpenses > 0) expenses += $"*{childrenTerm}:* {children} ({person.PerChild.AsCurrency()} {perChildTerm}){Environment.NewLine}";
        if (childrenExpenses > 0) expenses += $"*{childrenExpensesTerm}:* {childrenExpenses.AsCurrency()}{Environment.NewLine}";

        return expenses;
    }

    private static int GetLiability(PersonDto person, Liability name)
    {
        var liability = person.Liabilities.FirstOrDefault(l => l.Type == name)?.Cashflow ?? 0;
        return Math.Abs(liability);
    }

    private string SmallCircleDescription(PersonDto person, UserDto user)
    {
        var professionTerm = Terms.Get("Profession", user);
        var cashTerm = Terms.Get("Cash", user);
        var salaryTerm = Terms.Get("Salary", user);
        var incomeTerm = Terms.Get("Income", user);
        var expensesTerm = Terms.Get("Expenses", user);
        var cashFlowTerm = Terms.Get("Cashflow", user);
        var personProfession = Terms.Get(person.Profession, user.Language);

        return
            $"*{professionTerm}:* {personProfession}{Environment.NewLine}" +
            $"*{cashTerm}:* {person.Cash.AsCurrency()}{Environment.NewLine}" +
            $"*{salaryTerm}:* {person.Salary.AsCurrency()}{Environment.NewLine}" +
            $"*{incomeTerm}:* {person.GetIncome().AsCurrency()}{Environment.NewLine}" +
            $"*{expensesTerm}:* {(-person.GetTotalExpenses()).AsCurrency()}{Environment.NewLine}" +
            $"*{cashFlowTerm}:* {person.GetSmallCircleCashflow().AsCurrency()}";
    }

    private string BigCircleDescription(PersonDto person, UserDto user)
    {
        var professionTerm = Terms.Get("Profession", user);
        var cashTerm = Terms.Get("Cash", user);
        var cashFlowTerm = Terms.Get("Cashflow", user);
        var initialTerm = Terms.Get("Initial", user);
        var currentTerm = Terms.Get("Current", user);
        var targetTerm = Terms.Get("Target", user);
        var assets = person.Assets.Where(a => a.BigCircle).ToList();

        var description =
            $"*{professionTerm}:* {person.Profession}{Environment.NewLine}" +
            $"*{cashTerm}:* {person.Cash.AsCurrency()}{Environment.NewLine}" +
            $"{initialTerm} {cashFlowTerm}: {person.InitialCashFlow.AsCurrency()}{Environment.NewLine}" +
            $"{currentTerm} {cashFlowTerm}: {person.GetBigCircleCashflow().AsCurrency()}{Environment.NewLine}" +
            $"{targetTerm} {cashFlowTerm}: {person.TargetCashFlow.AsCurrency()}{Environment.NewLine}";
        description += AssetsDescription(assets, user);

        return description.Trim();
    }

    public string GetDescription(ActionType Action, long value, UserDto user, long assetId)
    {
        switch (Action)
        {
            case ActionType.PayMoney:
                return Terms.Get("Pay {0}", user, value.AsCurrency());

            case ActionType.GetMoney:
                return Terms.Get("Get {0}", user, value.AsCurrency());

            case ActionType.Child:
                return Terms.Get("Get a child", user);

            case ActionType.Downsize:
                return Terms.Get("Downsize and paying {0}", user, value.AsCurrency());

            case ActionType.Credit:
                return Terms.Get("Get credit: {0}", user, value.AsCurrency());

            case ActionType.Charity:
                return Terms.Get("Charity: {0}", user, value.AsCurrency());

            case ActionType.Mortgage:
            case ActionType.SchoolLoan:
            case ActionType.CarLoan:
            case ActionType.CreditCard:
            case ActionType.SmallCredit:
            case ActionType.BankLoan:
            case ActionType.PayOffBoat:
            case ActionType.BankruptcyBankLoan:
                var reduceLiabilities = Terms.Get("Reduce Liabilities", user);
                var type = Terms.Get(Action, user);
                var amount = value.AsCurrency();
                return $"{reduceLiabilities}. {type}: {amount}";

            case ActionType.BuyRealEstate:
            case ActionType.BuyBusiness:
            case ActionType.BuyStocks:
            case ActionType.BuyLand:
            case ActionType.StartCompany:
            case ActionType.BuyCoins:
                var buyAsset = Terms.Get(Action, user);
                var asset = AssetService.Get(assetId, user);
                var description = GetAssetDescription(asset, user);
                return $"{buyAsset}. {description}";

            case ActionType.IncreaseCashFlow:
                var smallBusiness = AssetService.Get(assetId, user);
                var increaseCashFlow = Terms.Get(Action, user);
                return $"*{smallBusiness.Title}* - {increaseCashFlow}. {value.AsCurrency()}";

            case ActionType.SellRealEstate:
            case ActionType.SellBusiness:
            case ActionType.SellStocks:
            case ActionType.SellLand:
            case ActionType.SellCoins:
            case ActionType.BankruptcySellAsset:
                var sellAsset = Terms.Get(Action, user);
                var assetToSell = AssetService.Get(assetId, user);
                var sellDescription = GetAssetDescription(assetToSell, user);

                return $"{sellAsset}. {sellDescription}";

            case ActionType.Stocks1To2:
            case ActionType.Stocks2To1:
                var multiply = Terms.Get(Action, user);
                var stock = AssetService.Get(assetId, user);
                var stockDescription = GetAssetDescription(stock, user);

                return $"{multiply}. {stockDescription}";

            case ActionType.MicroCredit:
                return Terms.Get("Pay with Credit Card", user) + " - " + value.AsCurrency();

            case ActionType.BuyBoat:
                var buyBoat = Terms.Get("Buy a boat", user);
                return $"{buyBoat}: {value.AsCurrency()}";

            case ActionType.BankruptcyDebtRestructuring:
            case ActionType.Bankruptcy:
                return Terms.Get(Action, user);

            case ActionType.GoToBigCircle:
            case ActionType.Divorce:
            case ActionType.TaxAudit:
            case ActionType.Lawsuit:
                return Terms.Get(Action, user);

            default:
                return $"<{Action}> - {value}";
        }
    }

    public string GetAssetDescription(AssetDto asset, UserDto user)
    {
        var mortgage = Terms.Get("Mortgage", user);
        var price = Terms.Get("Price", user);
        var cashFlow = Terms.Get("Cashflow", user);

        return asset.Type switch
        {
            AssetType.Stock => asset.IsDeleted
                            ? $"*{asset.Title}* - {asset.Qtty} @ {asset.SellPrice.AsCurrency()}"
                            : asset.CashFlow == 0
                                ? $"*{asset.Title}* - {asset.Qtty} @ {asset.Price.AsCurrency()}"
                                : $"*{asset.Title}* - {asset.Qtty} @ {asset.Price.AsCurrency()}, {cashFlow}: {asset.CashFlow.AsCurrency()} x {asset.Qtty} = {(asset.CashFlow * asset.Qtty).AsCurrency()}",

            AssetType.RealEstate => asset.IsDeleted
                            ? $"*{asset.Title}* - {price}: {asset.SellPrice.AsCurrency()}"
                            : $"*{asset.Title}* - {price}: {asset.Price.AsCurrency()}, {mortgage}: {asset.Mortgage.AsCurrency()}, {cashFlow}: {asset.CashFlow.AsCurrency()}",

            AssetType.Land => asset.IsDeleted
                            ? $"*{asset.Title}* - {price}: {asset.SellPrice.AsCurrency()}"
                            : $"*{asset.Title}* - {price}: {asset.Price.AsCurrency()}",

            AssetType.Business => asset.IsDeleted
                            ? $"*{asset.Title}* - {price}: {asset.SellPrice.AsCurrency()}"
                            : asset.Mortgage > 0
                                ? $"*{asset.Title}* - {price}: {asset.Price.AsCurrency()}, {mortgage}: {asset.Mortgage.AsCurrency()}, {cashFlow}: {asset.CashFlow.AsCurrency()}"
                                : $"*{asset.Title}* - {price}: {asset.Price.AsCurrency()}, {cashFlow}: {asset.CashFlow.AsCurrency()}",

            AssetType.BigBusinessType =>
                            $"*{asset.Title}* - {price}: {asset.Price.AsCurrency()}, {cashFlow}: {asset.CashFlow.AsCurrency()}",

            AssetType.Boat => asset.CashFlow == 0
                            ? $"*{asset.Title}* - {price}: {asset.Price.AsCurrency()}"
                            : $"*{asset.Title}* - {price}: {asset.Price.AsCurrency()}, {Terms.Get("monthly", user)}: {asset.CashFlow.AsCurrency()}",

            AssetType.SmallBusinessType => asset.CashFlow == 0
                            ? $"*{asset.Title}* - {price}: {asset.Price.AsCurrency()}"
                            : $"*{asset.Title}* - {price}: {asset.Price.AsCurrency()}, {Terms.Get("monthly", user)}: {asset.CashFlow.AsCurrency()}",

            AssetType.Coin => asset.IsDeleted
                            ? $"*{asset.Title}* - {asset.Qtty} @ {asset.SellPrice.AsCurrency()}"
                            : $"*{asset.Title}* - {asset.Qtty} @ {asset.Price.AsCurrency()}",

            _ => string.Empty,
        };
    }

    public string NoRecordsFound(UserDto user) => Terms.Get("No records found.", user);
}

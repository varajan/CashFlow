using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Data.Services;

public class DescriptionService(ITermsRepository terms, AssetService assetService)
{
    private ITermsRepository Terms { get; } = terms;
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

        var assetsTerm = Terms.Get(56, user, "Assets");
        var assets = $"{Environment.NewLine}{Environment.NewLine}*{assetsTerm}:*{Environment.NewLine}";
        assets += string.Join(Environment.NewLine, personAssets
            .Where(a => !a.IsDeleted)
            .OrderBy(a => a.Type)
            .Select(a => $"• {GetAssetDescription(a, user)}"));

        return assets;
    }

    private string ExpensesDescription(PersonDto person, UserDto user)
    {
        var expensesTerm = Terms.Get(54, user, "Expenses");
        var taxesTerm = Terms.Get(58, user, "Taxes");
        var mortgageTerm = Terms.Get(59, user, "Mortgage/Rent Pay");
        var schoolLoanTerm = Terms.Get(44, user, "School Loan");
        var carLoanTerm = Terms.Get(45, user, "Car Loan");
        var creditCardTerm = Terms.Get(46, user, "Credit Card");
        var smallCreditsTerm = Terms.Get(92, user, "Small Credit");
        var bankLoanTerm = Terms.Get(47, user, "Bank Loan");
        var boatLoanTerm = Terms.Get(114, user, "Boat Loan");
        var otherPaymentTerm = Terms.Get(60, user, "Other Payments");
        var childrenTerm = Terms.Get(61, user, "Children");
        var childrenExpensesTerm = Terms.Get(62, user, "Children Expenses");
        var perChildTerm = Terms.Get(63, user, "per child");

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
        var professionTerm = Terms.Get(50, user, "Profession");
        var cashTerm = Terms.Get(51, user, "Cash");
        var salaryTerm = Terms.Get(52, user, "Salary");
        var incomeTerm = Terms.Get(53, user, "Income");
        var expensesTerm = Terms.Get(54, user, "Expenses");
        var cashFlowTerm = Terms.Get(55, user, "Cashflow");

        return
            $"*{professionTerm}:* {person.Profession}{Environment.NewLine}" +
            $"*{cashTerm}:* {person.Cash.AsCurrency()}{Environment.NewLine}" +
            $"*{salaryTerm}:* {person.Salary.AsCurrency()}{Environment.NewLine}" +
            $"*{incomeTerm}:* {person.GetIncome().AsCurrency()}{Environment.NewLine}" +
            $"*{expensesTerm}:* {(-person.GetTotalExpenses()).AsCurrency()}{Environment.NewLine}" +
            $"*{cashFlowTerm}:* {person.GetSmallCircleCashflow().AsCurrency()}";
    }

    private string BigCircleDescription(PersonDto person, UserDto user)
    {
        var professionTerm = Terms.Get(50, user, "Profession");
        var cashTerm = Terms.Get(51, user, "Cash");
        var cashFlowTerm = Terms.Get(55, user, "Cashflow");
        var initialTerm = Terms.Get(65, user, "Initial");
        var currentTerm = Terms.Get(66, user, "Current");
        var targetTerm = Terms.Get(67, user, "Target");
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
                return Terms.Get(103, user, "Pay {0}", value.AsCurrency());

            case ActionType.GetMoney:
                return Terms.Get(104, user, "Get {0}", value.AsCurrency());

            case ActionType.Child:
                return Terms.Get(105, user, "Get a child");

            case ActionType.Downsize:
                return Terms.Get(106, user, "Downsize and paying {0}", value.AsCurrency());

            case ActionType.Credit:
                return Terms.Get(107, user, "Get credit: {0}", value.AsCurrency());

            case ActionType.Charity:
                return Terms.Get(108, user, "Charity: {0}", value.AsCurrency());

            case ActionType.Mortgage:
            case ActionType.SchoolLoan:
            case ActionType.CarLoan:
            case ActionType.CreditCard:
            case ActionType.SmallCredit:
            case ActionType.BankLoan:
            case ActionType.PayOffBoat:
            case ActionType.BankruptcyBankLoan:
                var reduceLiabilities = Terms.Get(40, user, "Reduce Liabilities");
                var type = Terms.Get((int)Action, user, "Liability");
                var amount = value.AsCurrency();
                return $"{reduceLiabilities}. {type}: {amount}";

            case ActionType.BuyRealEstate:
            case ActionType.BuyBusiness:
            case ActionType.BuyStocks:
            case ActionType.BuyLand:
            case ActionType.StartCompany:
            case ActionType.BuyCoins:
                var buyAsset = Terms.Get((int)Action, user, "Buy Asset");
                var asset = AssetService.Get(assetId, user);
                var description = GetAssetDescription(asset, user);
                return $"{buyAsset}. {description}";

            case ActionType.IncreaseCashFlow:
                var smallBusiness = AssetService.Get(assetId, user);
                var increaseCashFlow = Terms.Get((int)Action, user, "Increase Cashflow");
                return $"*{smallBusiness.Title}* - {increaseCashFlow}. {value.AsCurrency()}";

            case ActionType.SellRealEstate:
            case ActionType.SellBusiness:
            case ActionType.SellStocks:
            case ActionType.SellLand:
            case ActionType.SellCoins:
            case ActionType.BankruptcySellAsset:
                var sellAsset = Terms.Get((int)Action, user, "Sell Asset");
                var assetToSell = AssetService.Get(assetId, user);
                var sellDescription = GetAssetDescription(assetToSell, user);

                return $"{sellAsset}. {sellDescription}";

            case ActionType.Stocks1To2:
            case ActionType.Stocks2To1:
                var multiply = Terms.Get((int)Action, user, "Multiply Stocks");
                var stock = AssetService.Get(assetId, user);
                var stockDescription = GetAssetDescription(stock, user);

                return $"{multiply}. {stockDescription}";

            case ActionType.MicroCredit:
                return Terms.Get(96, user, "Pay with Credit Card") + " - " + value.AsCurrency();

            case ActionType.BuyBoat:
                var buyBoat = Terms.Get(112, user, "Buy a boat");
                return $"{buyBoat}: {value.AsCurrency()}";

            case ActionType.BankruptcyDebtRestructuring:
            case ActionType.Bankruptcy:
                return Terms.Get((int)Action, user, "Bankruptcy");

            case ActionType.GoToBigCircle:
            case ActionType.Divorce:
            case ActionType.TaxAudit:
            case ActionType.Lawsuit:
                return Terms.Get((int)Action, user, "BigCircle");

            default:
                return $"<{Action}> - {value}";
        }
    }

    public string GetAssetDescription(AssetDto asset, UserDto user)
    {
        var mortgage = Terms.Get(43, user, "Mortgage");
        var price = Terms.Get(64, user, "Price");
        var cashFlow = Terms.Get(55, user, "Cashflow");

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
                            : $"*{asset.Title}* - {price}: {asset.Price.AsCurrency()}, {Terms.Get(42, user, "monthly")}: {asset.CashFlow.AsCurrency()}",

            AssetType.SmallBusinessType => asset.CashFlow == 0
                            ? $"*{asset.Title}* - {price}: {asset.Price.AsCurrency()}"
                            : $"*{asset.Title}* - {price}: {asset.Price.AsCurrency()}, {Terms.Get(42, user, "monthly")}: {asset.CashFlow.AsCurrency()}",

            AssetType.Coin => asset.IsDeleted
                            ? $"*{asset.Title}* - {asset.Qtty} @ {asset.SellPrice.AsCurrency()}"
                            : $"*{asset.Title}* - {asset.Qtty} @ {asset.Price.AsCurrency()}",

            _ => string.Empty,
        };
    }

    public string NoRecordsFound(UserDto user) => Terms.Get(111, user, "No records found.");
}

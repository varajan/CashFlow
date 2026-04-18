using CashFlow.Data.Consts;
using CashFlow.Data.Consts.Terms;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Data.Services;

public class DescriptionService(ITranslationService terms, AssetService assetService)
{
    private ITranslationService TranslationService { get; } = terms;
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

        var assetsTerm = TranslationService.Get(Terms.Assets, user);
        var assets = $"{Environment.NewLine}{Environment.NewLine}*{assetsTerm}:*{Environment.NewLine}";
        assets += string.Join(Environment.NewLine, personAssets
            .Where(a => !a.IsDeleted)
            .OrderBy(a => a.Type)
            .Select(a => $"• {GetAssetDescription(a, user)}"));

        return assets;
    }

    private string ExpensesDescription(PersonDto person, UserDto user)
    {
        var expensesTerm = TranslationService.Get(Terms.Expenses, user);
        var taxesTerm = TranslationService.Get(Terms.Taxes, user);
        var mortgageTerm = TranslationService.Get(Terms.MortgageRent, user);
        var schoolLoanTerm = TranslationService.Get(Terms.SchoolLoan, user);
        var carLoanTerm = TranslationService.Get(Terms.CarLoan, user);
        var creditCardTerm = TranslationService.Get(Terms.CreditCard, user);
        var smallCreditsTerm = TranslationService.Get(Terms.SmallCredit, user);
        var bankLoanTerm = TranslationService.Get(Terms.BankLoan, user);
        var boatLoanTerm = TranslationService.Get(Terms.BoatLoan, user);
        var otherPaymentTerm = TranslationService.Get(Terms.OtherPayments, user);
        var childrenTerm = TranslationService.Get(Terms.Children, user);
        var childrenExpensesTerm = TranslationService.Get(Terms.ChildrenExpenses, user);
        var perChildTerm = TranslationService.Get(Terms.PerChild, user);

        var mortgage = GetLiability(person, Liability.Mortgage);
        var schoolLoan = GetLiability(person, Liability.SchoolLoan);
        var carLoan = GetLiability(person, Liability.CarLoan);
        var creditCard = GetLiability(person, Liability.CreditCard);
        var smallCredits = GetLiability(person, Liability.SmallCredit);
        var bankLoan = GetLiability(person, Liability.BankLoan);
        var boatLoan = GetLiability(person, Liability.BoatLoan);
        var taxes = GetLiability(person, Liability.Taxes);
        var others = GetLiability(person, Liability.OtherPayments);
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
        var professionTerm = TranslationService.Get(Terms.Profession, user);
        var cashTerm = TranslationService.Get(Terms.Cash, user);
        var salaryTerm = TranslationService.Get(Terms.Salary, user);
        var incomeTerm = TranslationService.Get(Terms.Income, user);
        var expensesTerm = TranslationService.Get(Terms.Expenses, user);
        var cashFlowTerm = TranslationService.Get(Terms.Cashflow, user);
        var personProfession = TranslationService.Get(person.Profession, user.Language);

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
        var professionTerm = TranslationService.Get(Terms.Profession, user);
        var cashTerm = TranslationService.Get(Terms.Cash, user);
        var cashFlowTerm = TranslationService.Get(Terms.Cashflow, user);
        var initialTerm = TranslationService.Get(Terms.Initial, user);
        var currentTerm = TranslationService.Get(Terms.Current, user);
        var targetTerm = TranslationService.Get(Terms.Target, user);
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
                return TranslationService.Get(Terms.Pay, user, value.AsCurrency());

            case ActionType.GetMoney:
                return TranslationService.Get(Terms.Get, user, value.AsCurrency());

            case ActionType.Child:
                return TranslationService.Get(Terms.GetChild, user);

            case ActionType.Downsize:
                return TranslationService.Get(Terms.DownsizePay, user, value.AsCurrency());

            case ActionType.Credit:
                return TranslationService.Get(Terms.GetCreditAmount, user, value.AsCurrency());

            case ActionType.Charity:
                return TranslationService.Get(Terms.CharityAmount, user, value.AsCurrency());

            case ActionType.Mortgage:
            case ActionType.SchoolLoan:
            case ActionType.CarLoan:
            case ActionType.CreditCard:
            case ActionType.SmallCredit:
            case ActionType.BankLoan:
            case ActionType.PayOffBoat:
            case ActionType.BankruptcyBankLoan:
                var reduceLiabilities = TranslationService.Get(Terms.ReduceLiabilities, user);
                var type = TranslationService.Get(Action.GetDescription(), user);
                var amount = value.AsCurrency();
                return $"{reduceLiabilities}. {type}: {amount}";

            case ActionType.BuyRealEstate:
            case ActionType.BuyBusiness:
            case ActionType.BuyStocks:
            case ActionType.BuyLand:
            case ActionType.StartCompany:
            case ActionType.BuyCoins:
                var buyAsset = TranslationService.Get(Action.GetDescription(), user);
                var asset = AssetService.Get(assetId, user);
                var description = GetAssetDescription(asset, user);
                return $"{buyAsset}. {description}";

            case ActionType.BuyDream:
                var buyDream = TranslationService.Get(Terms.BuyDream, user);
                return $"{buyDream}. {value.AsCurrency()}";

            case ActionType.IncreaseCashFlow:
                var smallBusiness = AssetService.Get(assetId, user);
                var increaseCashFlow = TranslationService.Get(Action.GetDescription(), user);
                return $"*{smallBusiness.Title}* - {increaseCashFlow}. {value.AsCurrency()}";

            case ActionType.SellRealEstate:
            case ActionType.SellBusiness:
            case ActionType.SellStocks:
            case ActionType.SellLand:
            case ActionType.SellCoins:
            case ActionType.BankruptcySellAsset:
                var sellAsset = TranslationService.Get(Action.GetDescription(), user);
                var assetToSell = AssetService.Get(assetId, user);
                var sellDescription = GetAssetDescription(assetToSell, user);

                return $"{sellAsset}. {sellDescription}";

            case ActionType.Stocks1To2:
            case ActionType.Stocks2To1:
                var multiply = TranslationService.Get(Action.GetDescription(), user);
                var stock = AssetService.Get(assetId, user);
                var stockDescription = GetAssetDescription(stock, user);

                return $"{multiply}. {stockDescription}";

            case ActionType.MicroCredit:
                return TranslationService.Get(Terms.PayCard, user) + " - " + value.AsCurrency();

            case ActionType.BuyBoat:
                var buyBoat = TranslationService.Get(Terms.BuyBoat, user);
                return $"{buyBoat}: {value.AsCurrency()}";

            case ActionType.BankruptcyDebtRestructuring:
            case ActionType.Bankruptcy:
                return TranslationService.Get(Action.GetDescription(), user);

            case ActionType.GoToBigCircle:
            case ActionType.Divorce:
            case ActionType.TaxAudit:
            case ActionType.Lawsuit:
                return TranslationService.Get(Action.GetDescription(), user);

            default:
                return $"<{Action}> - {value}";
        }
    }

    public string GetAssetDescription(AssetDto asset, UserDto user)
    {
        var mortgage = TranslationService.Get(Terms.Mortgage, user);
        var price = TranslationService.Get(Terms.Price, user);
        var cashFlow = TranslationService.Get(Terms.Cashflow, user);

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
                            : $"*{asset.Title}* - {price}: {asset.Price.AsCurrency()}, {TranslationService.Get(Terms.Monthly, user)}: {asset.CashFlow.AsCurrency()}",

            AssetType.SmallBusinessType => asset.CashFlow == 0
                            ? $"*{asset.Title}* - {price}: {asset.Price.AsCurrency()}"
                            : $"*{asset.Title}* - {price}: {asset.Price.AsCurrency()}, {TranslationService.Get(Terms.Monthly, user)}: {asset.CashFlow.AsCurrency()}",

            AssetType.Coin => asset.IsDeleted
                            ? $"*{asset.Title}* - {asset.Qtty} @ {asset.SellPrice.AsCurrency()}"
                            : $"*{asset.Title}* - {asset.Qtty} @ {asset.Price.AsCurrency()}",

            _ => string.Empty,
        };
    }

    public string NoRecordsFound(UserDto user) => TranslationService.Get(Terms.NoRecords, user);
}

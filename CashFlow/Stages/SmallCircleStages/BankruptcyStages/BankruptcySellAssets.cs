using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.BankruptcyStages;

public class BankruptcySellAssets(ITermsService termsService, IPersonManager personManager) : BaseStage(termsService, personManager)
{
    private PersonDto Person => PersonManager.Read(CurrentUser);
    private LiabilityDto BankLoan => Person.Liabilities.FirstOrDefault(l => l.Type == Liability.Bank_Loan);
    private IEnumerable<AssetDto> Assets => Person.Assets.Where(a => !a.IsDeleted).OrderBy(x => x.Type);

    public override string Message
    {
        get
        {
            var cashFlow = Terms.Get(55, CurrentUser, "Cashflow");
            var cash = Terms.Get(51, CurrentUser, "Cash");
            var bankLoan = Terms.Get(47, CurrentUser, Liability.Bank_Loan.AsString());
            var price = Terms.Get(64, CurrentUser, "Price");
            var i = 0;

            var message = $"*{Terms.Get(126, CurrentUser, "You're out of money.")}*";
            message += Environment.NewLine + $"{bankLoan}: *{BankLoan.FullAmount.AsCurrency()}*";
            message += Environment.NewLine + $"{cashFlow}: *{PersonManager.GetSmallCircleCashflow(Person).AsCurrency()}*";
            message += Environment.NewLine + $"{cash}: *{Person.Cash.AsCurrency()}*";

            foreach (var asset in Assets)
            {
                i++;
                message += Environment.NewLine +
                    (asset.CashFlow == 0
                    ? $"#{i} - *{asset.Title}* - {price}: {asset.BancrupcySellPrice.AsCurrency()}"
                    : $"#{i} - *{asset.Title}* - {price}: {asset.BancrupcySellPrice.AsCurrency()}, {cashFlow}: {(asset.Qtty * asset.CashFlow).AsCurrency()}");
            }

            return message;
        }
    }

    public override IEnumerable<string> Buttons
    {
        get
        {
            var buttons = new List<string>();
            int i = 0;

            foreach (var asset in Assets)
            {
                buttons.Add($"#{++i}");
            }

            return buttons;
        }
    }

    public override async Task HandleMessage(string message)
    {
        await SellAsset(Person, message.Trim().Replace("#", "").ToInt());
        await ReduceBankLoan(Person);
        await ProcessBankruptcy(Person);
    }

    private async Task SellAsset(PersonDto person, int item)
    {
        var assets = Assets.ToList();

        if (item > 0 && item <= assets.Count)
        {
            var price = Terms.Get(64, CurrentUser, "Price");
            var sellForDepbts = Terms.Get(131, CurrentUser, "Sale for debts");

            var asset = assets[item - 1];
            asset.IsDeleted = true;
            person.Cash += asset.BancrupcySellPrice;

            PersonManager.UpdateAsset(CurrentUser, asset);
            PersonManager.Update(person);

            var message = $"{sellForDepbts}: {asset.Title}, {price}: {asset.BancrupcySellPrice.AsCurrency()}";
            await CurrentUser.Notify(message);
        }
    }

    private Task ReduceBankLoan(PersonDto person)
    {
        if (person.Cash < 1000) return Task.CompletedTask;

        var liability = BankLoan;
        var amount = person.Cash / 1000 * 1000;
        amount = Math.Min(amount, liability.FullAmount);
        var percent = (decimal)1 / 10;
        var cashflow = (int)(amount * percent);

        person.Cash -= amount;
        liability.MarkedForReduction = false;
        liability.Cashflow += cashflow;
        liability.FullAmount -= amount;
        liability.Deleted = liability.FullAmount == 0;

        PersonManager.Update(person);
        PersonManager.Update(CurrentUser, liability);
        PersonManager.AddHistory(ActionType.ReduceLiability, amount, CurrentUser);
        return Task.CompletedTask;
    }
}

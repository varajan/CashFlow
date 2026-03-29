using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.BankruptcyStages;

public class BankruptcySellAssets(ITermsRepository termsService, IPersonService personManager, IUserRepository userRepository)
    : BaseStage(termsService, personManager, userRepository)
{
    private PersonDto Person => PersonService.Read(CurrentUser);
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
            message += Environment.NewLine + $"{cashFlow}: *{Person.GetSmallCircleCashflow().AsCurrency()}*";
            message += Environment.NewLine + $"{cash}: *{Person.Cash.AsCurrency()}*";
            message += Environment.NewLine;
            message += Environment.NewLine + Terms.Get(127, CurrentUser, "You have to sell your assets till you cash flow is positive.");
            message += Environment.NewLine;
            message += Environment.NewLine + Terms.Get(128, CurrentUser, "What asset do you want to sell?");
            message += Environment.NewLine;

            foreach (var asset in Assets)
            {
                i++;
                message += Environment.NewLine +
                    (asset.CashFlow == 0
                    ? $"#{i} - *{asset.Title}* - {price}: {asset.GetBancrupcySellPrice().AsCurrency()}"
                    : $"#{i} - *{asset.Title}* - {price}: {asset.GetBancrupcySellPrice().AsCurrency()}, {cashFlow}: {(asset.Qtty * asset.CashFlow).AsCurrency()}");
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

            buttons.Add(StopGame);
            buttons.Add(History);

            return buttons;
        }
    }

    public override async Task HandleMessage(string message)
    {
        if (MessageEquals(message, 41, "Stop Game"))
        {
            NextStage = New<StopGame>();
            return;
        }

        if (MessageEquals(message, 2, "History"))
        {
            NextStage = New<History>();
            return;
        }

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
            var bancrupcySellPrice = asset.GetBancrupcySellPrice();

            asset.IsDeleted = true;
            person.Cash += bancrupcySellPrice;
            PersonService.Update(person);
            PersonService.SellAsset(asset, bancrupcySellPrice, CurrentUser);
            PersonService.AddHistory(ActionType.BankruptcySellAsset, bancrupcySellPrice, CurrentUser, asset.Id);
            ReduceLiability(person, asset);

            var message = $"{sellForDepbts}: {asset.Title}, {price}: {asset.GetBancrupcySellPrice().AsCurrency()}";
            await CurrentUser.Notify(message);
        }
    }

    private void ReduceLiability(PersonDto person, AssetDto asset)
    {
        if (asset.Type != AssetType.Boat) return;
        if (asset.CashFlow == 0) return;

        var liability = person.Liabilities.FirstOrDefault(l => l.Type == Liability.Boat_Loan);

        liability.Cashflow = 0;
        liability.FullAmount = 0;
        liability.Deleted = true;

        PersonService.Update(CurrentUser, liability);
        PersonService.AddHistory((ActionType)liability.Type, 0, CurrentUser);
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

        PersonService.Update(person);
        PersonService.Update(CurrentUser, liability);
        PersonService.AddHistory(ActionType.BankLoan, amount, CurrentUser);
        return Task.CompletedTask;
    }
}

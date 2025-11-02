using CashFlowBot.Data.Consts;
using CashFlowBot.Data.DataBase;
using CashFlowBot.Data.Users.UserData.PersonData;
using CashFlowBot.Extensions;
using System.Collections.Generic;

namespace CashFlowBot.Data.Users.UserData.HistoryData;

public class HistoryRecord(IDataBase dataBase)
{
    private IDataBase DataBase { get; } = dataBase;

    private ITermsService Terms => new TermsService(DataBase);
    public long Id { get; set; }
    public long UserId { get; set; }
    public IUser User { get; set; }
    public ActionType Action { get; set; }
    public long Value { get; set; }
    public string Description { get; set; }

    public HistoryRecord Init(IUser user, ActionType action, long value)
    {
        User = user;
        Action = action;
        Value = value;

        return this;
    }

    public HistoryRecord Init(IList<string> historyRow)
    {
        Id = historyRow[0].ToLong();
        UserId = historyRow[1].ToLong();
        Action = historyRow[2].ParseEnum<ActionType>();
        Value = historyRow[3].ToLong();
        Description = historyRow[4];

        return this;
    }

    public void Add()
    {
        long newId = DataBase.GetValue("SELECT MAX(ID) FROM History").ToLong() + 1;
        DataBase.Execute($@"INSERT INTO History VALUES ({newId}, {User.Id}, {(int)Action}, {Value}, '• {Text}')");
    }

    public void Delete() => DataBase.Execute($"DELETE FROM History WHERE ID = {Id}");

    private Asset Asset => new(DataBase, User, (int)Value);
    private string Text
    {
        get
        {
            switch (Action)
            {
                case ActionType.PayMoney:
                    return Terms.Get(103, User, "Pay {0}", Value.AsCurrency());

                case ActionType.GetMoney:
                    return Terms.Get(104, User, "Get {0}", Value.AsCurrency());

                case ActionType.Child:
                    return Terms.Get(105, User, "Get a child");

                case ActionType.Downsize:
                    return Terms.Get(106, User, "Downsize and paying {0}", Value.AsCurrency());

                case ActionType.Credit:
                    return Terms.Get(107, User, "Get credit: {0}", Value.AsCurrency());

                case ActionType.Charity:
                    return Terms.Get(108, User, "Charity: {0}", Value.AsCurrency());

                case ActionType.Mortgage:
                case ActionType.SchoolLoan:
                case ActionType.CarLoan:
                case ActionType.CreditCard:
                case ActionType.SmallCredit:
                case ActionType.BankLoan:
                case ActionType.PayOffBoat:
                case ActionType.BankruptcyBankLoan:
                    var reduceLiabilities = Terms.Get(40, User, "Reduce Liabilities");
                    var type = Terms.Get((int)Action, User, "Liability");
                    var amount = Value.AsCurrency();
                    return $"{reduceLiabilities}. {type}: {amount}";

                case ActionType.BuyRealEstate:
                case ActionType.BuyBusiness:
                case ActionType.BuyStocks:
                case ActionType.BuyLand:
                case ActionType.StartCompany:
                case ActionType.BuyCoins:
                    var buyAsset = Terms.Get((int)Action, User, "Buy Asset");
                    return $"{buyAsset}. {Asset.Description}";

                case ActionType.IncreaseCashFlow:
                    var increaseCashFlow = Terms.Get((int)Action, User, "Increase Cash Flow");
                    return $"{increaseCashFlow}. {Value.AsCurrency()}";

                case ActionType.SellRealEstate:
                case ActionType.SellBusiness:
                case ActionType.SellStocks:
                case ActionType.SellLand:
                case ActionType.SellCoins:
                case ActionType.BankruptcySellAsset:
                    var sellAsset = Terms.Get((int)Action, User, "Sell Asset");
                    return $"{sellAsset}. {Asset.Description}";

                case ActionType.Stocks1To2:
                case ActionType.Stocks2To1:
                    var multiply = Terms.Get((int)Action, User, "Multiply Stocks");
                    return $"{multiply}. {Asset.Description}";

                case ActionType.MicroCredit:
                    return Terms.Get(96, User, "Pay with Credit Card") + " - " + Value.AsCurrency();

                case ActionType.BuyBoat:
                    var buyBoat = Terms.Get(112, User, "Buy a boat");
                    return $"{buyBoat}: {Value.AsCurrency()}";

                case ActionType.BankruptcyDebtRestructuring:
                case ActionType.Bankruptcy:
                    return Terms.Get((int)Action, User, "Bankruptcy");

                case ActionType.GoToBigCircle:
                case ActionType.Divorce:
                case ActionType.TaxAudit:
                case ActionType.Lawsuit:
                    return Terms.Get((int)Action, User, "BigCircle");

                default:
                    return $"<{Action}> - {Value}";
            }
        }
    }
}
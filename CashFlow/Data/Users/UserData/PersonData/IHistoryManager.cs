using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Data.Users.UserData.PersonData;

public interface IHistoryManager
{
    bool IsEmpty(long userId);
    string TopFive(long userId, IUser currentUser);
    List<HistoryDto> Read(long userId);
    void Add(ActionType type, long value, IUser user);
    void Rollback(HistoryDto record);
}

public class HistoryManager(IDataBase dataBase, IAssetManager assetManager, ITermsService terms) : IHistoryManager
{
    IAssetManager AssetManager { get; } = assetManager;
    IDataBase DataBase { get; } = dataBase;
    ITermsService TermsService { get; } = terms;

    //public int Id { get; set; }
    //public int UserId { get; set; }
    //public ActionType Action { get; set; }
    //public int Value { get; set; }
    //public string Description { get; set; }

    public bool IsEmpty(long userId) => DataBase.GetValue($"SELECT COUNT(*) FROM History WHERE UserID = {userId}").ToInt() == 0;

    public void Add(ActionType type, long value, IUser user)
    {
        var record = new HistoryDto { UserId = user.Id, Action = type, Value = value };
        long newId = DataBase.GetValue("SELECT MAX(ID) FROM History").ToLong() + 1;
        var text = GetDescription(record, user);
        DataBase.Execute($@"INSERT INTO History VALUES ({newId}, {user.Id}, {(int)type}, {value}, '• {text}')");
    }

    public string TopFive(long userId, IUser currentUser)
    {
        var records = Read(userId);
        records.Reverse();

        return records.Any() ?
            TermsService.Get(111, currentUser, "No records found.") :
            string.Join(Environment.NewLine, records.Take(5).Select(x => x.Description));
    }

    public List<HistoryDto> Read(long userId)
    {
        var sql = $"SELECT * FROM History WHERE UserID = {userId}";
        var rows = DataBase.GetRows(sql);

        throw new NotImplementedException();

        //return new AssetDto
        //{
        //    Type = data["Type"].ParseEnum<AssetType>(),
        //    Title = data["Title"],
        //    Id = data["Id"].ToInt(),
        //    UserId = data["UserId"].ToInt(),
        //    Price = data["Price"].ToInt(),
        //    SellPrice = data["SellPrice"].ToInt(),
        //    Qtty = data["Qtty"].ToInt(),
        //    Mortgage = data["Mortgage"].ToInt(),
        //    TotalCashFlow = data["TotalCashFlow"].ToInt(),
        //    CashFlow = data["CashFlow"].ToInt(),
        //    BigCircle = data["BigCircle"].ToInt() == 1,
        //    IsDraft = data["IsDraft"].ToInt() == 1,
        //    IsDeleted = data["IsDeleted"].ToInt() == 1,
        //};
    }

    public void Rollback(HistoryDto record)
    {
        DataBase.Execute($"DELETE FROM History WHERE ID = {record.Id}");
        throw new NotImplementedException();
    }

    private string GetDescription(HistoryDto record, IUser user)
    {
        switch (record.Action)
        {
            case ActionType.PayMoney:
                return TermsService.Get(103, user, "Pay {0}", record.Value.AsCurrency());

            case ActionType.GetMoney:
                return TermsService.Get(104, user, "Get {0}", record.Value.AsCurrency());

            case ActionType.Child:
                return TermsService.Get(105, user, "Get a child");

            case ActionType.Downsize:
                return TermsService.Get(106, user, "Downsize and paying {0}", record.Value.AsCurrency());

            case ActionType.Credit:
                return TermsService.Get(107, user, "Get credit: {0}", record.Value.AsCurrency());

            case ActionType.Charity:
                return TermsService.Get(108, user, "Charity: {0}", record.Value.AsCurrency());

            case ActionType.Mortgage:
            case ActionType.SchoolLoan:
            case ActionType.CarLoan:
            case ActionType.CreditCard:
            case ActionType.SmallCredit:
            case ActionType.BankLoan:
            case ActionType.PayOffBoat:
            case ActionType.BankruptcyBankLoan:
                var reduceLiabilities = TermsService.Get(40, user, "Reduce Liabilities");
                var type = TermsService.Get((int)record.Action, user, "Liability");
                var amount = record.Value.AsCurrency();
                return $"{reduceLiabilities}. {type}: {amount}";

            case ActionType.BuyRealEstate:
            case ActionType.BuyBusiness:
            case ActionType.BuyStocks:
            case ActionType.BuyLand:
            case ActionType.StartCompany:
            case ActionType.BuyCoins:
                var buyAsset = TermsService.Get((int)record.Action, user, "Buy Asset");
                var asset = AssetManager.Read(record.Value, user.Id);
                var description = AssetManager.GetDescription(asset, user);

                return $"{buyAsset}. {description}";

            case ActionType.IncreaseCashFlow:
                var increaseCashFlow = TermsService.Get((int)record.Action, user, "Increase Cash Flow");
                return $"{increaseCashFlow}. {record.Value.AsCurrency()}";

            case ActionType.SellRealEstate:
            case ActionType.SellBusiness:
            case ActionType.SellStocks:
            case ActionType.SellLand:
            case ActionType.SellCoins:
            case ActionType.BankruptcySellAsset:
                var sellAsset = TermsService.Get((int)record.Action, user, "Sell Asset");
                var assetToSell = AssetManager.Read(record.Value, user.Id);
                var sellDescription = AssetManager.GetDescription(assetToSell, user);

                return $"{sellAsset}. {sellDescription}";

            case ActionType.Stocks1To2:
            case ActionType.Stocks2To1:
                var multiply = TermsService.Get((int)record.Action, user, "Multiply Stocks");
                var stock = AssetManager.Read(record.Value, user.Id);
                var stockDescription = AssetManager.GetDescription(stock, user);

                return $"{multiply}. {stockDescription}";

            case ActionType.MicroCredit:
                return TermsService.Get(96, user, "Pay with Credit Card") + " - " + record.Value.AsCurrency();

            case ActionType.BuyBoat:
                var buyBoat = TermsService.Get(112, user, "Buy a boat");
                return $"{buyBoat}: {record.Value.AsCurrency()}";

            case ActionType.BankruptcyDebtRestructuring:
            case ActionType.Bankruptcy:
                return TermsService.Get((int)record.Action, user, "Bankruptcy");

            case ActionType.GoToBigCircle:
            case ActionType.Divorce:
            case ActionType.TaxAudit:
            case ActionType.Lawsuit:
                return TermsService.Get((int)record.Action, user, "BigCircle");

            default:
                return $"<{record.Action}> - {record.Value}";
        }
    }
}

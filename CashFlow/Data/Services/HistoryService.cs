using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Interfaces;
using MoreLinq;

namespace CashFlow.Data.Services;

public class HistoryService(IDataBase dataBase, IPersonRepository personRepository, DescriptionService personDescriptionService, ITranslationService terms)
{
    private IDataBase DataBase { get; } = dataBase;
    private AssetService AssetService => new(PersonRepository);
    private IPersonRepository PersonRepository { get; } = personRepository;
    private ITranslationService Terms { get; } = terms;

    public void AddRecord(ActionType type, long value, UserDto user, long assetId)
    {
        var record = new HistoryDto
        {
            UserId = user.Id,
            Date = DateTime.UtcNow,
            Action = type,
            Value = value,
            AssetId = assetId,
            Description = $"• {personDescriptionService.GetDescription(type, value, user, assetId)}"
        };

        DataBase.Execute($@"INSERT INTO History (UserId, Id, HistoryRecord) VALUES ({user.Id}, {record.Date.Ticks}, '{record.Serialize()}')");
    }

    public bool IsHistoryEmpty(long userId) => DataBase.GetValue($"SELECT COUNT(*) FROM History WHERE UserID = {userId}").ToInt() == 0;

    public List<HistoryDto> ReadHistory(long userId)
    {
        var sql = $"SELECT * FROM History WHERE UserID = {userId}";
        var data = DataBase.GetRows(sql);
        var result = data.Select(row => row["HistoryRecord"].Deserialize<HistoryDto>()).OrderBy(r => r.Date).ToList();
        return result;
    }

    public string GetTopFive(UserDto user, UserDto currentUser)
    {
        var records = ReadHistory(user.Id);
        records.Reverse();

        return records.Any()
            ? string.Join(Environment.NewLine, records.Take(5).Select(x => x.Description))
            : personDescriptionService.NoRecordsFound(currentUser);
    }

    public void RollbackRecord(PersonDto person, HistoryDto record)
    {
        var boat = person.Assets.FirstOrDefault(a => a.Type == AssetType.Boat);
        var asset = person.Assets.Find(a => a.Id == (int)record.AssetId);
        var amount = (int)record.Value;

        var profession = Terms.Get(person.Profession);
        var defaults = PersonRepository.GetDefault(profession, person.Id);

        decimal percent;
        int expenses;

        switch (record.Action)
        {
            case ActionType.PayMoney:
            case ActionType.Downsize:
            case ActionType.Charity:
                person.Cash += amount;
                break;

            case ActionType.GetMoney:
                person.Cash -= amount;
                break;

            case ActionType.Child:
                person.Children--;
                break;

            case ActionType.Credit:
                person.Cash -= amount;
                person.UpdateLiability(Liability.BankLoan, amount / 10, -amount);
                break;

            case ActionType.Mortgage:
                person.Cash += amount;
                person.UpdateLiability(defaults.GetLiability(Liability.Mortgage));
                break;

            case ActionType.SchoolLoan:
                person.Cash += amount;
                person.UpdateLiability(defaults.GetLiability(Liability.SchoolLoan));
                break;

            case ActionType.CarLoan:
                person.Cash += amount;
                person.UpdateLiability(defaults.GetLiability(Liability.CarLoan));
                break;

            case ActionType.CreditCard:
                var creditCard = defaults.GetLiability(Liability.CreditCard);
                percent = (decimal)creditCard.Cashflow / creditCard.FullAmount;
                expenses = (int)(amount * percent);

                person.Cash += amount;
                person.UpdateLiability(Liability.CreditCard, expenses, amount);
                break;

            case ActionType.SmallCredit:
                var smallCredit = defaults.GetLiability(Liability.SmallCredit);
                percent = (decimal)smallCredit.Cashflow / smallCredit.FullAmount;
                expenses = (int)(amount * percent);

                person.Cash += amount;
                person.UpdateLiability(Liability.SmallCredit, expenses, amount);
                break;

            case ActionType.BankLoan:
                person.Cash += amount;
                person.UpdateLiability(Liability.BankLoan, -amount / 10, amount);
                break;

            case ActionType.BankruptcyBankLoan:
                percent = 0.1m;
                expenses = (int)(amount * percent);

                person.Cash += amount;
                person.UpdateLiability(Liability.BankLoan, amount / 10, amount);
                person.Bankruptcy = true;
                break;

            case ActionType.BuyRealEstate:
            case ActionType.BuyBusiness:
            case ActionType.BuyLand:
            case ActionType.StartCompany:
                person.Cash += asset.Price - asset.Mortgage;
                AssetService.Delete(person, asset);
                break;

            case ActionType.BuyDream:
                person.Cash += amount;
                person.BoughtDream = false;
                break;

            case ActionType.IncreaseCashFlow:
                person.Assets.Where(a => a.Type == AssetType.SmallBusiness).ForEach(x => x.CashFlow -= (int)record.Value);
                break;

            case ActionType.SellRealEstate:
            case ActionType.SellBusiness:
            case ActionType.SellLand:
                person.Cash -= asset.SellPrice - asset.Mortgage;
                AssetService.Restore(person, asset);
                break;

            case ActionType.BuyStocks:
            case ActionType.BuyCoins:
                person.Cash += asset.Price * asset.Qtty;
                AssetService.Delete(person, asset);
                break;

            case ActionType.SellStocks:
            case ActionType.SellCoins:
                person.Cash -= asset.Qtty * asset.SellPrice;
                AssetService.Restore(person, asset);
                break;

            case ActionType.Stocks1To2:
                asset.Qtty /= 2;
                break;

            case ActionType.Stocks2To1:
                asset.Qtty *= 2;
                break;

            case ActionType.MicroCredit:
                person.UpdateLiability(Liability.CreditCard, (int)(amount * 0.03), -amount);
                break;

            case ActionType.BuyBoat:
                person.Cash += 1_000;
                person.DeleteLiability(Liability.BoatLoan);
                AssetService.Delete(person, boat);
                break;

            case ActionType.PayOffBoat:
                person.Cash += amount;
                boat.CashFlow = -340;
                person.UpdateLiability(Liability.BoatLoan, boat.CashFlow, boat.Mortgage);
                AssetService.Restore(person, boat);
                break;

            case ActionType.Bankruptcy:
                person.Bankruptcy = false;
                break;

            case ActionType.BankruptcySellAsset:
                person.Cash -= asset.GetBancrupcySellPrice();
                person.Bankruptcy = true;
                AssetService.Restore(person, asset);
                break;

            case ActionType.BankruptcyDebtRestructuring:
                foreach (var liability in person.Liabilities.Where(l => l.IsBankruptcyDivisible))
                {
                    liability.FullAmount *= 2;
                    liability.Cashflow *= 2;
                }
                break;

            case ActionType.GoToBigCircle:
                person.Cash -= person.InitialCashFlow;
                person.InitialCashFlow = 0;
                person.BigCircle = false;
                break;

            case ActionType.Divorce:
            case ActionType.TaxAudit:
            case ActionType.Lawsuit:
                person.Cash += amount;
                break;

            default:
                throw new Exception($"<{record.Action}> ???");
        }

        PersonRepository.Save(person);
        DataBase.Execute($"DELETE FROM History WHERE UserId = {record.UserId} AND Id = {record.Date.Ticks}");
    }
}

using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Data.Users.UserData.PersonData;

public enum Circle
{
    Small,
    Big,
    Both
}

public interface IPersonManager
{
    bool Exists(IUser user);
    void Create(string profession, IUser user);
    void Update(PersonDto person);
    PersonDto Read(IUser user);

    string GetDescription(IUser user, bool compact = true);
    void Delete(IUser user);
    void Update(IUser user, LiabilityDto liability);

    void AddHistory(ActionType type, long value, IUser user);
    void AddHistory(ActionType type, long value, IUser user, long assetId);
    List<HistoryDto> ReadHistory(IUser user);
    bool IsHistoryEmpty(IUser user);
    string HistoryTopFive(IUser user, IUser currentUser);
    void RollbackHistory(PersonDto person, HistoryDto record);

    List<AssetDto> ReadAllAssets(AssetType type, IUser user);
    void CreateAsset(IUser user, AssetDto asset);
    void DeleteAsset(IUser user, AssetDto asset);
    void UpdateAsset(IUser user, AssetDto asset);
    void SellAsset(AssetDto asset, ActionType action, int price, IUser user);
    string GetAssetDescription(AssetDto asset, IUser user);
}

public class PersonManager(IPersonRepository personRepository, IDataBase dataBase, ITermsService terms) : IPersonManager
{
    private IPersonRepository PersonRepository { get; } = personRepository;
    private AssetService AssetService => new(PersonRepository);
    private HistoryService HistoryService => new(dataBase, PersonRepository, PersonDescriptionService);
    private DescriptionService PersonDescriptionService => new(terms, AssetService);


    public bool Exists(IUser user) => PersonRepository.Exists(user.Id);
    public void Update(PersonDto person) => PersonRepository.Save(person);
    public PersonDto Read(IUser user) => PersonRepository.Get(user.Id);
    public void Delete(IUser user) => PersonRepository.Delete(user.Id);

    public void Create(string profession, IUser user)
    {
        var defaults = Persons.Get(profession);

        PersonRepository.Delete(user.Id);

        var person = new PersonDto
        {
            Id = user.Id,
            Profession = defaults.Profession[user.Language],
            Cash = defaults.Cash,
            Salary = defaults.Salary,
            PerChild = defaults.Expenses.PerChild,
        };

        person.Liabilities =
            [
            new()
            {
                Type = Liability.Taxes,
                Name = Liability.Taxes.AsString(),
                FullAmount = 0,
                Cashflow = -defaults.Expenses.Taxes
            },

            new()
            {
                Type = Liability.Mortgage,
                Name = Liability.Mortgage.AsString(),
                FullAmount = defaults.Liabilities.Mortgage,
                Cashflow = -defaults.Expenses.Mortgage
            },

            new()
            {
                Type = Liability.School_Loan,
                Name = Liability.School_Loan.AsString(),
                FullAmount = defaults.Liabilities.SchoolLoan,
                Cashflow = -defaults.Expenses.SchoolLoan
            },

            new()
            {
                Type = Liability.Car_Loan,
                Name = Liability.Car_Loan.AsString(),
                IsBankruptcyDivisible = true,
                FullAmount = defaults.Liabilities.CarLoan,
                Cashflow = -defaults.Expenses.CarLoan
            },

            new()
                {
                Type = Liability.Credit_Card,
                Name = Liability.Credit_Card.AsString(),
                IsBankruptcyDivisible = true,
                FullAmount = defaults.Liabilities.CreditCard,
                Cashflow = -defaults.Expenses.CreditCard
            },

            new()
            {
                Type = Liability.Bank_Loan,
                Name = Liability.Bank_Loan.AsString(),
                FullAmount = defaults.Liabilities.BankLoan,
                Cashflow = -defaults.Expenses.BankLoan,
                AllowsPartialPayment = true
            },

            new()
            {
                Type = Liability.Others,
                Name = Liability.Others.AsString(),
                FullAmount = 0,
                Cashflow = -defaults.Expenses.Others
            },

            new()
            {
                Type = Liability.Small_Credit,
                Name = Liability.Small_Credit.AsString(),
                IsBankruptcyDivisible = true,
                FullAmount = defaults.Liabilities.SmallCredits,
                Cashflow = -defaults.Expenses.SmallCredits
            },
        ];

        person.Cash += person.GetSmallCircleCashflow();
        PersonRepository.Save(person);
    }

    public string GetDescription(IUser user, bool compact = true)
    {
        var person = PersonRepository.Get(user.Id);
        return PersonDescriptionService.GetDescription(user, person, compact);
    }

    public void Update(IUser user, LiabilityDto liability)
    {
        var person = PersonRepository.Get(user.Id);
        var index = person.Liabilities.FindIndex(l => l.Name == liability.Name);
        person.Liabilities[index] = liability;
        PersonRepository.Save(person);
    }

    #region History

    public void AddHistory(ActionType type, long value, IUser user) => HistoryService.AddRecord(type, value, user, 0);
    public void AddHistory(ActionType type, long value, IUser user, long assetId) => HistoryService.AddRecord(type, value, user, assetId);
    public bool IsHistoryEmpty(IUser user) => HistoryService.IsHistoryEmpty(user.Id);
    public List<HistoryDto> ReadHistory(IUser user) => HistoryService.ReadHistory(user.Id);
    public string HistoryTopFive(IUser user, IUser currentUser) => HistoryService.GetTopFive(user, currentUser);
    public void RollbackHistory(PersonDto person, HistoryDto record) => HistoryService.RollbackRecord(person, record);

    #endregion

    #region Assets

    public List<AssetDto> ReadAllAssets(AssetType type, IUser user) => AssetService.GetAll(type, user);
    public void CreateAsset(IUser user, AssetDto asset) => AssetService.Create(user, asset);
    public void DeleteAsset(IUser user, AssetDto asset) => AssetService.Delete(user, asset);
    public void UpdateAsset(IUser user, AssetDto asset) => AssetService.Update(user, asset);
    public void SellAsset(AssetDto asset, ActionType action, int price, IUser user) => AssetService.Sell(asset, action, price, user);
    public string GetAssetDescription(AssetDto asset, IUser user) => PersonDescriptionService.GetAssetDescription(asset, user);

    #endregion
}

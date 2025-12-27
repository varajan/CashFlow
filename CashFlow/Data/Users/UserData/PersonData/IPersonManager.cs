using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Data.Users.UserData.PersonData;

public interface IPersonManager
{
    bool Exists(long id);
    void Create(string profession, long userId);
    void Update(PersonDto person);
    PersonDto Read(long id);
    string GetDescription(long id);
    void Delete(long id);

    void UpdateLiability(long id, LiabilityDto liability);

    void AddHistory(ActionType type, long value, IUser user);
}

public class PersonManager(IDataBase dataBase, ITermsService terms) : IPersonManager
{
    public void Create(string profession, long userId)
    {
        throw new NotImplementedException();

        var defaultProfessionData = Persons.Get(profession);

        //Clear();
        dataBase.Execute($"INSERT INTO Persons " +
                   "(ID, Profession, Salary, Cash, SmallRealEstate, ReadyForBigCircle, BigCircle, InitialCashFlow, Bankruptcy, CreditsReduced) " +
                   $"VALUES ({userId}, '', '', '', '', '', '', '', 0, 0)");

        //Assets.Clear();
        //Profession = defaultProfessionData.Profession[User.Language];
        //Cash = defaultProfessionData.Cash;
        //Salary = defaultProfessionData.Salary;

        //Expenses.Clear();
        //Expenses.Create(defaultProfessionData.Expenses);

        //Liabilities.Clear();
        //Liabilities.Create(defaultProfessionData.Liabilities);
    }

    public void Update(PersonDto person)
    {
        var sql = $"" +
            $"UPDATE Persons SET " +
            $"Profession = '{person.Profession}'," +
            $"Salary = {person.Salary}," +
            $"Cash = {person.Cash}," +
            $"ReadyForBigCircle = {(person.ReadyForBigCircle ? 1 : 0)}," +
            $"BigCircle = {(person.BigCircle ? 1 : 0)}," +
            $"InitialCashFlow = {person.InitialCashFlow}," +
            $"Bankruptcy = {(person.Bankruptcy ? 1 : 0)}," +
            $"CreditsReduced = {(person.CreditsReduced ? 1 : 0)}," +
            $"WHERE ID = {person.Id}";
        dataBase.Execute(sql);
    }

    public bool Exists(long id)
    {
        var sql = $"SELECT * FROM Persons WHERE ID = {id}";
        var data = dataBase.GetRows(sql);

        return data.Any();
    }

    public PersonDto Read(long id)
    {
        var sql = $"SELECT * FROM Persons WHERE ID = {id}";
        var data = dataBase.GetRow(sql);

        return new PersonDto
        {
            Id = id,
            Profession = data["Profession"],
            Salary = data["Salary"].ToInt(),
            Cash = data["Cash"].ToInt(),
            ReadyForBigCircle = data["ReadyForBigCircle"].ToInt() == 1,
            BigCircle = data["BigCircle"].ToInt() == 1,
            InitialCashFlow = data["InitialCashFlow"].ToInt(),
            Bankruptcy = data["Bankruptcy"].ToInt() == 1,
            CreditsReduced = data["CreditsReduced"].ToInt() == 1,
        };
    }

    public string GetDescription(long id) => throw new NotImplementedException();

    public void Delete(long id) => throw new NotImplementedException();

    public void UpdateLiability(long id, LiabilityDto liability) => throw new NotImplementedException();

    public void AddHistory(ActionType type, long value, IUser user)
    {
        //var record = new HistoryDto { UserId = user.Id, Action = type, Value = value };
        //long newId = DataBase.GetValue("SELECT MAX(ID) FROM History").ToLong() + 1;
        //var text = GetDescription(record, user);
        //DataBase.Execute($@"INSERT INTO History VALUES ({newId}, {user.Id}, {(int)type}, {value}, '• {text}')");
    }
}

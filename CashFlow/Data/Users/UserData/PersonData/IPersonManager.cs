using CashFlow.Data.DataBase;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;

namespace CashFlow.Data.Users.UserData.PersonData;

public interface IPersonManager
{
    bool Exists(long id);
    void Create(PersonDto person);
    void Update(PersonDto person);
    PersonDto Read(long id);
    string GetDescription(PersonDto person);
    void Delete(long id);
}

public class PersonManager(IDataBase dataBase, ITermsService terms) : IPersonManager
{
    public void Create(PersonDto person)
    {
        throw new NotImplementedException();
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

    public string GetDescription(PersonDto person) => throw new NotImplementedException();

    public void Delete(long id) => throw new NotImplementedException();
}

using CashFlow.Data.DTOs;
using CashFlow.Data.Users;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Infrastructure;

public interface IPersonRepository
{
    PersonDto Get(long userId);
    void Save(PersonDto person);
    void Delete(long userId);
    bool Exists(long userId);
}

public class PersonRepository(IDataBase dataBase) : IPersonRepository
{
    private IDataBase DataBase { get; } = dataBase;

    public PersonDto Get(long userId)
    {
        var personData = DataBase.GetValue($"SELECT PersonData FROM Persons WHERE ID = {userId}");

        return string.IsNullOrEmpty(personData)
            ? default
            : personData.Deserialize<PersonDto>();
    }

    public void Save(PersonDto person)
    {
        person.LastActive = DateTime.Now;

        if (Exists(person.Id))
        {
            DataBase.Execute($"UPDATE Persons SET PersonData = '{person.Serialize()}' WHERE ID = {person.Id}");
        }
        else
        {
            DataBase.Execute($"INSERT INTO Persons (ID, PersonData) VALUES ({person.Id}, '{person.Serialize()}')");
        }
    }

    public void Delete(long userId)
    {
        DataBase.Execute($"DELETE FROM History WHERE UserID = {userId}");
        DataBase.Execute($"DELETE FROM Persons WHERE ID = {userId}");
        // assets?
        // liabilities?
        // history?
    }

    public bool Exists(long userId)
    {
        var sql = $"SELECT * FROM Persons WHERE ID = {userId}";
        var data = DataBase.GetRows(sql);

        return data.Any();
    }
}

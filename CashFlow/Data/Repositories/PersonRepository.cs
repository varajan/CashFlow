using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Data.Repositories;

public class PersonRepository(IDataBase dataBase) : IPersonRepository
{
    private IDataBase DataBase { get; } = dataBase;

    public List<PersonDto> GetAll()
    {
        var sql = $"SELECT PersonData FROM Persons";
        var data = DataBase.GetRows(sql);
        return data.Select(row => row["PersonData"].ToString().Deserialize<PersonDto>()).ToList();
    }

    public PersonDto Get(long userId)
    {
        var personData = DataBase.GetValue($"SELECT PersonData FROM Persons WHERE ID = {userId}");

        return string.IsNullOrEmpty(personData)
            ? default
            : personData.Deserialize<PersonDto>();
    }

    public List<string> GetAllProfessions()
    {
        var path = $"{AppDomain.CurrentDomain.BaseDirectory}/Data/Persons";
        var professions = Directory.GetFiles(path, "*.json").Select(Path.GetFileNameWithoutExtension).ToList();
        return professions;
    }

    public PersonDto GetDefault(string profession, long userId)
    {
        var json = $"{AppDomain.CurrentDomain.BaseDirectory}/Data/Persons/{profession}.json";
        var person = File.ReadAllText(json).Deserialize<PersonDto>();
        person.Id = userId;

        return person;
    }

    public void Save(PersonDto person, DateTime? lastActive = null)
    {
        person.LastActive = lastActive ?? DateTime.Now;

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
    }

    public bool Exists(long userId)
    {
        var sql = $"SELECT * FROM Persons WHERE ID = {userId}";
        var data = DataBase.GetRows(sql);

        return data.Any();
    }
}

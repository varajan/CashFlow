using CashFlow.Data.DTOs;

namespace CashFlow.Interfaces;

public interface IPersonRepository
{
    PersonDto Get(long userId);
    PersonDto GetDefault(string profession, long userId);
    List<string> GetAllProfessions();
    List<PersonDto> GetAll();
    void Save(PersonDto person, DateTime? lastActive = null);
    void Delete(long userId);
    bool Exists(long userId);
}
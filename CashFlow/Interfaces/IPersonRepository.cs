using CashFlow.Data.DTOs;

namespace CashFlow.Interfaces;

public interface IPersonRepository
{
    PersonDto Get(long userId);
    List<PersonDto> GetAll();
    void Save(PersonDto person, DateTime? lastActive = null);
    void Delete(long userId);
    bool Exists(long userId);
}
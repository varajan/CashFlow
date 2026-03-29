using CashFlow.Data.DTOs;

namespace CashFlow.Interfaces;

public interface IUserRepository
{
    bool Exists(long userId);
    List<UserDto> GetAll();
    UserDto Get(long userId);
    void Save(UserDto user);
}

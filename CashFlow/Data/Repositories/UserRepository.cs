using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Data.Repositories;

public class UserRepository(IDataBase dataBase) : IUserRepository
{
    private IDataBase DataBase { get; } = dataBase;

    public bool Exists(long userId) => DataBase.GetColumn($"SELECT ID FROM Users WHERE ID = {userId}").Any();

    public List<UserDto> GetAll() => DataBase.GetColumn($"SELECT Data FROM Users").Select(data => data.Deserialize<UserDto>()).ToList();

    public UserDto Get(long userId)
    {
        var data = DataBase.GetValue($"SELECT Data FROM Users WHERE ID = {userId}");

        return string.IsNullOrEmpty(data)
            ? default
            : data.Deserialize<UserDto>();
    }

    public void Save(UserDto user)
    {
        if (!Exists(user.Id))
            DataBase.Execute($"INSERT INTO Users (ID, Data) VALUES ({user.Id}, '{user.Serialize()}')");
        else
            DataBase.Execute($"UPDATE Users SET DATA = '{user.Serialize()}' WHERE ID = {user.Id} ");
    }
}

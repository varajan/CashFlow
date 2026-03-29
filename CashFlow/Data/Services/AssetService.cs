using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Interfaces;

namespace CashFlow.Data.Services;

public class AssetService(IPersonRepository personRepository)
{
    private IPersonRepository PersonRepository { get; } = personRepository;

    public List<AssetDto> GetAll(AssetType type, UserDto user) => PersonRepository.Get(user.Id).Assets.Where(a => a.Type == type).ToList();

    public AssetDto Get(long id, UserDto user) => PersonRepository.Get(user.Id).Assets.First(a => a.Id == id);

    public void Create(UserDto user, AssetDto asset)
    {
        var person = PersonRepository.Get(user.Id);
        asset.Id = person.Assets.Any() ? person.Assets.Max(a => a.Id) + 1 : 1;
        person.Assets.Add(asset);
        PersonRepository.Save(person);
    }

    public void Delete(PersonDto person, AssetDto asset)
    {
        var index = person.Assets.FindIndex(a => a.Id == asset.Id);
        person.Assets[index].IsDeleted = true;
        PersonRepository.Save(person);
    }

    public void Delete(UserDto user, AssetDto asset)
    {
        var person = PersonRepository.Get(user.Id);
        var index = person.Assets.FindIndex(a => a.Id == asset.Id);
        person.Assets[index].IsDeleted = true;
        PersonRepository.Save(person);
    }

    public void Restore(PersonDto person, AssetDto asset)
    {
        var index = person.Assets.FindIndex(a => a.Id == asset.Id);
        person.Assets[index].IsDeleted = false;
        PersonRepository.Save(person);
    }

    public void Update(UserDto user, AssetDto asset)
    {
        var person = PersonRepository.Get(user.Id);
        var index = person.Assets.FindIndex(a => a.Id == asset.Id);
        person.Assets[index] = asset;

        PersonRepository.Save(person);
    }

    public void Sell(AssetDto asset, int price, UserDto user)
    {
        asset.SellPrice = price;
        asset.MarkedToSell = false;

        Update(user, asset);
        Delete(user, asset);
    }
}

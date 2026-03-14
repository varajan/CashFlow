using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;

namespace CashFlow.Data.Users.UserData.PersonData;

public class AssetService(IPersonRepository personRepository)
{
    private IPersonRepository PersonRepository { get; } = personRepository;

    public List<AssetDto> GetAll(AssetType type, IUser user) => PersonRepository.Get(user.Id).Assets.Where(a => a.Type == type).ToList();

    public AssetDto Get(long id, IUser user) => PersonRepository.Get(user.Id).Assets.First(a => a.Id == id);

    public void Create(IUser user, AssetDto asset)
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

    public void Delete(IUser user, AssetDto asset)
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

    public void Update(IUser user, AssetDto asset)
    {
        var person = PersonRepository.Get(user.Id);
        var index = person.Assets.FindIndex(a => a.Id == asset.Id);
        person.Assets[index] = asset;

        PersonRepository.Save(person);
    }

    public void Sell(AssetDto asset, ActionType action, int price, IUser user)
    {
        asset.SellPrice = price;
        asset.MarkedToSell = false;

        Update(user, asset);
        Delete(user, asset);
    }
}

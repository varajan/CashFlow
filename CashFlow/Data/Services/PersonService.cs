using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Interfaces;

namespace CashFlow.Data.Services;

public class PersonService(IPersonRepository personRepository, IDataBase dataBase, ITranslationService terms) : IPersonService
{
    private IPersonRepository PersonRepository { get; } = personRepository;
    private ITranslationService Terms { get; } = terms;
    private AssetService AssetService => new(PersonRepository);
    private HistoryService HistoryService => new(dataBase, PersonRepository, PersonDescriptionService, Terms);
    private DescriptionService PersonDescriptionService => new(Terms, AssetService);


    public bool Exists(UserDto user) => PersonRepository.Exists(user.Id);
    public void Update(PersonDto person) => PersonRepository.Save(person);
    public PersonDto Read(UserDto user) => PersonRepository.Get(user.Id);
    public void Delete(UserDto user) => PersonRepository.Delete(user.Id);

    public void Create(string profession, UserDto user)
    {
        profession = Terms.Translate(profession, user.Language, Language.EN);
        var person = PersonRepository.GetDefault(profession, user.Id);

        if (PersonRepository.Exists(user.Id))
            PersonRepository.Delete(user.Id);

        PersonRepository.Save(person);
    }

    public List<string> GetAllProfessions() => PersonRepository.GetAllProfessions();

    public string GetDescription(UserDto user, bool compact = true)
    {
        var person = PersonRepository.Get(user.Id);
        return PersonDescriptionService.GetDescription(user, person, compact);
    }

    public void Update(UserDto user, LiabilityDto liability)
    {
        var person = PersonRepository.Get(user.Id);
        var index = person.Liabilities.FindIndex(l => l.Name == liability.Name);
        person.Liabilities[index] = liability;
        PersonRepository.Save(person);
    }


    public void AddHistory(ActionType type, long value, UserDto user) => HistoryService.AddRecord(type, value, user, 0);
    public void AddHistory(ActionType type, long value, UserDto user, long assetId) => HistoryService.AddRecord(type, value, user, assetId);
    public bool IsHistoryEmpty(UserDto user) => HistoryService.IsHistoryEmpty(user.Id);
    public List<HistoryDto> ReadHistory(UserDto user) => HistoryService.ReadHistory(user.Id);
    public string HistoryTopFive(UserDto user, UserDto currentUser) => HistoryService.GetTopFive(user, currentUser);
    public void RollbackHistory(PersonDto person, HistoryDto record) => HistoryService.RollbackRecord(person, record);


    public List<AssetDto> ReadActiveAssets(AssetType type, UserDto user) =>
        AssetService.GetAll(type, user).Where(x => !x.IsDeleted).ToList();

    public void CreateAsset(UserDto user, AssetDto asset) => AssetService.Create(user, asset);
    public void DeleteAsset(UserDto user, AssetDto asset) => AssetService.Delete(user, asset);
    public void UpdateAsset(UserDto user, AssetDto asset) => AssetService.Update(user, asset);
    public void SellAsset(AssetDto asset, int price, UserDto user) => AssetService.Sell(asset, price, user);
    public string GetAssetDescription(AssetDto asset, UserDto user) => PersonDescriptionService.GetAssetDescription(asset, user);
}

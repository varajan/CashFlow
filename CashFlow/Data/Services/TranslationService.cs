using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Data.Services;

public class TranslationService : ITranslationService
{
    private readonly Dictionary<Language, Dictionary<string, string>> Translations = [];

    private Dictionary<string, string> GetDictionary(Language language)
    {
        var translationsPath = $"{AppDomain.CurrentDomain.BaseDirectory}/Data/Translations/{language}.json";
        
        if (Translations.ContainsKey(language))
            return Translations[language];

        var dictionary = File.ReadAllText(translationsPath).Deserialize<Dictionary<string, string>>();
        Translations[language] = dictionary;

        return dictionary;
    }

    public string Get(string key, UserDto user, params object[] args) => Get(key, user.Language, args);

    public string Get(string key, Language language = Language.EN, params object[] args)
    {
        var term = GetDictionary(language)[key];
        return string.Format(term, args);
    }
}

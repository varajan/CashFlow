using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Interfaces;
using MoreLinq;
using System.Reflection;

namespace CashFlow.Data.Services;

public class TranslationService : ITranslationService
{
    private readonly Dictionary<Language, Dictionary<string, string>> Translations = [];

    private List<string> _noTranslationNeeded;
    public List<string> NoTranslationNeeded => _noTranslationNeeded ??= typeof(Terms)
        .GetFields()
        .Where(f => f.GetCustomAttribute<NoTranslationNeededAttribute>() is not null)
        .SelectMany(f =>
        {
            var value = f.GetValue(null);

            return value switch
            {
                string s => [s],
                IEnumerable<string> collection => collection.Where(v => v is not null)!,
                _ => []
            };
        })
        .ToList();

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
        if (NoTranslationNeeded.Contains(key)) return key;

        var dictionary = GetDictionary(language);
        var term = dictionary[key];
        return string.Format(term, args);
    }

    public string Translate(string value, Language source, Language target, params object[] args)
    {
        var key = Translations[source].FirstOrDefault(x => x.Value == value).Key;
        return Get(key, target, args);
    }
}

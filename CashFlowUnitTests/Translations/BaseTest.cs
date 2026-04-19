using CashFlow.Data;
using CashFlow.Data.Consts;
using System.Reflection;

namespace CashFlowUnitTests.Translations;

public class BaseTest
{
    public static IEnumerable<Language> Languages => Enum.GetValues<Language>();

    public static readonly List<string> TermValues = typeof(Terms)
        .GetFields()
        .Where(f => f.GetCustomAttribute<NoTranslationNeededAttribute>() is null)
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
}

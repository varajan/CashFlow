using CashFlow.Data.Consts;
using CashFlow.Data.Consts.Terms;
using System.Reflection;

namespace CashFlowUnitTests.Translations;

public class BaseTest
{
    public static IEnumerable<Language> Languages => Enum.GetValues<Language>();

    public static readonly List<string?> TermValues = typeof(Terms)
        .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
        .Where(f => f.IsLiteral && !f.IsInitOnly)
        .Select(f => (string?)f.GetValue(null))
        .Where(v => v is not null)
        .ToList();
}

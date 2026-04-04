using CashFlow.Data.Consts;
using System.Reflection;

namespace CashFlowUnitTests.Translations;

public class BaseTest
{
    protected readonly List<string?> TermValues = typeof(Terms)
        .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
        .Where(f => f.IsLiteral && !f.IsInitOnly)
        .Select(f => (string?)f.GetValue(null))
        .Where(v => v is not null)
        .ToList();
}

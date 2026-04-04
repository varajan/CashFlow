using CashFlow.Data.Consts;
using CashFlow.Extensions;
using System.Reflection;

namespace CashFlowUnitTests.Translations;

[TestFixture]
public class LiabilityTests
{
    public static IEnumerable<Liability> Liabilities => Enum.GetValues<Liability>();

    private readonly List<string?> TermValues = typeof(Terms)
        .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
        .Where(f => f.IsLiteral && !f.IsInitOnly)
        .Select(f => (string?)f.GetValue(null))
        .Where(v => v is not null)
        .ToList();

    [TestCaseSource(nameof(Liabilities))]
    public void Liability_has_translation(Liability liability) =>
        Assert.That(TermValues, Does.Contain(liability.GetDescription()));

    [Test]
    public void Liabilities_have_distinct_translations() =>
        Assert.That(Liabilities.Select(at => at.GetDescription()), Is.Unique);
}

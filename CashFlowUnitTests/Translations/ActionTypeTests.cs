using CashFlow.Data.Consts;
using CashFlow.Extensions;
using System.Reflection;

namespace CashFlowUnitTests.Translations;

[TestFixture]
public class ActionTypeTests
{
    public static IEnumerable<ActionType> ActionTypes => Enum.GetValues<ActionType>();

    private readonly List<string?> TermValues = typeof(Terms)
        .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
        .Where(f => f.IsLiteral && !f.IsInitOnly)
        .Select(f => (string?)f.GetValue(null))
        .Where(v => v is not null)
        .ToList();

    [TestCaseSource(nameof(ActionTypes))]
    public void ActionType_has_translation(ActionType actionType) =>
        Assert.That(TermValues, Does.Contain(actionType.GetDescription()));

    [Test]
    public void ActionTypes_have_distinct_translations() =>
        Assert.That(ActionTypes.Select(at => at.GetDescription()), Is.Unique);
}

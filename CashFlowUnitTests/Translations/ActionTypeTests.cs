using CashFlow.Data.Consts;
using CashFlow.Extensions;

namespace CashFlowUnitTests.Translations;

[TestFixture]
public class ActionTypeTests : BaseTest
{
    public static IEnumerable<ActionType> ActionTypes => Enum.GetValues<ActionType>();

    [TestCaseSource(nameof(ActionTypes))]
    public void ActionType_has_translation(ActionType actionType) =>
        Assert.That(TermValues, Does.Contain(actionType.GetDescription()));

    [Test]
    public void ActionTypes_have_distinct_translations() =>
        Assert.That(ActionTypes.Select(at => at.GetDescription()), Is.Unique);
}

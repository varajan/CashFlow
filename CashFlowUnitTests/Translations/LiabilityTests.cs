using CashFlow.Data.Consts;
using CashFlow.Extensions;

namespace CashFlowUnitTests.Translations;

[TestFixture]
public class LiabilityTests : BaseTest
{
    public static IEnumerable<Liability> Liabilities => Enum.GetValues<Liability>();

    [TestCaseSource(nameof(Liabilities))]
    public void Liability_has_translation(Liability liability) =>
        Assert.That(TermValues, Does.Contain(liability.GetDescription()));

    [Test]
    public void Liabilities_have_distinct_translations() =>
        Assert.That(Liabilities.Select(at => at.GetDescription()), Is.Unique);
}

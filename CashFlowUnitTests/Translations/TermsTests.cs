namespace CashFlowUnitTests.Translations;

[TestFixture]
public class TermsTests : BaseTest
{
    [Test]
    public void Terms_have_distinct_values() => Assert.That(TermValues, Is.Unique);
}

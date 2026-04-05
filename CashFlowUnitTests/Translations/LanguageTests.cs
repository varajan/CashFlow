using CashFlow.Data.Consts;
using CashFlow.Data.Services;

namespace CashFlowUnitTests.Translations;

[TestFixtureSource(typeof(BaseTest), nameof(Languages))]
public class LanguageTests(Language language) : BaseTest
{
    private readonly TranslationService TranslationService = new();

    private readonly string[] Params = ["0", "1", "2", "3", "4"];

    [TestCaseSource(typeof(BaseTest), nameof(TermValues))]
    public void Language_has_all_translations(string term) =>
        Assert.That(TranslationService.Get(term, language, Params),
            Is.Not.Null.And.Not.Empty);
}

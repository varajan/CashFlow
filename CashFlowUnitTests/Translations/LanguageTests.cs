using CashFlow.Data.Consts;
using CashFlow.Data.Services;

namespace CashFlowUnitTests.Translations;

[TestFixtureSource(typeof(BaseTest), nameof(Languages))]
public class LanguageTests(Language language) : BaseTest
{
    private readonly TranslationService TranslationService = new();

    [TestCaseSource(typeof(BaseTest), nameof(TermValues))]
    public void Term_has_valid_translation(string term)
    {
        string[] Params = ["{0}", "{1}", "{2}", "{3}", "{4}"];
        var translation = TranslationService.Get(term, language, Params);
        var enParams = ExtractParams(term);
        var deParams = ExtractParams(translation);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(translation, Is.Not.Null.And.Not.Empty, $"Not translated: [{term}]");
            Assert.That(deParams, Is.EquivalentTo(enParams), $"Params don't match: [{term}]");
        }
    }

    private static List<string> ExtractParams(string text) =>
        Regex.Matches(text, @"\{\d+\}")
            .Select(m => m.Value)
            .Distinct()
            .ToList();
}

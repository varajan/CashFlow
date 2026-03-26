using CashFlow.Data.Consts;
using CashFlow.Stages;

namespace CashFlow.Tests.Stages;

[TestFixture]
public class ChooseLanguageTests : StagesBaseTest
{
    [Test, Ignore("Not applicable")]
    public override Task Stage_CanBeCanceled() => Task.CompletedTask;

    [Test]
    public async Task ChooseLanguage_CanNotBeCanceled([Values] bool personExists)
    {
        // Arrange
        var testStage = GetTestStage();
        var expected = personExists ? typeof(Start) : typeof(ChooseLanguage);

        PersonServiceMock.Setup(x => x.Exists(CurrentUser)).Returns(personExists);

        // Act
        await testStage.HandleMessage("cancel");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf(expected));
    }

    [Test]
    public void ChooseLanguage_CanSelectAnyLanguage([Values] bool personExists)
    {
        // Arrange
        var testStage = GetTestStage();
        var languages = new List<string> { "EN", "DE", "UA" };

        PersonServiceMock.Setup(x => x.Exists(CurrentUser)).Returns(personExists);
        if (personExists) languages.Add("Cancel");

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo("Language/Мова"));
            Assert.That(testStage.Buttons, Is.EqualTo(languages));
        });
    }

    [Test]
    public async Task ChooseLanguage_SelectInvalidLanguage_StayOnStage([Values] bool personExists)
    {
        // Arrange
        var testStage = GetTestStage();
        var expected = personExists ? typeof(Start) : typeof(ChooseLanguage);

        PersonServiceMock.Setup(x => x.Exists(CurrentUser)).Returns(personExists);

        // Act
        await testStage.HandleMessage("IT");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<ChooseLanguage>());
    }

    [Test]
    public async Task ChooseLanguage_SelectValidLanguage_StayOnStage([Values] bool personExists, [Values] Language language)
    {
        // Arrange
        var testStage = GetTestStage();
        PersonServiceMock.Setup(x => x.Exists(CurrentUser)).Returns(personExists);

        // Act
        await testStage.HandleMessage($"{language}");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());
    }

    protected override IStage GetTestStage() => GetStage<ChooseLanguage>();
}

using CashFlow.Stages;

namespace CashFlowUnitTests.Stages;

[TestFixture]
public class GameMenuTests : StagesBaseTest
{
    [Test]
    public void GameMenu_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act

        // Assert
        Assert.That(testStage.Message, Is.EqualTo("What do you want?"));
        Assert.That(testStage.Buttons, Is.EqualTo(new List<string> { "Stop Game", "Language/Мова", "Cancel" }));
    }

    [TestCase("Stop Game", typeof(StopGame))]
    [TestCase("Language/Мова", typeof(ChooseLanguage))]
    public async Task GameMenu_Select_ValidOption(string message, Type nextStage)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(message);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf(nextStage));
    }

    protected override IStage GetTestStage() => GetStage<GameMenu>();
}
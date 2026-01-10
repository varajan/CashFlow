using CashFlow.Stages;
using Moq;

namespace CashFlow.Tests.Stages;

[TestFixture]
public class StopGameTests : StagesBaseTest
{
    [Test]
    public void StopGame_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act

        // Assert
        Assert.That(testStage.Message, Is.EqualTo("Are you sure want to stop current game?"));
        Assert.That(testStage.Buttons, Is.EqualTo(new List<string> { "Yes", "Cancel" } ));
    }

    [Test]
    public async Task StopGame_Confirm()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("yes");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        PersonManagerMock.Verify(p => p.ClearHistory(CurrentUserMock.Object), Times.Once);
        PersonManagerMock.Verify(p => p.Delete(CurrentUserMock.Object), Times.Once);
    }

    [Test]
    public async Task StopGame_Dismiss([Values("no", "cancel")] string message)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(message);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        PersonManagerMock.Verify(p => p.ClearHistory(CurrentUserMock.Object), Times.Never);
        PersonManagerMock.Verify(p => p.Delete(CurrentUserMock.Object), Times.Never);
    }

    protected override IStage GetTestStage() => new StopGame(TermsServiceMock.Object, PersonManagerMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}
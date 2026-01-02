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

        HistoryManagerMock.Verify(h => h.Clear(CurrentUserMock.Object.Id), Times.Once);
        PersonManagerMock.Verify(p => p.Delete(CurrentUserMock.Object.Id), Times.Once);
        PersonManagerMock.Verify(a => a.DeleteAllAssets(CurrentUserMock.Object.Id), Times.Once);
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

        HistoryManagerMock.Verify(h => h.Clear(CurrentUserMock.Object.Id), Times.Never);
        AssetManagerMock.Verify(a => a.DeleteAll(CurrentUserMock.Object.Id), Times.Never);
        PersonManagerMock.Verify(p => p.Delete(CurrentUserMock.Object.Id), Times.Never);
    }

    protected override IStage GetTestStage() => new StopGame(
        TermsServiceMock.Object,
        PersonManagerMock.Object,
        AssetManagerMock.Object,
        HistoryManagerMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}
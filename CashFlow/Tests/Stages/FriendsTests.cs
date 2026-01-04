using CashFlow.Stages;
using Moq;

namespace CashFlow.Tests.Stages;

[TestFixture]
public class FriendsTests : StagesBaseTest
{
    [TestCase(true, true)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    public void Friends_Question_and_Buttons(bool onSmall, bool onBig)
    {
        // Arrange
        OtherUsers = OtherUsers.Where(u => u.IsActive && (u.Name.Contains("Big") == onBig || u.Name.Contains("Small") == onSmall)).ToList();

        var testStage = GetTestStage();
        var buttons = OtherUsers.Select(u => u.Name).Append("Cancel");

        // Act

        // Assert
        if (!onSmall) Assert.That(testStage.Message.Contains("Small"), Is.False);
        if (!onBig) Assert.That(testStage.Message.Contains("Big"), Is.False);

        Assert.That(testStage.Message.Contains("Big"), Is.EqualTo(onBig));
        Assert.That(testStage.Message.Contains("Small"), Is.EqualTo(onSmall));
        Assert.That(testStage.Buttons, Is.EqualTo(buttons));
    }

    [Test]
    public async Task Friends_SelectValidValue()
    {
        // Arrange
        var testStage = GetTestStage();
        var testUser = OtherUsers.First(u => u.IsActive);
        var description = $"{testUser.Name} description";
        var top5 = $"{testUser.Name} history";

        PersonManagerMock.Setup(p => p.GetDescription(testUser)).Returns(description);
        PersonManagerMock.Setup(p => p.HistoryTopFive(testUser, CurrentUserMock.Object)).Returns(top5);

        // Act
        await testStage.HandleMessage(testUser.Name.ToLower());

        // Assert
        CurrentUserMock.Verify(u => u.Notify(description), Times.Once);
        CurrentUserMock.Verify(u => u.Notify(top5), Times.Once);
    }

    [Test]
    public async Task Friends_SelectInvalidValue()
    {
        // Arrange
        var testStage = GetTestStage();
        var testUser = OtherUsers.First(u => !u.IsActive);

        // Act
        await testStage.HandleMessage(testUser.Name.ToLower());

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Friends>());
        CurrentUserMock.Verify(u => u.Notify(It.IsAny<string>()), Times.Never);
    }

    protected override IStage GetTestStage() => new Friends(TermsServiceMock.Object, PersonManagerMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}
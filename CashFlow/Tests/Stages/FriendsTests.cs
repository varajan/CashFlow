using CashFlow.Extensions;
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
        OtherUsers = OtherUsers.Where(u => u.IsActive() && (u.Name.Contains("Big") == onBig || u.Name.Contains("Small") == onSmall)).ToList();
        UserRepositoryMock.Setup(r => r.GetAll()).Returns(OtherUsers.Append(CurrentUser).ToList());

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
        var testUser = OtherUsers.First(u => u.IsActive());
        var description = $"{testUser.Name} description";
        var top5 = $"{testUser.Name} history";

        PersonServiceMock.Setup(p => p.GetDescription(testUser, true)).Returns(description);
        PersonServiceMock.Setup(p => p.HistoryTopFive(testUser, CurrentUser)).Returns(top5);

        // Act
        await testStage.HandleMessage(testUser.Name.ToLower());

        // Assert
        NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, description), Times.Once);
        NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, top5), Times.Once);
    }

    [Test]
    public async Task Friends_SelectInvalidValue()
    {
        // Arrange
        var testStage = GetTestStage();
        var testUser = OtherUsers.First(u => !u.IsActive());

        // Act
        await testStage.HandleMessage(testUser.Name.ToLower());

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Friends>());
        NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, It.IsAny<string>()), Times.Never);
    }

    protected override IStage GetTestStage() => GetStage<Friends>();
}
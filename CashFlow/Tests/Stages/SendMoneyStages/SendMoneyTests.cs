using CashFlow.Stages;
using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using Moq;

namespace CashFlow.Tests.Stages.SendMoneyStages;

[TestFixture]
public class SendMoneyTests : StagesBaseTest
{
    [Test]
    public async Task SendMoney_CanBeCanceled()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("Cancel");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());
    }

    [Test]
    public async Task SendMoney_SendToInactiveUser_NotFondMesage()
    {
        // Arrange
        var testUser = OtherUsers.First(u => !u.IsActive && u.Person.Circle == Circle.Small);
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(testUser.Name);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SendMoney>());

        CurrentUserMock.Verify(u => u.Notify("Not found."), Times.Once);
    }

    [Test]
    public async Task SendMoney_SendToBigCircleUser_NotFondMesage()
    {
        // Arrange
        var testUser = OtherUsers.First(u => u.IsActive && u.Person.Circle == Circle.Big);
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(testUser.Name);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SendMoney>());

        CurrentUserMock.Verify(u => u.Notify("Not found."), Times.Once);
    }

    [Test]
    public async Task SendMoney_MoveTo_SendMoneyTo_WhenSendToValidUser()
    {
        // Arrange
        var testUser = OtherUsers.First(u => u.IsActive && u.Person.Circle == Circle.Small);
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(testUser.Name);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SendMoneyAmount>());

        CurrentUserMock.Verify(u => u.Person.Assets.Add(testUser.Name, AssetType.Transfer, false), Times.Once);
    }

    [Test]
    public async Task SendMoney_MoveTo_SendMoneyTo_WhenSendToBank()
    {
        // Arrange
        var bank = "bank";
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(bank);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SendMoneyAmount>());

        CurrentUserMock.Verify(u => u.Person.Assets.Add(bank, AssetType.Transfer, false), Times.Once);
    }

    [Test]
    public void SendMoney_CanSendToActiveUsersOnSmallCircle()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo("Whom?"));
            Assert.That(testStage.Buttons, Is.EqualTo(new List<string>
            {
                "1st Active on Small Circle",
                "2nd Active on Small Circle",
                "Bank",
                "Cancel"
            }));
        });
    }

    [Test]
    public void SendMoney_NoOtherUsers_CanSendToBankOnly()
    {
        // Arrange
        OtherUsers = [];
        var testStage = GetTestStage();

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo("Whom?"));
            Assert.That(testStage.Buttons, Is.EqualTo(new List<string> { "Bank", "Cancel" }));
        });
    }

    [Test]
    public void SendMoney_NoOtherActiveUsers_CanSendToBankOnly()
    {
        // Arrange
        OtherUsers = OtherUsers.Where(x => !x.IsActive).ToList();
        var testStage = GetTestStage();

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo("Whom?"));
            Assert.That(testStage.Buttons, Is.EqualTo(new List<string> { "Bank", "Cancel" }));
        });
    }

    [Test]
    public void SendMoney_NoOthersOnSmallCircle_CanSendToBankOnly()
    {
        // Arrange
        OtherUsers = OtherUsers.Where(x => x.Person.Circle == Circle.Big).ToList();
        var testStage = GetTestStage();

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo("Whom?"));
            Assert.That(testStage.Buttons, Is.EqualTo(new List<string> { "Bank", "Cancel" }));
        });
    }

    private SendMoney GetTestStage() => new(OtherUsers, CurrentUserMock.Object, TermsServiceMock.Object, LoggerMock.Object, AssetsMock.Object);
}

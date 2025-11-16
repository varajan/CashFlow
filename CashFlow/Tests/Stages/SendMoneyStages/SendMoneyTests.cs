using CashFlow.Stages;
using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using Moq;
using CashFlow.Data.DTOs;
using CashFlow.Stages.SmallCircleStages.SendMoneyStages;

namespace CashFlow.Tests.Stages.SendMoneyStages;

[TestFixture]
public class SendMoneyTests : StagesBaseTest
{
    private AssetDto Asset => new() { UserId = CurrentUserMock.Object.Id, Type = AssetType.Transfer, IsDraft = true };

    [SetUp]
    public void Setup()
    {
        AssetManagerMock.Setup(a => a.ReadAll(AssetType.Transfer, CurrentUserMock.Object.Id)).Returns([Asset]);
    }

    [Test]
    public async Task SendMoney_SendToInactiveUser_NotFondMesage()
    {
        // Arrange
        var testUser = OtherUsers.First(u => !u.IsActive && u.Person_OBSOLETE.Circle == Circle.Small);
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(testUser.Name);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SendMoney>());

        CurrentUserMock.Verify(u => u.Notify("Not found."), Times.Once);
        PersonManagerMock.Verify(x => x.Update(It.IsAny<PersonDto>()), Times.Never, "No person data should be updated");
        AssetManagerMock.Verify(x => x.Update(It.IsAny<AssetDto>()), Times.Never, "No asset should be updated");
    }

    [Test]
    public async Task SendMoney_SendToBigCircleUser_NotFondMesage()
    {
        // Arrange
        var testUser = OtherUsers.First(u => u.IsActive && u.Person_OBSOLETE.Circle == Circle.Big);
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(testUser.Name);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SendMoney>());

        CurrentUserMock.Verify(u => u.Notify("Not found."), Times.Once);
        PersonManagerMock.Verify(x => x.Update(It.IsAny<PersonDto>()), Times.Never, "No person data should be updated");
    }

    [Test]
    public async Task SendMoney_MoveTo_SendMoneyTo_WhenSendToValidUser()
    {
        // Arrange
        var testUser = OtherUsers.First(u => u.IsActive && u.Person_OBSOLETE.Circle == Circle.Small);
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(testUser.Name);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SendMoneyAmount>());

        AssetManagerMock.Verify(a => a.Create(
            It.Is<AssetDto>(x =>
                x.Title == testUser.Name &&
                x.UserId == CurrentUserMock.Object.Id &&
                x.Type == AssetType.Transfer)
        ), Times.Once);

        PersonManagerMock.Verify(x => x.Update(It.IsAny<PersonDto>()), Times.Never, "No person data should be updated");
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

        AssetManagerMock.Verify(a => a.Create(
            It.Is<AssetDto>(x =>
                x.Title == bank &&
                x.UserId == CurrentUserMock.Object.Id &&
                x.Type == AssetType.Transfer)
        ), Times.Once);
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
        OtherUsers = OtherUsers.Where(x => x.Person_OBSOLETE.Circle == Circle.Big).ToList();
        var testStage = GetTestStage();

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo("Whom?"));
            Assert.That(testStage.Buttons, Is.EqualTo(new List<string> { "Bank", "Cancel" }));
        });
    }

    protected override IStage GetTestStage() => new SendMoney(AssetManagerMock.Object, TermsServiceMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}

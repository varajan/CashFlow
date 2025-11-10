using CashFlowBot.Data.Users;
using CashFlowBot.Data;
using CashFlowBot.Stages;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using CashFlowBot.Loggers;
using CashFlowBot.Data.Users.UserData.PersonData;
using System.Linq;
using CashFlowBot.Data.Consts;

namespace CashFlowBot.Tests.Stages;

[TestFixture]
public class SendMoneyTests
{
    private Mock<IUser> _currentUserMock;
    private Mock<ITermsService> _termsServiceMock;
    private Mock<ILogger> _loggerMock;
    private Mock<IAvailableAssets> _assetsMock;
    private List<IUser> _otherUsers;

    [SetUp]
    public void SetUp()
    {
        _currentUserMock = GetUserMock("Myself", true, Circle.Small);
        _termsServiceMock = new Mock<ITermsService>();
        _loggerMock = new Mock<ILogger>();
        _assetsMock = new Mock<IAvailableAssets>();
        _otherUsers =
            [
                GetUserMock("1st Active on Small Circle", true, Circle.Small).Object,
                GetUserMock("1st Active on Big Circle", true, Circle.Big).Object,
                GetUserMock("1st Inactive on Small Circle", false, Circle.Small).Object,
                GetUserMock("1st Inactive on Big Circle", false, Circle.Big).Object,
                GetUserMock("2nd Active on Small Circle", true, Circle.Small).Object,
                GetUserMock("2nd Active on Big Circle", true, Circle.Big).Object,
                GetUserMock("2nd Inactive on Small Circle", false, Circle.Small).Object,
                GetUserMock("2nd Inactive on Big Circle", false, Circle.Big).Object,
            ];

        _termsServiceMock
            .Setup(t => t.Get(It.IsAny<int>(), It.IsAny<IUser>(), It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns((int id, IUser user, string defaultValue, object[] args) => defaultValue);
    }

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
        var testUser = _otherUsers.First(u => !u.IsActive && u.Person.Circle == Circle.Small);
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(testUser.Name);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SendMoney>());

        _currentUserMock.Verify(
            u => u.Notify("Not found."),
            Times.Once,
            "Expected CurrentUser.Notify to be called once with 'Not found.' when name is invalid."
        );
    }
    
    [Test]
    public async Task SendMoney_SendToBigCircleUser_NotFondMesage()
    {
        // Arrange
        var testUser = _otherUsers.First(u => u.IsActive && u.Person.Circle == Circle.Big);
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(testUser.Name);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SendMoney>());

        _currentUserMock.Verify(
            u => u.Notify("Not found."),
            Times.Once,
            "Expected CurrentUser.Notify to be called once with 'Not found.' when name is invalid."
        );
    }

    [Test]
    public async Task SendMoney_MoveTo_SendMoneyTo_WhenSendToValidUser()
    {
        // Arrange
        var testUser = _otherUsers.First(u => u.IsActive && u.Person.Circle == Circle.Small);
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(testUser.Name);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SendMoneyTo>());

        _currentUserMock.Verify(
            u => u.Person.Assets.Add(testUser.Name, AssetType.Transfer, false),
            Times.Once,
            "Expected Assets.Add(message, AssetType.Transfer) to be called once for valid user."
        );
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
        Assert.That(testStage.NextStage, Is.TypeOf<SendMoneyTo>());

        _currentUserMock.Verify(
            u => u.Person.Assets.Add(bank, AssetType.Transfer, false),
            Times.Once,
            "Expected Assets.Add(message, AssetType.Transfer) to be called once for valid user."
        );
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
        _otherUsers = [];
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
        _otherUsers = _otherUsers.Where(x => !x.IsActive).ToList();
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
        _otherUsers = _otherUsers.Where(x => x.Person.Circle == Circle.Big).ToList();
        var testStage = GetTestStage();

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo("Whom?"));
            Assert.That(testStage.Buttons, Is.EqualTo(new List<string> { "Bank", "Cancel" }));
        });
    }

    private SendMoney GetTestStage() => new( _otherUsers, _currentUserMock.Object, _termsServiceMock.Object, _loggerMock.Object, _assetsMock.Object);

    private static Mock<IUser> GetUserMock(string name, bool isActive, Circle cirle)
    {
        var user = new Mock<IUser>();
        var assets = new Mock<IAssets>();
        var person = new Mock<IPerson>();

        person.SetupGet(p => p.Circle).Returns(cirle);
        person.SetupGet(p => p.Assets).Returns(assets.Object);

        user.SetupGet(u => u.IsActive).Returns(isActive);
        user.SetupGet(u => u.Name).Returns(name);
        user.SetupGet(u => u.Person).Returns(person.Object);

        return user;
    }
}

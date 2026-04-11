using CashFlow.Stages;
using CashFlow.Data.Consts;
using Moq;
using CashFlow.Data.DTOs;
using CashFlow.Stages.SmallCircleStages.SendMoneyStages;

namespace CashFlowUnitTests.Stages.SmallCircleTests.SendMoneyStages;

[TestFixture]
public class SendMoneyTests : StagesBaseTest
{
    private AssetDto Asset => new() { UserId = CurrentUser.Id, Type = AssetType.Transfer, IsDraft = true };

    [SetUp]
    public void Setup() => PersonServiceMock.Setup(a => a.ReadAllAssets(AssetType.Transfer, CurrentUser)).Returns([Asset]);

    [Test]
    public async Task SendMoney_SendToInactiveUser_NotFondMesage()
    {
        // Arrange
        var testUser = OtherUsers.First(u => u.Name.Contains("Inactive") && u.Name.Contains("Small"));
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(testUser.Name);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SendMoney>());

        NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, "Not found"), Times.Once);
        PersonServiceMock.Verify(x => x.Update(It.IsAny<PersonDto>()), Times.Never, "No person data should be updated");
        PersonServiceMock.Verify(x => x.UpdateAsset(CurrentUser, It.IsAny<AssetDto>()), Times.Never, "No asset should be updated");
    }

    [Test]
    public async Task SendMoney_SendToBigCircleUser_NotFondMesage()
    {
        // Arrange
        var testUser = OtherUsers.First(u => u.Name.Contains("Active") && u.Name.Contains("Big"));
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(testUser.Name);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SendMoney>());

        NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, "Not found"), Times.Once);
        PersonServiceMock.Verify(x => x.Update(It.IsAny<PersonDto>()), Times.Never, "No person data should be updated");
    }

    [Test]
    public async Task SendMoney_MoveTo_SendMoneyTo_WhenSendToValidUser()
    {
        // Arrange
        var testUser = OtherUsers.First(u => u.Name.Contains("Active") && u.Name.Contains("Small"));
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(testUser.Name);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SendMoneyAmount>());

        PersonServiceMock.Verify(a => a.CreateAsset(
            CurrentUser,
            It.Is<AssetDto>(x =>
                x.Title == testUser.Name &&
                x.UserId == CurrentUser.Id &&
                x.Type == AssetType.Transfer)
        ), Times.Once);

        PersonServiceMock.Verify(x => x.Update(It.IsAny<PersonDto>()), Times.Never, "No person data should be updated");
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

        PersonServiceMock.Verify(a => a.CreateAsset(
            CurrentUser,
            It.Is<AssetDto>(x =>
                x.Title == bank &&
                x.UserId == CurrentUser.Id &&
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
        using (Assert.EnterMultipleScope())
        {
            Assert.That(testStage.Message, Is.EqualTo("Whom?"));
            Assert.That(testStage.Buttons, Is.EqualTo(new List<string>
            {
                "1st Active on Small Circle",
                "2nd Active on Small Circle",
                "Bank",
                "Cancel"
            }));
        }
    }

    [Test]
    public void SendMoney_NoOtherUsers_CanSendToBankOnly()
    {
        // Arrange
        OtherUsers = [];
        UserRepositoryMock.Setup(r => r.GetAll()).Returns(OtherUsers.Append(CurrentUser).ToList());

        var testStage = GetTestStage();

        // Act

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(testStage.Message, Is.EqualTo("Whom?"));
            Assert.That(testStage.Buttons, Is.EqualTo(new List<string> { "Bank", "Cancel" }));
        }
    }

    [Test]
    public void SendMoney_NoOtherActiveUsers_CanSendToBankOnly()
    {
        // Arrange
        OtherUsers = OtherUsers.Where(x => x.Name.Contains("Inactive")).ToList();
        UserRepositoryMock.Setup(r => r.GetAll()).Returns(OtherUsers.Append(CurrentUser).ToList());

        var testStage = GetTestStage();

        // Act

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(testStage.Message, Is.EqualTo("Whom?"));
            Assert.That(testStage.Buttons, Is.EqualTo(new List<string> { "Bank", "Cancel" }));
        }
    }

    [Test]
    public void SendMoney_NoOthersOnSmallCircle_CanSendToBankOnly()
    {
        // Arrange
        OtherUsers = OtherUsers.Where(x => x.Name.Contains("Big")).ToList();
        UserRepositoryMock.Setup(r => r.GetAll()).Returns(OtherUsers.Append(CurrentUser).ToList());

        var testStage = GetTestStage();

        // Act

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(testStage.Message, Is.EqualTo("Whom?"));
            Assert.That(testStage.Buttons, Is.EqualTo(new List<string> { "Bank", "Cancel" }));
        }
    }

    protected override IStage GetTestStage() => GetStage<SendMoney>();
}

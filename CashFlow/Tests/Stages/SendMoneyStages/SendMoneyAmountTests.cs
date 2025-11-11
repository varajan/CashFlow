using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Data.Users.UserData.HistoryData;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Stages;
using Moq;

namespace CashFlow.Tests.Stages.SendMoneyStages;

[TestFixture]
public class SendMoneyAmountTests : StagesBaseTest
{
    [SetUp]
    public void Setup()
    {
        var transferAsset = new AssetDto
        {
            UserId = CurrentUserMock.Object.Id,
            Type = AssetType.Transfer
        };
        AssetManagerMock.Setup(a => a.Read(AssetType.Transfer, CurrentUserMock.Object.Id)).Returns(transferAsset);
    }

    [Test]
    public async Task SendMoneyAmount_CanBeCanceled()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("cancel");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        AssetManagerMock.Verify(a => a.Delete(
            It.Is<AssetDto>(x =>
                x.UserId == CurrentUserMock.Object.Id &&
                x.Type == AssetType.Transfer)
        ), Times.Once);
    }

    [Test]
    public void SendMoneyAmount_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo("How much?"));
            Assert.That(testStage.Buttons, Is.EqualTo(new List<string>
            {
                "$500",
                "$1,000",
                "$1,500",
                "$2,000",
                "$2,500",
                "$3,000",
                "$3,500",
                "$4,000",
                "Cancel"
            }));
        });
    }

    [TestCase("0")]
    [TestCase("a")]
    [TestCase("12a")]
    public async Task SendMoneyAmount_InvalidAmount(string amount)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(amount);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SendMoneyAmount>());
        CurrentUserMock.Verify(u => u.Notify("Invalid value. Try again."), Times.Once);
    }

    [Test]
    public async Task SendMoneyAmount_AvailableAmount_TransferIsCompleted()
    {
        // Arrange
        var testStage = GetTestStage();
        var assetsMock = new Mock<IAssets>();
        var historyMock = new Mock<IHistory>();
        var personMock = new Mock<IPerson>();

        personMock.SetupGet(p => p.Cash).Returns(100);
        personMock.SetupGet(p => p.Assets).Returns(assetsMock.Object);

        CurrentUserMock.SetupGet(u => u.Person).Returns(personMock.Object);
        CurrentUserMock.SetupGet(u => u.History).Returns(historyMock.Object);

        // Act
        await testStage.HandleMessage("100");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        AssetManagerMock.Verify(a => a.Write(
            It.Is<AssetDto>(x =>
                x.UserId == CurrentUserMock.Object.Id &&
                x.Qtty == 100 &&
                x.Type == AssetType.Transfer)
        ), Times.Once);

        //CurrentUserMock.Verify(u => u.Notify("Invalid value. Try again."), Times.Once);
    }

    [Test]
    public async Task SendMoneyAmount_UnavailableAmount_MoveToCredit()
    {
        // Arrange
        var testStage = GetTestStage();
        var personMock = new Mock<IPerson>();

        personMock.SetupGet(p => p.Cash).Returns(100);
        CurrentUserMock.SetupGet(u => u.Person).Returns(personMock.Object);

        // Act
        await testStage.HandleMessage("101");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SendMoneyCredit>());
    }

    protected override IStage GetTestStage() => new SendMoneyAmount(AssetManagerMock.Object, TermsServiceMock.Object, AssetsMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsersMock);
}

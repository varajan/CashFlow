using CashFlow.Data.Users.UserData.HistoryData;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Stages;
using Moq;

namespace CashFlow.Tests.Stages.SendMoneyStages;

public class SendMoneyAmountTests : StagesBaseTest
{
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
    public async Task SendMoneyAmount_AvailableAmount_Send()
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
        Assert.That(testStage.NextStage, Is.TypeOf<SendMoneyToWithCredit>());
    }

    private SendMoneyAmount GetTestStage() => new(OtherUsers, CurrentUserMock.Object, TermsServiceMock.Object, LoggerMock.Object, AssetsMock.Object);
}

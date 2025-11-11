using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.HistoryData;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Stages;
using Moq;

namespace CashFlow.Tests.Stages.SendMoneyStages;

[TestFixture]
public class SendMoneyCreditTests : StagesBaseTest
{
    [Test]
    public void SendMoneyCredit_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();
        var personMock = new Mock<IPerson>();
        var assetsMock = new Mock<IAssets>();

        // protected Asset TransferAsset => CurrentUser.Person.Assets.Get(AssetType.Transfer);
        assetsMock.Setup(a => a.Get(AssetType.Transfer)).Returns(new Asset_OLD(default, default, default));
        personMock.Setup(p => p.Cash).Returns(100);
        personMock.Setup(p => p.Assets).Returns(assetsMock.Object);
        CurrentUserMock.Setup(p => p.Person).Returns(personMock.Object);

        // SETUP: asset price: 1000

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo("You don''t have $1,000, but only $100"));
            Assert.That(testStage.Buttons, Is.EqualTo(new List<string> { "Get Credit", "Cancel" }));
        });
    }

    [Test]
    public async Task SendMoneyCredit_InvalidInput_IsIgnored()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("hello-world");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SendMoneyCredit>());
    }

    [Test]
    public async Task SendMoneyCredit_Confirmed_TransferIsCompleted()
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
        await testStage.HandleMessage("Get credit");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());
        // COPY FROM SendMoneyAmount_AvailableAmount_TransferIsCompleted
        //CurrentUserMock.Verify(u => u.Notify("Invalid value. Try again."), Times.Once);
    }

    protected override SendMoneyCredit GetTestStage() => new(OtherUsers, CurrentUserMock.Object, TermsServiceMock.Object, LoggerMock.Object, AssetsMock.Object);
}

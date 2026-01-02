using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Data.Users.UserData.HistoryData;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Extensions;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.SendMoneyStages;
using Moq;
using MoreLinq;

namespace CashFlow.Tests.Stages.BigCircleTests;


[TestFixture]
public class SendMoneyTests : StagesBaseTest
{
    private const int Cash = 100_000;
    private PersonDto TestPerson => new() { Id = CurrentUserMock.Object.Id, Cash = Cash, BigCircle = true };
    private AssetDto TransferAsset => new() { UserId = CurrentUserMock.Object.Id, Title = "Bank", Type = AssetType.Transfer, IsDraft = true };

    [SetUp]
    public void Setup()
    {
        PersonManagerMock.Setup(a => a.ReadAllAssets(AssetType.Transfer, TestPerson.Id)).Returns([TransferAsset]);
        PersonManagerMock.Setup(p => p.Read(TestPerson.Id)).Returns(TestPerson);
        AvailableAssetsMock.Setup(a => a.GetAsCurrency(AssetType.BigGiveMoney)).Returns(["$100,000", "$200,000"]);
    }

    [Test]
    public async Task SendMoney_CanBeCanceled()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("cancel");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        PersonManagerMock.Verify(a => a.DeleteAsset(
            It.Is<AssetDto>(x =>
                x.UserId == CurrentUserMock.Object.Id &&
                x.Type == AssetType.Transfer)
        ), Times.Once);

        PersonManagerMock.Verify(x => x.Update(It.IsAny<PersonDto>()), Times.Never, "No person data should be updated");
    }

    [Test]
    public void SendMoney_Question_and_Buttons()
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
                "$100,000",
                "$200,000",
                "Cancel"
            }));
        });
    }

    [TestCase("0")]
    [TestCase("a")]
    [TestCase("12a")]
    public async Task SendMoney_InvalidAmount(string amount)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(amount);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SendMoneyAmount>());
        CurrentUserMock.Verify(x => x.Notify("Invalid value. Try again."), Times.Once, "Message to user should be sent");
        AssetManagerMock.Verify(x => x.Update(It.IsAny<AssetDto>()), Times.Never, "No asset should be updated");
        PersonManagerMock.Verify(x => x.Update(It.IsAny<PersonDto>()), Times.Never, "No person data should be updated");
    }

    [Test]
    public async Task SendMoney_AvailableAmount_TransferIsCompleted()
    {
        // Arrange
        var transferAsset = TransferAsset;
        transferAsset.Title = "Bank";
        PersonManagerMock.Setup(a => a.ReadAllAssets(AssetType.Transfer, TestPerson.Id)).Returns([transferAsset]);

        var transferAmount = 100;
        var message = string.Format("{0} transferred {2} to {1}.", CurrentUserMock.Object.Name, "Bank", transferAmount.AsCurrency());
        var activeUsers = OtherUsers.Where(u => u.IsActive).Select(u => Mock.Get(u)).Append(CurrentUserMock);

        var testStage = GetTestStage();
        var historyMock = new Mock<IHistory>();

        // Act
        await testStage.HandleMessage($"{transferAmount}");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        AvailableAssetsMock.Verify(a => a.Add(It.IsAny<int>(), AssetType.BigGiveMoney), Times.Once);

        PersonManagerMock.Verify(a => a.UpdateAsset(
            It.Is<AssetDto>(x =>
                x.UserId == TestPerson.Id &&
                x.Qtty == transferAmount &&
                x.Type == AssetType.Transfer)
        ), Times.Once);

        PersonManagerMock.Verify(a => a.DeleteAsset(
            It.Is<AssetDto>(x =>
                x.UserId == TestPerson.Id &&
                x.Type == AssetType.Transfer)
        ), Times.Once);

        PersonManagerMock.Verify(p => p.Update(
            It.Is<PersonDto>(x =>
            x.Id == TestPerson.Id &&
            x.Cash == TestPerson.Cash - transferAmount)
            ), Times.Once);

        HistoryManagerMock.Verify(x => x.Add(ActionType.PayMoney, transferAmount, CurrentUserMock.Object), Times.Once);

        activeUsers.ForEach(u => u.Verify(u => u.Notify(message), Times.Once));
    }

    [Test]
    public async Task SendMoney_UnavailableAmount_NotCompleted()
    {
        // Arrange
        var transferAmount = Cash + 1;
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage($"{transferAmount}");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        PersonManagerMock.Verify(a => a.DeleteAsset(
        It.Is<AssetDto>(x =>
                x.UserId == TestPerson.Id &&
                x.Qtty == transferAmount &&
                x.Type == AssetType.Transfer)
            ), Times.Once);

        PersonManagerMock.Verify(x => x.Update(It.IsAny<PersonDto>()), Times.Never, "No person data should be updated");
        CurrentUserMock.Verify(x => x.Notify("You don't have enough money."), Times.Once);
    }

    protected override IStage GetTestStage() => new SendMoneyAmount(
        AssetManagerMock.Object,
        PersonManagerMock.Object,
        HistoryManagerMock.Object,
        TermsServiceMock.Object,
        AvailableAssetsMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}

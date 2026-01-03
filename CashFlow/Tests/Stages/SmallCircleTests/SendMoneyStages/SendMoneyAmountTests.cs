using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Data.Users;
using CashFlow.Data.Users.UserData.HistoryData;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Extensions;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.SendMoneyStages;
using Moq;
using MoreLinq;

namespace CashFlow.Tests.Stages.SmallCircleTests.SendMoneyStages;

[TestFixture]
public class SendMoneyAmountTests : StagesBaseTest
{
    private IUser Recipient => OtherUsers.Last(u => u.IsActive && u.Person_OBSOLETE.Circle == Circle.Small);
    private PersonDto TestPerson => new() { Id = CurrentUserMock.Object.Id, Cash = 100 };
    private PersonDto RecipientPerson => new() { Id = Recipient.Id, Cash = 200 };
    private AssetDto TransferAsset => new() { UserId = CurrentUserMock.Object.Id, Title = Recipient.Name, Type = AssetType.Transfer, IsDraft = true };

    [SetUp]
    public void Setup()
    {
        PersonManagerMock.Setup(a => a.ReadAllAssets(AssetType.Transfer, CurrentUserMock.Object)).Returns([TransferAsset]);
        PersonManagerMock.Setup(p => p.Read(CurrentUserMock.Object)).Returns(TestPerson);
        PersonManagerMock.Setup(p => p.Read(Recipient)).Returns(RecipientPerson);
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

        PersonManagerMock.Verify(a => a.DeleteAsset(
            It.Is<AssetDto>(x =>
                x.UserId == CurrentUserMock.Object.Id &&
                x.Type == AssetType.Transfer)
        ), Times.Once);

        PersonManagerMock.Verify(x => x.Update(It.IsAny<PersonDto>()), Times.Never, "No person data should be updated");
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
        CurrentUserMock.Verify(x => x.Notify("Invalid value. Try again."), Times.Once, "Message to user should be sent");
        AssetManagerMock.Verify(x => x.Update(It.IsAny<AssetDto>()), Times.Never, "No asset should be updated");
        PersonManagerMock.Verify(x => x.Update(It.IsAny<PersonDto>()), Times.Never, "No person data should be updated");
    }

    [Test]
    public async Task SendMoneyAmount_ToUser_AvailableAmount_TransferIsCompleted()
    {
        // Arrange
        var transferAmount = 100;
        var message = string.Format("{0} transferred {2} to {1}.", CurrentUserMock.Object.Name, Recipient.Name, transferAmount.AsCurrency());
        var activeUsers = OtherUsers.Where(u => u.IsActive).Select(u => Mock.Get(u)).Append(CurrentUserMock);

        var testStage = GetTestStage();
        var historyMock = new Mock<IHistory>();
        CurrentUserMock.SetupGet(u => u.History_OBSOLETE).Returns(historyMock.Object);

        // Act
        await testStage.HandleMessage($"{transferAmount}");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        AvailableAssetsMock.Verify(a => a.Add(It.IsAny<int>(), AssetType.BigGiveMoney), Times.Never);

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

        PersonManagerMock.Verify(p => p.Update(
            It.Is<PersonDto>(x =>
            x.Id == RecipientPerson.Id &&
            x.Cash == RecipientPerson.Cash + transferAmount)
            ), Times.Once);

        HistoryManagerMock.Verify(x => x.Add(ActionType.PayMoney, transferAmount, CurrentUserMock.Object), Times.Once);
        HistoryManagerMock.Verify(x => x.Add(ActionType.GetMoney, transferAmount, Recipient), Times.Once);

        activeUsers.ForEach(u => u.Verify(u => u.Notify(message), Times.Once));
    }

    [Test]
    public async Task SendMoneyAmount_ToBank_AvailableAmount_TransferIsCompleted()
    {
        // Arrange
        var transferAsset = TransferAsset;
        transferAsset.Title = "Bank";
        PersonManagerMock.Setup(a => a.ReadAllAssets(AssetType.Transfer, CurrentUserMock.Object)).Returns([transferAsset]);

        var transferAmount = 100;
        var message = string.Format("{0} transferred {2} to {1}.", CurrentUserMock.Object.Name, "Bank", transferAmount.AsCurrency());
        var activeUsers = OtherUsers.Where(u => u.IsActive).Select(u => Mock.Get(u)).Append(CurrentUserMock);

        var testStage = GetTestStage();
        var historyMock = new Mock<IHistory>();
        CurrentUserMock.SetupGet(u => u.History_OBSOLETE).Returns(historyMock.Object);

        // Act
        await testStage.HandleMessage($"{transferAmount}");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        AvailableAssetsMock.Verify(a => a.Add(It.IsAny<int>(), AssetType.BigGiveMoney), Times.Never);

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

        PersonManagerMock.Verify(p => p.Update(It.Is<PersonDto>(x =>x.Id == RecipientPerson.Id)), Times.Never);

        HistoryManagerMock.Verify(x => x.Add(ActionType.PayMoney, transferAmount, CurrentUserMock.Object), Times.Once);
        HistoryManagerMock.Verify(x => x.Add(ActionType.GetMoney, transferAmount, Recipient), Times.Never);

        activeUsers.ForEach(u => u.Verify(u => u.Notify(message), Times.Once));
    }

    [Test]
    public async Task SendMoneyAmount_UnavailableAmount_MoveToCredit()
    {
        // Arrange
        var transferAmount = 101;
        var testStage = GetTestStage();
        var personMock = new Mock<IPerson>();

        personMock.SetupGet(p => p.Cash).Returns(100);
        CurrentUserMock.SetupGet(u => u.Person_OBSOLETE).Returns(personMock.Object);

        // Act
        await testStage.HandleMessage($"{transferAmount}");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SendMoneyCredit>());

        PersonManagerMock.Verify(a => a.UpdateAsset(
        It.Is<AssetDto>(x =>
                x.UserId == TestPerson.Id &&
                x.Qtty == transferAmount &&
                x.Type == AssetType.Transfer)
            ), Times.Once);

        PersonManagerMock.Verify(x => x.Update(It.IsAny<PersonDto>()), Times.Never, "No person data should be updated");
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

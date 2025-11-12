using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Data.Users;
using CashFlow.Data.Users.UserData.HistoryData;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Extensions;
using CashFlow.Stages;
using Moq;
using MoreLinq;

namespace CashFlow.Tests.Stages.SendMoneyStages;

[TestFixture]
public class SendMoneyAmountTests : StagesBaseTest
{
    private IUser Recipient => OtherUsers.Last(u => u.IsActive && u.Person.Circle == Circle.Small);
    private PersonDto TestPerson => new PersonDto { Id = CurrentUserMock.Object.Id, Cash = 100 };
    private PersonDto RecipientPerson => new PersonDto { Id = Recipient.Id, Cash = 200 };

    [SetUp]
    public void Setup()
    {
        var transferAsset = new AssetDto
        {
            UserId = CurrentUserMock.Object.Id,
            Title = Recipient.Name,
            Type = AssetType.Transfer
        };

        AssetManagerMock.Setup(a => a.Read(AssetType.Transfer, TestPerson.Id)).Returns(transferAsset);
        PersonManagerMock.Setup(p => p.Read(TestPerson.Id)).Returns(TestPerson);
        PersonManagerMock.Setup(p => p.Read(RecipientPerson.Id)).Returns(RecipientPerson);
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
    public async Task SendMoneyAmount_AvailableAmount_TransferIsCompleted()
    {
        // Arrange
        var transferAmount = 100;
        var message = string.Format("{0} transferred {2} to {1}.", CurrentUserMock.Object.Name, Recipient.Name, transferAmount.AsCurrency());
        var activeUsers = OtherUsers.Where(u => u.IsActive).Select(u => Mock.Get(u)).Append(CurrentUserMock);

        var testStage = GetTestStage();
        var historyMock = new Mock<IHistory>();
        CurrentUserMock.SetupGet(u => u.History).Returns(historyMock.Object);

        // Act
        await testStage.HandleMessage($"{transferAmount}");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        AssetManagerMock.Verify(a => a.Update(
            It.Is<AssetDto>(x =>
                x.UserId == TestPerson.Id &&
                x.Qtty == transferAmount &&
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

        // history "manager"
        // ??? 

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
        CurrentUserMock.SetupGet(u => u.Person).Returns(personMock.Object);

        // Act
        await testStage.HandleMessage($"{transferAmount}");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SendMoneyCredit>());

        AssetManagerMock.Verify(a => a.Update(
        It.Is<AssetDto>(x =>
                x.UserId == TestPerson.Id &&
                x.Qtty == transferAmount &&
                x.Type == AssetType.Transfer)
            ), Times.Once);

        PersonManagerMock.Verify(x => x.Update(It.IsAny<PersonDto>()), Times.Never, "No person data should be updated");
    }

    protected override IStage GetTestStage() => new SendMoneyAmount(AssetManagerMock.Object, PersonManagerMock.Object, TermsServiceMock.Object, AssetsMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}

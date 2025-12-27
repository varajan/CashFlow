using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Data.Users;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Extensions;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.SendMoneyStages;
using Moq;
using MoreLinq;

namespace CashFlow.Tests.Stages.SmallCircleTests.SendMoneyStages;

[TestFixture]
public class SendMoneyCreditTests : StagesBaseTest
{
    private IUser Recipient => OtherUsers.Last(u => u.IsActive && u.Person_OBSOLETE.Circle == Circle.Small);
    private PersonDto TestPerson => new() { Id = CurrentUserMock.Object.Id, Cash = 600 };
    private PersonDto RecipientPerson => new() { Id = Recipient.Id, Cash = 200 };
    private AssetDto TransferAsset => new() { UserId = CurrentUserMock.Object.Id, Qtty = 1500, Type = AssetType.Transfer, IsDraft = true };

    [SetUp]
    public void Setup()
    {
        AssetManagerMock.Setup(a => a.ReadAll(AssetType.Transfer, TestPerson.Id)).Returns([TransferAsset]);
        PersonManagerMock.Setup(p => p.Read(TestPerson.Id)).Returns(TestPerson);
        PersonManagerMock.Setup(p => p.Read(RecipientPerson.Id)).Returns(RecipientPerson);
    }

    [Test]
    public async Task SendMoneyCredit_CanBeCanceled()
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
    public void SendMoneyCredit_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo($"You don''t have {TransferAsset.Qtty.AsCurrency()}, but only {TestPerson.Cash.AsCurrency()}"));
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
    public async Task SendMoneyCredit_ToBank_Confirmed_TransferIsCompleted()
    {
        // Arrange
        var transferAmount = TransferAsset.Qtty;
        var creditAmount = (int)Math.Ceiling((transferAmount - TestPerson.Cash) / 1_000d) * 1_000;
        var testStage = GetTestStage();
        var activeUsers = OtherUsers.Where(u => u.IsActive).Select(u => Mock.Get(u)).Append(CurrentUserMock);
        var message = string.Format("{0} transferred {2} to {1}.", CurrentUserMock.Object.Name, "Bank", transferAmount.AsCurrency());

        // Act
        await testStage.HandleMessage("Get credit");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        CurrentUserMock.Verify(u => u.GetCredit_OBSOLETE(creditAmount), Times.Once);

        AssetManagerMock.Verify(a => a.Delete(
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
        HistoryManagerMock.Verify(x => x.Add(ActionType.GetMoney, transferAmount, Recipient), Times.Never);

        activeUsers.ForEach(u => u.Verify(u => u.Notify(message), Times.Once));
    }

    [Test]
    public async Task SendMoneyCredit_ToUser_Confirmed_TransferIsCompleted()
    {
        // Arrange
        var transferAsset = TransferAsset;
        transferAsset.Title = Recipient.Name;
        AssetManagerMock.Setup(a => a.ReadAll(AssetType.Transfer, TestPerson.Id)).Returns([transferAsset]);

        var transferAmount = transferAsset.Qtty;
        var creditAmount = (int)Math.Ceiling((transferAmount - TestPerson.Cash) / 1_000d) * 1_000;
        var testStage = GetTestStage();
        var activeUsers = OtherUsers.Where(u => u.IsActive).Select(u => Mock.Get(u)).Append(CurrentUserMock);
        var message = string.Format("{0} transferred {2} to {1}.", CurrentUserMock.Object.Name, Recipient.Name, transferAmount.AsCurrency());

        // Act
        await testStage.HandleMessage("Get credit");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        CurrentUserMock.Verify(u => u.GetCredit_OBSOLETE(creditAmount), Times.Once);

        AssetManagerMock.Verify(a => a.Delete(
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

    protected override IStage GetTestStage() => new SendMoneyCredit(AssetManagerMock.Object, PersonManagerMock.Object, HistoryManagerMock.Object, TermsServiceMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}

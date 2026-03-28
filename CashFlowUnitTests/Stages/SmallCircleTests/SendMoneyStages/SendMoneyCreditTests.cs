using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.SendMoneyStages;
using CashFlowUnitTests.Stages;
using Moq;
using MoreLinq;

namespace CashFlowUnitTests.Stages.SmallCircleTests.SendMoneyStages;

[TestFixture]
public class SendMoneyCreditTests : StagesBaseTest
{
    private UserDto Recipient => OtherUsers.Last(u => u.IsActive() && u.Name.Contains("Small"));
    private PersonDto TestPerson => new() { Id = CurrentUser.Id, Cash = 600 };
    private PersonDto RecipientPerson => new() { Id = Recipient.Id, Cash = 200 };
    private AssetDto TransferAsset => new() { UserId = CurrentUser.Id, Qtty = 1500, Type = AssetType.Transfer, IsDraft = true };

    [SetUp]
    public void Setup()
    {
        PersonServiceMock.Setup(a => a.ReadAllAssets(AssetType.Transfer, CurrentUser)).Returns([TransferAsset]);
        PersonServiceMock.Setup(p => p.Read(CurrentUser)).Returns(TestPerson);
        PersonServiceMock.Setup(p => p.Read(Recipient)).Returns(RecipientPerson);
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

        PersonServiceMock.Verify(a => a.DeleteAsset(
            CurrentUser,
            It.Is<AssetDto>(x =>
                x.UserId == CurrentUser.Id &&
                x.Type == AssetType.Transfer)
        ), Times.Once);

        PersonServiceMock.Verify(x => x.Update(It.IsAny<PersonDto>()), Times.Never, "No person data should be updated");
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
        var activeUsers = OtherUsers.Where(u => u.IsActive()).Append(CurrentUser);
        var message = string.Format("{0} transferred {2} to {1}.", CurrentUser.Name, "Bank", transferAmount.AsCurrency());

        // Act
        await testStage.HandleMessage("Get credit");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        PersonServiceMock.Verify(a => a.DeleteAsset(
            CurrentUser,
            It.Is<AssetDto>(x =>
                x.UserId == TestPerson.Id &&
                x.Type == AssetType.Transfer)
        ), Times.Once);

        PersonServiceMock.Verify(p => p.Update(
            It.Is<PersonDto>(x =>
            x.Id == TestPerson.Id &&
            x.Cash == TestPerson.Cash + creditAmount - transferAmount)
            ), Times.Exactly(2));

        PersonServiceMock.Verify(x => x.AddHistory(ActionType.PayMoney, transferAmount, CurrentUser), Times.Once);
        PersonServiceMock.Verify(x => x.AddHistory(ActionType.GetMoney, transferAmount, Recipient), Times.Never);

        activeUsers.ForEach(u => NotifyServiceMock.Verify(n => n.Notify(u.Id, message), Times.Once));
    }

    [Test]
    public async Task SendMoneyCredit_ToUser_Confirmed_TransferIsCompleted()
    {
        // Arrange
        var transferAsset = TransferAsset;
        transferAsset.Title = Recipient.Name;
        PersonServiceMock.Setup(a => a.ReadAllAssets(AssetType.Transfer, CurrentUser)).Returns([transferAsset]);

        var transferAmount = transferAsset.Qtty;
        var creditAmount = (int)Math.Ceiling((transferAmount - TestPerson.Cash) / 1_000d) * 1_000;
        var testStage = GetTestStage();
        var activeUsers = OtherUsers.Where(u => u.IsActive()).Append(CurrentUser);
        var message = string.Format("{0} transferred {2} to {1}.", CurrentUser.Name, Recipient.Name, transferAmount.AsCurrency());

        // Act
        await testStage.HandleMessage("Get credit");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, $"You've taken {creditAmount.AsCurrency()} from bank."), Times.Once);

        PersonServiceMock.Verify(a => a.DeleteAsset(
            CurrentUser,
            It.Is<AssetDto>(x =>
                x.UserId == TestPerson.Id &&
                x.Type == AssetType.Transfer)
        ), Times.Once);

        PersonServiceMock.Verify(p => p.Update(
            It.Is<PersonDto>(x =>
            x.Id == TestPerson.Id &&
            x.Cash == TestPerson.Cash + creditAmount - transferAmount)
            ), Times.Exactly(2));

        PersonServiceMock.Verify(p => p.Update(
            It.Is<PersonDto>(x =>
            x.Id == RecipientPerson.Id &&
            x.Cash == RecipientPerson.Cash + transferAmount)
            ), Times.Once);

        PersonServiceMock.Verify(x => x.AddHistory(ActionType.PayMoney, transferAmount, CurrentUser), Times.Once);
        PersonServiceMock.Verify(x => x.AddHistory(ActionType.GetMoney, transferAmount, Recipient), Times.Once);

        activeUsers.ForEach(u => NotifyServiceMock.Verify(n => n.Notify(u.Id, message), Times.Once));
    }

    protected override IStage GetTestStage() => GetStage<SendMoneyCredit>();
}

using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
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
    private PersonDto TestPerson => new() { Id = CurrentUser.Id, Cash = Cash, BigCircle = true };
    private AssetDto TransferAsset => new() { UserId = CurrentUser.Id, Title = "Bank", Type = AssetType.Transfer, IsDraft = true };

    [SetUp]
    public void Setup()
    {
        PersonServiceMock.Setup(a => a.ReadAllAssets(AssetType.Transfer, CurrentUser)).Returns([TransferAsset]);
        PersonServiceMock.Setup(p => p.Read(CurrentUser)).Returns(TestPerson);
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

        PersonServiceMock.Verify(a => a.DeleteAsset(
            CurrentUser,
            It.Is<AssetDto>(x =>
                x.UserId == CurrentUser.Id &&
                x.Type == AssetType.Transfer)
        ), Times.Once);

        PersonServiceMock.Verify(x => x.Update(It.IsAny<PersonDto>()), Times.Never, "No person data should be updated");
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
        NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, "Invalid value. Try again."), Times.Once, "Message to user should be sent");
        PersonServiceMock.Verify(x => x.UpdateAsset(CurrentUser, It.IsAny<AssetDto>()), Times.Never, "No asset should be updated");
        PersonServiceMock.Verify(x => x.Update(It.IsAny<PersonDto>()), Times.Never, "No person data should be updated");
    }

    [Test]
    public async Task SendMoney_AvailableAmount_TransferIsCompleted()
    {
        // Arrange
        var transferAsset = TransferAsset;
        transferAsset.Title = "Bank";
        PersonServiceMock.Setup(a => a.ReadAllAssets(AssetType.Transfer, CurrentUser)).Returns([transferAsset]);

        var transferAmount = 100;
        var message = string.Format("{0} transferred {2} to {1}.", CurrentUser.Name, "Bank", transferAmount.AsCurrency());
        var activeUsers = OtherUsers.Where(u => u.IsActive()).Append(CurrentUser);

        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage($"{transferAmount}");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        AvailableAssetsMock.Verify(a => a.Add(It.IsAny<int>(), AssetType.BigGiveMoney), Times.Once);

        PersonServiceMock.Verify(a => a.UpdateAsset(
            CurrentUser,
            It.Is<AssetDto>(x =>
                x.UserId == TestPerson.Id &&
                x.Qtty == transferAmount &&
                x.Type == AssetType.Transfer)
        ), Times.Once);

        PersonServiceMock.Verify(a => a.DeleteAsset(
            CurrentUser,
            It.Is<AssetDto>(x =>
                x.UserId == TestPerson.Id &&
                x.Type == AssetType.Transfer)
        ), Times.Once);

        PersonServiceMock.Verify(p => p.Update(
            It.Is<PersonDto>(x =>
            x.Id == TestPerson.Id &&
            x.Cash == TestPerson.Cash - transferAmount)
            ), Times.Once);

        PersonServiceMock.Verify(x => x.AddHistory(ActionType.PayMoney, transferAmount, CurrentUser), Times.Once);

        activeUsers.ForEach(u => NotifyServiceMock.Verify(n => n.Notify(u.Id, message), Times.Once));
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

        PersonServiceMock.Verify(a => a.DeleteAsset(
            CurrentUser,
            It.Is<AssetDto>(x =>
                x.UserId == TestPerson.Id &&
                x.Qtty == transferAmount &&
                x.Type == AssetType.Transfer)
            ), Times.Once);

        PersonServiceMock.Verify(x => x.Update(It.IsAny<PersonDto>()), Times.Never, "No person data should be updated");
        NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, "You don't have enough money."), Times.Once);
    }

    protected override IStage GetTestStage() => GetStage<SendMoneyAmount>();
}

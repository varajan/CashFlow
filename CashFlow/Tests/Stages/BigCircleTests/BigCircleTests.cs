using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Stages;
using CashFlow.Stages.BigCircleStages;
using CashFlow.Stages.SmallCircleStages.SendMoneyStages;
using CashFlow.Stages.SmallCircleStages.ShowMyDataStages;
using Moq;
using MoreLinq;

namespace CashFlow.Tests.Stages.BigCircleTests;

[TestFixture]
public class BigCircleTests : StagesBaseTest
{
    private const int cash = 1_000_000;
    private const int paycheck = 500_000;
    private PersonDto Person => new()
    {
        BigCircle = true,
        Cash = cash,
        //CashFlow = 5_000,
        CurrentCashFlow = paycheck,
        TargetCashFlow = 1_000_000,
    };

    [SetUp]
    public void Setup() => PersonManagerMock.Setup(x => x.Read(CurrentUserMock.Object)).Returns(Person);

    [Test]
    public void BigCircle_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();

        var bigCircleDescription = $"{CurrentUserMock.Object.Name} at BigCircle!";
        var buttons = new[]
        {
            "Pay Check",
            "Get Money",
            "Give Money",
            "Divorce",
            "Tax Audit",
            "Lawsuit",
            "Buy Business",
            "Friends",
            "History",
            "Stop Game",
        };

        PersonManagerMock.Setup(x => x.GetDescription(CurrentUserMock.Object)).Returns(bigCircleDescription);

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo(bigCircleDescription));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        });
    }

    [Test]
    public async Task BigCircle_BeforeStage_NotifyNone()
    {
        // Arrange
        var testStage = GetTestStage();
        var message = $"{CurrentUserMock.Object.Name} is the winner!";
        var activeUsers = OtherUsers.Where(u => u.IsActive).Select(u => Mock.Get(u)).Append(CurrentUserMock);

        // Act
        await testStage.BeforeStage();

        // Assert
        activeUsers.ForEach(u => u.Verify(u => u.Notify(It.IsAny<string>()), Times.Never));
    }

    [Test]
    public async Task BigCircle_PayCheck()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("Pay Check");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BigCircle>());

        PersonManagerMock.Verify(p => p.Update(It.Is<PersonDto>(p => p.Cash == cash + paycheck)), Times.Once);
        PersonManagerMock.Verify(p => p.AddHistory(ActionType.GetMoney, paycheck, CurrentUserMock.Object), Times.Once);
    }

    [TestCase("Divorce", ActionType.Divorce, 1)]
    [TestCase("Tax Audit", ActionType.TaxAudit, 0.5)]
    [TestCase("Lawsuit", ActionType.Lawsuit, 0.5)]
    public async Task BigCircle_LostMoney(string message, ActionType action, double muliplier)
    {
        // Arrange
        var testStage = GetTestStage();
        var lostMoney = (int)(cash * muliplier);

        // Act
        await testStage.HandleMessage(message);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BigCircle>());

        PersonManagerMock.Verify(p => p.Update(It.Is<PersonDto>(p => p.Cash == cash - lostMoney)), Times.Once);
        PersonManagerMock.Verify(p => p.AddHistory(action, lostMoney, CurrentUserMock.Object), Times.Once);
    }

    [TestCase("Get Money", typeof(GetMoney))]
    [TestCase("Buy Business", typeof(BuyBigBusiness))]
    [TestCase("Friends", typeof(Friends))]
    [TestCase("History", typeof(History))]
    [TestCase("Stop Game", typeof(StopGame))]
    public async Task BigCircle_ValidOption(string message, Type nextStage)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(message);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf(nextStage));
    }

    [Test]
    public async Task BigCircle_GiveMoney()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("Give Money");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SendMoneyAmount>());

        PersonManagerMock.Verify(a => a.CreateAsset(
            It.Is<AssetDto>(x =>
                x.UserId == CurrentUserMock.Object.Id &&
                x.Type == AssetType.Transfer &&
                x.IsDraft &&
                x.Title == "Bank")
        ), Times.Once);
    }

    [TestCase("Cancel")]
    [TestCase("Win game")]
    public async Task BigCircle_InvalidOption(string message)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(message);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BigCircle>());

        PersonManagerMock.Verify(p => p.Update(It.IsAny<PersonDto>()), Times.Never);
        PersonManagerMock.Verify(p => p.AddHistory(It.IsAny<ActionType>(), It.IsAny<long>(), CurrentUserMock.Object), Times.Never);
    }

    [Test]
    public async Task BigCircle_CanNotBeCanceled()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("Cancel");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BigCircle>());
    }

    [Test, Ignore("Not applicable")]
    public override Task Stage_CanBeCanceled() => Task.CompletedTask;

    protected override IStage GetTestStage() => new BigCircle(TermsServiceMock.Object, PersonManagerMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}
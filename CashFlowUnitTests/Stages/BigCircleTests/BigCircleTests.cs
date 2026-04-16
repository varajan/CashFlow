using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Stages;
using CashFlow.Stages.BigCircleStages;
using CashFlow.Stages.SmallCircleStages.SendMoneyStages;
using CashFlow.Stages.SmallCircleStages.ShowMyDataStages;
using Moq;
using MoreLinq;

namespace CashFlowUnitTests.Stages.BigCircleTests;

[TestFixture]
public class BigCircleTests : StagesBaseTest
{
    private const int cash = 1_000_000;
    private const int paycheck = 500_000;

    private static PersonDto Person => new()
    {
        BigCircle = true,
        Cash = cash,
        InitialCashFlow = paycheck,
        TargetCashFlow = 1_000_000,
    };

    [SetUp]
    public void Setup() => PersonServiceMock.Setup(x => x.Read(CurrentUser)).Returns(Person);

    [Test]
    public void BigCircle_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();

        var smallCircleDescription = $"{CurrentUser.Name} at SmallCircle!";
        var bigCircleDescription = $"{CurrentUser.Name} at BigCircle!";
        var buttons = new[]
        {
            "Paycheck",
            "Get Money",
            "Give Money",
            "Divorce",
            "Tax Audit",
            "Lawsuit",
            "Buy Business",
            "Buy Dream",
            "Friends",
            "History",
            "Game menu",
        };

        PersonServiceMock.Setup(x => x.GetDescription(CurrentUser, false)).Returns(smallCircleDescription);
        PersonServiceMock.Setup(x => x.GetDescription(CurrentUser, true)).Returns(bigCircleDescription);

        // Act

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(testStage.Message, Is.EqualTo(bigCircleDescription));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        }
    }

    [Test]
    public async Task BigCircle_BeforeStage_NoWinner_NotifyNone()
    {
        // Arrange
        var testStage = GetTestStage();
        var activeUsers = OtherUsers.Where(u => u.Name.Contains("Active")).Append(CurrentUser);

        // Act
        await testStage.BeforeStage();

        // Assert
        activeUsers.ForEach(u => NotifyServiceMock.Verify(n => n.Notify(u.Id, It.IsAny<string>()), Times.Never));
    }

    [Test]
    public async Task BigCircle_BeforeStage_RollbackWinTransaction_NotifyNone()
    {
        // Arrange
        var person = Person.Clone();
        person.IsWinner = true;
        PersonServiceMock.Setup(x => x.Read(CurrentUser)).Returns(person);

        var testStage = GetTestStage();
        var activeUsers = OtherUsers.Where(u => u.Name.Contains("Active")).Append(CurrentUser);

        // Act
        await testStage.BeforeStage();

        // Assert
        activeUsers.ForEach(u => NotifyServiceMock.Verify(n => n.Notify(u.Id, It.IsAny<string>()), Times.Never));

        PersonServiceMock.Verify(p => p.Update(It.Is<PersonDto>(pr => pr.IsWinner == false)), Times.Once);
    }

    [Test]
    public async Task BigCircle_PayCheck()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("Paycheck");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BigCircle>());

        PersonServiceMock.Verify(p => p.Update(It.Is<PersonDto>(p => p.Cash == cash + paycheck)), Times.Once);
        PersonServiceMock.Verify(p => p.AddHistory(ActionType.GetMoney, paycheck, CurrentUser), Times.Once);
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

        PersonServiceMock.Verify(p => p.Update(It.Is<PersonDto>(p => p.Cash == cash - lostMoney)), Times.Once);
        PersonServiceMock.Verify(p => p.AddHistory(action, lostMoney, CurrentUser), Times.Once);
    }

    [TestCase("Get Money", typeof(GetMoney))]
    [TestCase("Buy Business", typeof(BuyBigBusiness))]
    [TestCase("Friends", typeof(Friends))]
    [TestCase("History", typeof(History))]
    [TestCase("Game menu", typeof(GameMenu))]
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

        PersonServiceMock.Verify(a => a.CreateAsset(
            CurrentUser,
            It.Is<AssetDto>(x =>
                x.UserId == CurrentUser.Id &&
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

        PersonServiceMock.Verify(p => p.Update(It.IsAny<PersonDto>()), Times.Never);
        PersonServiceMock.Verify(p => p.AddHistory(It.IsAny<ActionType>(), It.IsAny<long>(), CurrentUser), Times.Never);
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

    protected override IStage GetTestStage() => GetStage<BigCircle>();
}
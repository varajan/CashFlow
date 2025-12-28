using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Stages;
using CashFlow.Stages.BigCircleStages;
using Moq;

namespace CashFlow.Tests.Stages.BigCircleTests;

[TestFixture]
public class BigCircleTests : StagesBaseTest
{
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

        PersonManagerMock.Setup(x => x.GetDescription(CurrentUserMock.Object.Id)).Returns(bigCircleDescription);

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo(bigCircleDescription));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        });
    }

    [Test]
    public async Task BigCircle_PayCheck()
    {
        // Arrange
        var testStage = GetTestStage();
        var cash = 1_000_000;
        var paycheck = 500_000;
        var person = new PersonDto { Id = CurrentUserMock.Object.Id, Cash = cash, CashFlow = paycheck };

        PersonManagerMock.Setup(x => x.Read(CurrentUserMock.Object.Id)).Returns(person);

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
        var cash = 1_000_000;
        var lostMoney = (int)(cash * muliplier);
        var person = new PersonDto { Id = CurrentUserMock.Object.Id, Cash = cash };

        PersonManagerMock.Setup(x => x.Read(CurrentUserMock.Object.Id)).Returns(person);

        // Act
        await testStage.HandleMessage(message);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BigCircle>());

        PersonManagerMock.Verify(p => p.Update(It.Is<PersonDto>(p => p.Cash == cash - lostMoney)), Times.Once);
        PersonManagerMock.Verify(p => p.AddHistory(action, lostMoney, CurrentUserMock.Object), Times.Once);
    }

    [Test]
    public async Task BigCircle_GetMoney() => throw new NotImplementedException();

    [Test]
    public async Task BigCircle_BuyBusiness() => throw new NotImplementedException();

    [Test]
    public async Task BigCircle_Friends() => throw new NotImplementedException();

    [Test]
    public async Task BigCircle_History() => throw new NotImplementedException();

    [Test]
    public async Task BigCircle_StopGame() => throw new NotImplementedException();

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

    [Test, Ignore("Not applicable")]
    public override Task Stage_CanBeCanceled() => Task.CompletedTask;

    protected override IStage GetTestStage() => new BigCircle(TermsServiceMock.Object, PersonManagerMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}
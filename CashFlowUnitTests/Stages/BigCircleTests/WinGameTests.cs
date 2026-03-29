using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Stages;
using CashFlow.Stages.BigCircleStages;
using CashFlowUnitTests.Stages;
using Moq;
using MoreLinq;

namespace CashFlowUnitTests.Stages.BigCircleTests;

[TestFixture]
public class WinGameTests : StagesBaseTest
{
    private static PersonDto Person => new()
    {
        BigCircle = true,
        Cash = 500_000,
        InitialCashFlow = 500_000,
        TargetCashFlow = 500_000,
    };

    [SetUp]
    public void Setup() => PersonServiceMock.Setup(x => x.Read(CurrentUser)).Returns(Person);

    [Test]
    public void WinGame_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();
        var buttons = new[] { "History", "Stop Game" };

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo("You are the winner!"));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        });
    }

    [TestCase("History", typeof(History))]
    [TestCase("Stop Game", typeof(StopGame))]
    public async Task WinGame_ValidOption(string message, Type nextStage)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(message);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf(nextStage));
    }

    [Test]
    public async Task WinGame_BeforeStage_NotifyOthers([Values] bool alreadyNotified)
    {
        // Arrange
        var person = Person.Clone();
        person.IsWinning = alreadyNotified;
        PersonServiceMock.Setup(x => x.Read(CurrentUser)).Returns(person);

        var testStage = GetTestStage();
        var message = $"{CurrentUser.Name} is the winner!";
        var activeUsers = OtherUsers.Where(u => u.IsActive());

        // Act
        await testStage.BeforeStage();

        // Assert
        NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, It.IsAny<string>()), Times.Never);

        activeUsers.ForEach(u => NotifyServiceMock.Verify(n => n.Notify(u.Id, message), alreadyNotified ? Times.Never : Times.Once));

        PersonServiceMock.Verify(p => p.Update(It.Is<PersonDto>(pr =>
            pr.IsWinning == true)),
            alreadyNotified ? Times.Never : Times.Once);
    }

    [TestCase("Paycheck")]
    [TestCase("Get Money")]
    [TestCase("Give Money")]
    [TestCase("Divorce")]
    [TestCase("Tax Audit")]
    [TestCase("Lawsuit")]
    [TestCase("Buy Business")]
    [TestCase("Friends")]
    public async Task WinGame_InvalidOption(string message)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(message);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BigCircle>());
    }

    [Test, Ignore("Not applicable")]
    public override Task Stage_CanBeCanceled() => Task.CompletedTask;

    [Test]
    public async Task WinGame_CanNotBeCanceled()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("Cancel");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BigCircle>());
    }

    protected override IStage GetTestStage() => GetStage<BigCircle>();
}
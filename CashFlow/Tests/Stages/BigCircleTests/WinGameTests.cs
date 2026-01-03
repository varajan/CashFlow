using CashFlow.Data.DTOs;
using CashFlow.Stages;
using CashFlow.Stages.BigCircleStages;
using Moq;
using MoreLinq;

namespace CashFlow.Tests.Stages.BigCircleTests;

[TestFixture]
public class WinGameTests : StagesBaseTest
{
    private PersonDto Person => new()
    {
        BigCircle = true,
        Cash = 500_000,
        CashFlow = 5_000,
        CurrentCashFlow = 500_000,
        TargetCashFlow = 500_000,
    };

    [SetUp]
    public void Setup() => PersonManagerMock.Setup(x => x.Read(CurrentUserMock.Object.Id)).Returns(Person);

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
    public async Task WinGame_BeforeStage_NotifyOthers()
    {
        // Arrange
        var testStage = GetTestStage();
        var message = $"{CurrentUserMock.Object.Name} is the winner!";
        var activeUsers = OtherUsers.Where(u => u.IsActive).Select(u => Mock.Get(u));

        // Act
        await testStage.BeforeStage();

        // Assert
        CurrentUserMock.Verify(x => x.Notify(It.IsAny<string>()), Times.Never);
        activeUsers.ForEach(u => u.Verify(u => u.Notify(message), Times.Once));

        PersonManagerMock.Verify(p => p.Update(It.Is<PersonDto>(pr =>
            pr.IsWinning == true)),
            Times.Once);
    }

    [TestCase("Pay Check")]
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

    protected override IStage GetTestStage() => new BigCircle(TermsServiceMock.Object, PersonManagerMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}
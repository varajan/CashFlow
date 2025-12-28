using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages;

namespace CashFlow.Tests.Stages.SmallCircleTests.BankruptcyStages;

[TestFixture]
public class BankruptcyTests : StagesBaseTest
{
    [Test]
    public void Bankruptcy_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo("You are bankrupt. Game is over."));
            Assert.That(testStage.Buttons, Is.EqualTo(new[] { "Stop Game", "History" } ));
        });
    }

    [Test, Ignore("Not applicable")]
    public override Task Stage_CanBeCanceled() => Task.CompletedTask;

    [Test]
    public async Task Bankruptcy_CanNotBeCanceled()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("Cancel");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Bankruptcy>());
    }

    [TestCase("Stop game", typeof(StopGame))]
    [TestCase("History", typeof(History))]
    public async Task Bankruptcy_Select(string message, Type nextStage)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(message);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf(nextStage));
    }

    protected override IStage GetTestStage() => new Bankruptcy(TermsServiceMock.Object, PersonManagerMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}

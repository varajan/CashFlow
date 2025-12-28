using CashFlow.Extensions;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.ShowMyDataStages;
using Moq;

namespace CashFlow.Tests.Stages.SmallCircleTests.ShowMyDataStages;

[TestFixture]
public class GetCreditTests : StagesBaseTest
{
    [Test]
    public void GetCredit_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();
        var buttons = new List<string> { "1000", "2000", "5000", "10 000", "20 000", "Cancel" };

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo("How much?"));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        });
    }

    [TestCase("0")]
    [TestCase("900")]
    [TestCase("1900")]
    public async Task GetCredit_SelectInvalidAmount(string amount)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(amount);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<GetCredit>());
        CurrentUserMock.Verify(u => u.Notify("Invalid amount. The amount must be a multiple of 1000"), Times.Once);
    }

    [TestCase("1000")]
    [TestCase("$2000")]
    public async Task GetCredit_SelectValidAmount(string amount)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(amount);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());
        CurrentUserMock.Verify(u => u.GetCredit_OBSOLETE(amount.AsCurrency()), Times.Once);
    }

    protected override IStage GetTestStage() => new GetCredit(TermsServiceMock.Object, PersonManagerMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}

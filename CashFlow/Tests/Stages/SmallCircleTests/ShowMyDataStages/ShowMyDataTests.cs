using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.ShowMyDataStages;

namespace CashFlow.Tests.Stages.SmallCircleTests.ShowMyDataStages;

[TestFixture]
public class ShowMyDataTests : StagesBaseTest
{
    [Test]
    public void ShowMyData_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();
        var buttons = new List<string>
        {
            "Get Money",
            "Get Credit",
            "Charity - Pay 10%",
            "Reduce Liabilities",
            "Stop Game",
            "Main menu",
            "Cancel"
        };

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo("Test User at Small circle"));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        });
    }

    [Test]
    public void NotImplemented()
    {
#if DEBUG
        Assert.Fail("Not Implemented.");
#endif
    }

    protected override IStage GetTestStage() => new ShowMyData(TermsServiceMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}
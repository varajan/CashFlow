using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.BuyCoinsStages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.StartCompanyStages;

namespace CashFlow.Tests.Stages.SmallOpportunityStages;

[TestFixture]
public class SmallOpportunityTests : StagesBaseTest
{
    [Test]
    public void SmallOpportunity_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();
        var buttons = new List<string>
        {
            "Buy Stocks",
            "Sell Stocks",
            "Stocks x2",
            "Stocks ÷2",
            "Buy Real Estate",
            "Buy Land",
            "Buy coins",
            "Start a company",
            "Cancel"
        };

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo("What do you want?"));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        });
    }

    [TestCase("Buy Stocks", typeof(Start))]
    [TestCase("Sell Stocks", typeof(Start))]
    [TestCase("Stocks x2", typeof(Start))]
    [TestCase("Stocks ÷2", typeof(Start))]
    [TestCase("Buy Real Estate", typeof(Start))]
    [TestCase("Buy Land", typeof(Start))]
    [TestCase("Buy coins", typeof(BuyCoins))]
    [TestCase("Start a company", typeof(StartCompany))]
    [TestCase("Cancel", typeof(Start))]
    public async Task SmallOpportunity_SelectValidOption(string message, Type nextStage)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(message);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf(nextStage));
    }

    [Test]
    public async Task SmallOpportunity_SelectInValidOption()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("message");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SmallOpportunity>());
    }

    protected override IStage GetTestStage() => new SmallOpportunity(TermsServiceMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}
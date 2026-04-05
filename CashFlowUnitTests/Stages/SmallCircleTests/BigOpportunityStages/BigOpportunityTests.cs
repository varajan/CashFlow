using CashFlow.Data.Consts;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.BigOpportunityStages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages;
using CashFlowUnitTests.Stages;
using Moq;

namespace CashFlowUnitTests.Stages.SmallCircleTests.BigOpportunityStages;

[TestFixture]
public class BigOpportunityTests : StagesBaseTest
{
    [SetUp]
    public void Setup()
    {
        PersonServiceMock.Setup(a => a.ReadAllAssets(It.IsAny<AssetType>(), CurrentUser)).Returns([]);
    }

    [Test]
    public void BigOpportunity_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();
        var buttons = new List<string>
        {
            "Buy Real Estate",
            "Buy Business",
            "Buy Land",
            "Cancel"
        };

        // Act

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(testStage.Message, Is.EqualTo("What do you want?"));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        }
    }

    [TestCase("Buy Real Estate", typeof(BuyBigRealEstate))]
    [TestCase("Buy Business", typeof(BuyBusiness))]
    [TestCase("Buy Land", typeof(BuyLand))]
    [TestCase("Cancel", typeof(Start))]
    public async Task BigOpportunity_SelectValidOption(string message, Type nextStage)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(message);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf(nextStage));
    }

    [Test]
    public async Task BigOpportunity_SelectInValidOption()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("message");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BigOpportunity>());
    }

    protected override IStage GetTestStage() => GetStage<BigOpportunity>();
}

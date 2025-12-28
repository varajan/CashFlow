using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.BuyCoinsStages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.StocksStages;
using Moq;

namespace CashFlow.Tests.Stages.SmallCircleTests.SmallOpportunityStages;

[TestFixture]
public class SmallOpportunityTests : StagesBaseTest
{
    [SetUp]
    public void Setup()
    {
        AssetManagerMock.Setup(a => a.ReadAll(It.IsAny<AssetType>(), CurrentUserMock.Object.Id)).Returns([]);
    }

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

    [TestCase("Buy Stocks", typeof(BuyStocks))]
    [TestCase("Buy Real Estate", typeof(BuySmallRealEstate))]
    [TestCase("Buy Land", typeof(BuyLand))]
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

    [TestCase(false, "Sell Stocks", typeof(SmallOpportunity))]
    [TestCase(false, "Stocks x2", typeof(SmallOpportunity))]
    [TestCase(false, "Stocks ÷2", typeof(SmallOpportunity))]
    [TestCase(true, "Sell Stocks", typeof(SellStocks))]
    [TestCase(true, "Stocks x2", typeof(StocksMultiply))]
    [TestCase(true, "Stocks ÷2", typeof(StocksReduce))]
    public async Task SmallOpportunity_SelectValidOption(bool hasStocks, string message, Type nextStage)
    {
        // Arrange
        var testStage = GetTestStage();

        if (hasStocks)
        {
            AssetManagerMock.Setup(a => a.ReadAll(It.IsAny<AssetType>(), CurrentUserMock.Object.Id)).Returns([new AssetDto()]);
        }

        // Act
        await testStage.HandleMessage(message);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf(nextStage));

        CurrentUserMock.Verify(u => u.Notify("You have no stocks."), Times.Exactly(hasStocks ? 0 : 1));
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

    protected override IStage GetTestStage() => new SmallOpportunity(TermsServiceMock.Object, AssetManagerMock.Object, PersonManagerMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}
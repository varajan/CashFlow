using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.StocksStages;
using System.Text;
using Moq;

namespace CashFlowUnitTests.Stages.SmallCircleTests.SmallOpportunityStages.StocksStages.BuyStocksStages;

[TestFixture]
public class BuyStocksTests : StagesBaseTest
{
    private static readonly string[] Names = ["Uno", "Dos"];

    [SetUp]
    public void Setup()
    {
        AvailableAssetsMock.Setup(x => x.GetAsText(AssetType.Stock, It.IsAny<Language>())).Returns(Names);
    }

    [Test]
    public void BuyStocks_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();
        var buttons = Names.OrderBy(x => x).Append("Cancel");

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo("Title:"));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        });
    }

    [Test]
    public async Task BuyStocks_SelectInvalidValue_StayOnStage()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("Tres");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BuyStocks>());
    }

    [TestCaseSource(nameof(Names))]
    public async Task BuyStocks_SelectValidValue_MoveForward(string name)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(name.ToLower());

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BuyStocksPrice>());

        PersonServiceMock.Verify(a => a.CreateAsset(
            CurrentUser,
            It.Is<AssetDto>(x =>
                x.Title == name &&
                x.UserId == CurrentUser.Id &&
                x.Type == AssetType.Stock &&
                x.IsDraft)
        ), Times.Once);
    }

    protected override IStage GetTestStage() => GetStage<BuyStocks>();
}

using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.StocksStages;
using CashFlowUnitTests.Stages;
using Moq;

namespace CashFlowUnitTests.Stages.SmallCircleTests.SmallOpportunityStages.StocksStages.SellStocksStages;

[TestFixture]
public class SellStocksTests : StagesBaseTest
{
    private static readonly List<AssetDto> Assets =
    [
        new AssetDto { Title = "Uno", Type = AssetType.Stock, Qtty = 25 },
        new AssetDto { Title = "Uno", Type = AssetType.Stock, Qtty = 50 },
        new AssetDto { Title = "Dos", Type = AssetType.Stock, Qtty = 75 },
    ];

    [SetUp]
    public void Setup()
    {
        PersonServiceMock.Setup(a => a.ReadAllAssets(AssetType.Stock, CurrentUser)).Returns(Assets);
    }

    [Test]
    public void SellStocks_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();
        var buttons = Assets.Where(a => a.Type == AssetType.Stock).Select(x => x.Title).Distinct().Append("Cancel");

        // Act

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(testStage.Message, Is.EqualTo("What stocks do you want to sell?"));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        }
    }

    [Test]
    public async Task SellStocks_SelectInvalidValue_StayOnStage()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("Tres");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SellStocks>());
        NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, "Invalid stocks name."), Times.Once);
    }

    [TestCaseSource(nameof(Assets))]
    public async Task SellStocks_SelectValidValue_MoveForward(AssetDto asset)
    {
        // Arrange
        var testStage = GetTestStage();
        var assetsCount = Assets.Count(x => x.Title == asset.Title);

        // Act
        await testStage.HandleMessage(asset.Title.ToLower());

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SellStocksPrice>());

        PersonServiceMock.Verify(a => a.UpdateAsset(
            CurrentUser,
            It.Is<AssetDto>(x =>
                x.Title == asset.Title &&
                x.Type == AssetType.Stock &&
                x.MarkedToSell)
        ), Times.Exactly(assetsCount));
    }

    protected override IStage GetTestStage() => GetStage<SellStocks>();
}

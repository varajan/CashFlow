using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.StocksStages;
using Moq;

namespace CashFlow.Tests.Stages.SmallCircleTests.SmallOpportunityStages.StocksStages.MultiplyStocksStages;

public class StocksReduceTests : StagesBaseTest
{
    private static readonly List<AssetDto> Assets =
    [
        new AssetDto { Title = "Uno", Type = AssetType.Stock, Qtty = 25 },
        new AssetDto { Title = "Uno", Type = AssetType.Stock, Qtty = 50 },
        new AssetDto { Title = "Dos", Type = AssetType.Stock, Qtty = 75 },
    ];

    private static IEnumerable<string> StockNames => Assets.Select(x => x.Title).Distinct().ToList();

    [SetUp]
    public void Setup()
    {
        AssetManagerMock.Setup(a => a.ReadAll(AssetType.Stock, CurrentUserMock.Object.Id)).Returns(Assets);
    }

    [Test]
    public void StocksReduce_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();
        var buttons = Assets.Select(x => x.Title).Distinct().Append("Cancel");

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo("Title:"));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        });
    }

    [Test]
    public async Task StocksReduce_SelectInvalidValue_StayOnStage()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("Tres");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<StocksReduce>());
    }

    [TestCaseSource(nameof(StockNames))]
    public async Task StocksReduce_SelectValidValue_MoveForward(string stockName)
    {
        // Arrange
        var testStage = GetTestStage();
        var updatedAssets = Assets
            .Where(x => x.Title == stockName)
            .Select(x => new AssetDto
            {
                Title = x.Title,
                Type = x.Type,
                Qtty = x.Qtty / 2
            })
            .ToList();

        // Act
        await testStage.HandleMessage(stockName.ToLower());

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        AssetManagerMock.Verify(a => a.Update(It.IsAny<AssetDto>()), Times.Exactly(updatedAssets.Count));

        updatedAssets.ForEach(updatedAsset =>
        {
            AssetManagerMock.Verify(a => a.Update(
                It.Is<AssetDto>(x =>
                    x.Title == updatedAsset.Title &&
                    x.Type == AssetType.Stock &&
                    x.Qtty == updatedAsset.Qtty
                    )
            ), Times.Once);
        });
    }

    protected override IStage GetTestStage() => new StocksReduce(TermsServiceMock.Object, AssetManagerMock.Object, HistoryManagerMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}

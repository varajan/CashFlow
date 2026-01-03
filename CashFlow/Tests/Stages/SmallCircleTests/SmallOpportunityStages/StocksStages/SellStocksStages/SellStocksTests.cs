using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.StocksStages;
using Moq;

namespace CashFlow.Tests.Stages.SmallCircleTests.SmallOpportunityStages.StocksStages.SellStocksStages;

[TestFixture]
public class SellStocksTests: StagesBaseTest
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
        PersonManagerMock.Setup(a => a.ReadAllAssets(AssetType.Stock, CurrentUserMock.Object)).Returns(Assets);
    }

    [Test]
    public void SellStocks_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();
        var buttons = Assets.Where(a => a.Type == AssetType.Stock).Select(x => x.Title).Distinct().Append("Cancel");

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo("What stocks do you want to sell?"));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        });
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
        CurrentUserMock.Verify(c => c.Notify("Invalid stocks name."), Times.Once);
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

        PersonManagerMock.Verify(a => a.UpdateAsset(
            It.Is<AssetDto>(x =>
                x.Title == asset.Title &&
                x.Type == AssetType.Stock &&
                x.MarkedToSell)
        ), Times.Exactly(assetsCount));
    }

    protected override IStage GetTestStage() => new SellStocks(TermsServiceMock.Object, AssetManagerMock.Object, PersonManagerMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}

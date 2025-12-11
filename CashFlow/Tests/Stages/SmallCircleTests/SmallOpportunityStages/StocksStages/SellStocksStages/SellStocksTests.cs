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
        new AssetDto { Title = "Uno", Type = AssetType.Stock },
        new AssetDto { Title = "Dos", Type = AssetType.Stock },
    ];

    [SetUp]
    public void Setup()
    {
        AssetManagerMock.Setup(a => a.ReadAll(AssetType.Stock, CurrentUserMock.Object.Id)).Returns(Assets);
    }

    [Test]
    public void SellStocks_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();
        var buttons = Assets.Select(x => x.Title).Append("Cancel");

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo("What stocks do you want to sell?"));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        });
    }

    [Test]
    public async Task SellStocks_SelectInvalidName_StayOnStage()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("Tres");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SellStocks>());
    }

    [TestCaseSource(nameof(Assets))]
    public async Task SellStocks_SelectValidName_MoveForward(AssetDto asset)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(asset.Title.ToLower());

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SellStocksPrice>());

        AssetManagerMock.Verify(a => a.Update(
            It.Is<AssetDto>(x =>
                x.Title == asset.Title &&
                x.Type == AssetType.Stock &&
                x.MarkedToSell)
        ), Times.Once);
    }

    protected override IStage GetTestStage() => new SellStocks(TermsServiceMock.Object, AssetManagerMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}

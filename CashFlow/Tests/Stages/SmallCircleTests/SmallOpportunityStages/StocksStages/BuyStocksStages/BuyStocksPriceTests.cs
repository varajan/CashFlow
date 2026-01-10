using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.StocksStages;
using Moq;

namespace CashFlow.Tests.Stages.SmallCircleTests.SmallOpportunityStages.StocksStages.BuyStocksStages;

[TestFixture]
public class BuyStocksPriceTests : StagesBaseTest
{
    private static readonly string[] Prices = ["$10", "$50"];
    private AssetDto Asset => new() { Id = 123, Title = "Stock", UserId = CurrentUserMock.Object.Id, Type = AssetType.Stock, IsDraft = true };

    [SetUp]
    public void Setup()
    {
        AvailableAssetsMock.Setup(x => x.GetAsText(AssetType.StockPrice, CurrentUserMock.Object.Language)).Returns(Prices);
        PersonManagerMock.Setup(a => a.ReadAllAssets(AssetType.Stock, CurrentUserMock.Object)).Returns([Asset]);
    }

    [Test]
    public void BuyStocksPrice_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();
        var buttons = Prices.Append("Cancel");

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo("What is the price?"));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        });
    }

    [TestCase("a")]
    [TestCase("-1")]
    [TestCase("0")]
    [TestCase(" ")]
    [TestCase("$")]
    public async Task BuyStocksPrice_SelectInvalidPrice_StayOnStage(string count)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(count);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BuyStocksPrice>());
    }

    [TestCaseSource(nameof(Prices))]
    [TestCase("$1")]
    [TestCase("1")]
    public async Task BuyStocksPrice_SelectValidCount_MoveForward(string price)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(price);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.NextStage, Is.TypeOf<BuyStocksCount>());

            PersonManagerMock.Verify(m => m.UpdateAsset(
                CurrentUserMock.Object,
                It.Is<AssetDto>(x => x.Id == Asset.Id && x.Price == price.AsCurrency() && x.Qtty == 0)
            ), Times.Once);
        });
    }

    protected override IStage GetTestStage() => new BuyStocksPrice(
            TermsServiceMock.Object,
            AvailableAssetsMock.Object,
            PersonManagerMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}

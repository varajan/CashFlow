using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.StocksStages;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Moq;
using MoreLinq;

namespace CashFlowUnitTests.Stages.SmallCircleTests.SmallOpportunityStages.StocksStages.SellStocksStages;

[TestFixture]
public class SellStocksPriceTests : StagesBaseTest
{
    private PersonDto TestPerson => new() { Id = CurrentUser.Id, Cash = 300 };
    private static readonly List<string> AvailablePrices = Prices.StockPrice.OrderBy(x => x).AsCurrency().ToList();

    private static List<AssetDto> Assets =>
    [
        new AssetDto { Id = 1, Title = "Uno", Type = AssetType.Stock, Qtty = 10, MarkedToSell = true },
        new AssetDto { Id = 2, Title = "Dos", Type = AssetType.Stock, Qtty = 20, MarkedToSell = true },
        new AssetDto { Id = 3, Title = "Tres", Type = AssetType.Stock, Qtty = 30, MarkedToSell = false },
    ];

    [SetUp]
    public void Setup()
    {
        PersonServiceMock.Setup(a => a.ReadAllAssets(AssetType.Stock, CurrentUser)).Returns(Assets);
        PersonServiceMock.Setup(p => p.Read(CurrentUser)).Returns(TestPerson);
    }

    [Test]
    public void SellStocksPrice_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();
        var buttons = AvailablePrices.Append("Cancel");

        // Act

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(testStage.Message, Is.EqualTo("What is the price?"));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        }
    }

    [Test]
    public async Task SellStocksPrice_CanBeCanceled()
    {
        // Arrange
        var testStage = GetTestStage();
        var assets = Assets;

        // Act
        await testStage.HandleMessage("cancel");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        assets.Where(a => a.MarkedToSell).ForEach(asset =>
        {
            PersonServiceMock.Verify(a => a.UpdateAsset(
                CurrentUser,
                It.Is<AssetDto>(x =>
                    x.Title == asset.Title &&
                    x.Type == AssetType.Stock &&
                    x.MarkedToSell == false)
            ), Times.Once);
        });
    }

    [Test]
    public async Task SellStocksPrice_SelectInvalidValue_StayOnStage([Values("-1", "0", "$0", "test")] string price)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(price);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SellStocksPrice>());
        NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, "Invalid price value. Try again."), Times.Once);
        PersonServiceMock.Verify(a => a.UpdateAsset(CurrentUser, It.IsAny<AssetDto>()), Times.Never);
        PersonServiceMock.Verify(a => a.SellAsset(It.IsAny<AssetDto>(), It.IsAny<int>(), CurrentUser), Times.Never);
    }

    [TestCase("1")]
    [TestCaseSource(nameof(AvailablePrices))]
    public async Task SellStocksPrice_SelectValidValue_Completed(string price)
    {
        // Arrange
        var testStage = GetTestStage();
        var payedAmmount = 0;
        var assets = Assets;

        // Act
        await testStage.HandleMessage(price);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        assets
            .Where(a => a.Type == AssetType.Stock && a.MarkedToSell)
            .ForEach(asset =>
            {
                payedAmmount += asset.Qtty * price.AsCurrency();

                PersonServiceMock.Verify(a => a.SellAsset(It.Is<AssetDto>(a => a.Id == asset.Id), price.AsCurrency(), CurrentUser), Times.Once);
                PersonServiceMock.Verify(x => x.AddHistory(ActionType.SellStocks, price.AsCurrency(), CurrentUser, asset.Id), Times.Once);
                PersonServiceMock.Verify(p => p.Update(It.Is<PersonDto>(x => x.Id == TestPerson.Id)), Times.Exactly(2));
            });

        PersonServiceMock.Verify(p => p.Update(It.Is<PersonDto>(x => x.Id == TestPerson.Id && x.Cash == TestPerson.Cash + payedAmmount)), Times.Exactly(2));
        NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, "Done."), Times.Once);
    }

    protected override IStage GetTestStage() => GetStage<SellStocksPrice>();
}
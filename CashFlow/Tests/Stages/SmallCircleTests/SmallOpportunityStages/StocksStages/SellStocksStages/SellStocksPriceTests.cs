using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.StocksStages;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Moq;
using MoreLinq;

namespace CashFlow.Tests.Stages.SmallCircleTests.SmallOpportunityStages.StocksStages.SellStocksStages;

[TestFixture]
public class SellStocksPriceTests : StagesBaseTest
{
    private PersonDto TestPerson => new() { Id = CurrentUserMock.Object.Id, Cash = 300 };
    private static readonly List<string> AvailablePrices = [ "$100", "$500", "$1,000", ];

    private static readonly List<AssetDto> Assets =
    [
        new AssetDto { Title = "Uno", Type = AssetType.Stock, Qtty = 10, MarkedToSell = true },
        new AssetDto { Title = "Dos", Type = AssetType.Stock, Qtty = 20, MarkedToSell = true },
        new AssetDto { Title = "Tres", Type = AssetType.Stock, Qtty = 30, MarkedToSell = false },
    ];

    [SetUp]
    public void Setup()
    {
        PersonManagerMock.Setup(a => a.ReadAllAssets(AssetType.Stock, CurrentUserMock.Object)).Returns(Assets);
        AvailableAssetsMock.Setup(a => a.GetAsCurrency(AssetType.StockPrice)).Returns(AvailablePrices);
        PersonManagerMock.Setup(p => p.Read(CurrentUserMock.Object)).Returns(TestPerson);
    }

    [Test]
    public void SellStocksPrice_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();
        var buttons = AvailablePrices.Append("Cancel");

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo("What is the price?"));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        });
    }

    [Test]
    public async Task SellStocksPrice_CanBeCanceled()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("cancel");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        Assets.Where(a => a.MarkedToSell).ForEach(asset =>
        {
            PersonManagerMock.Verify(a => a.UpdateAsset(
                CurrentUserMock.Object,
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
        CurrentUserMock.Verify(u => u.Notify("Invalid price value. Try again."), Times.Once);
        PersonManagerMock.Verify(a => a.UpdateAsset(CurrentUserMock.Object, It.IsAny<AssetDto>()), Times.Never);
        PersonManagerMock.Verify(a => a.SellAsset(It.IsAny<AssetDto>(), It.IsAny<ActionType>(), It.IsAny<int>(), CurrentUserMock.Object), Times.Never);
    }

    [TestCase("1")]
    [TestCaseSource(nameof(AvailablePrices))]
    public async Task SellStocksPrice_SelectValidValue_Completed(string price)
    {
        // Arrange
        var testStage = GetTestStage();
        var payedAmmount = 0;

        // Act
        await testStage.HandleMessage(price);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        Assets
            .Where(a => a.Type == AssetType.Stock && a.MarkedToSell)
            .ForEach(asset =>
            {
                payedAmmount += asset.Qtty * price.AsCurrency();
                PersonManagerMock.Verify(a => a.SellAsset(asset, ActionType.SellStocks, price.AsCurrency(), CurrentUserMock.Object), Times.Once);
                PersonManagerMock.Verify(x => x.AddHistory(ActionType.SellStocks, asset.Id, CurrentUserMock.Object), Times.Once);
            });

        PersonManagerMock.Verify(p => p.Update(It.Is<PersonDto>(x => x.Id == TestPerson.Id && x.Cash == TestPerson.Cash + payedAmmount)), Times.Once);
        CurrentUserMock.Verify(u => u.Notify("Done."), Times.Once);
    }

    protected override IStage GetTestStage() => new SellStocksPrice(
        TermsServiceMock.Object,
        AvailableAssetsMock.Object,
        PersonManagerMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}
using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.MarketStages;
using Moq;
using MoreLinq;

namespace CashFlowUnitTests.Stages.SmallCircleTests.MarketStages;

[TestFixture]
public class SellRealEstatePriceTests : SellAssetBaseTest
{
    private PersonDto TestPerson => new() { Id = CurrentUser.Id, Cash = 300 };
    private static readonly List<string> AvailablePrices = ["$100", "$500", "$1,000",];

    [SetUp]
    public void PricesSetup()
    {
        AvailableAssetsMock.Setup(a => a.GetAsCurrency(AssetType.RealEstateSellPrice)).Returns(AvailablePrices);
        PersonServiceMock.Setup(p => p.Read(CurrentUser)).Returns(TestPerson);
    }

    [TestCase("2/1", "What is the price?")]
    [TestCase("3/2", "What is the price?")]
    [TestCase("2-plex", "You have *2* apartments. What is the price per one?")]
    [TestCase("8-plex", "You have *8* apartments. What is the price per one?")]
    public void SellRealEstatePrice_Question_and_Buttons(string apparment, string message)
    {
        // Arrange
        var testStage = GetTestStage();
        var buttons = AvailablePrices.Append("Cancel");
        Assets.Where(a => a.MarkedToSell).ForEach(a => a.Title = apparment);

        // Act

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(testStage.Message, Is.EqualTo(message));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        }
    }

    [Test]
    public async Task SellRealEstatePrice_CanBeCanceled()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("cancel");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        Assets
            .Where(a => a.Type == AssetType.RealEstate && a.MarkedToSell)
            .ForEach(asset =>
            {
                PersonServiceMock.Verify(a => a.UpdateAsset(
                    CurrentUser,
                    It.Is<AssetDto>(x =>
                        x.Title == asset.Title &&
                        x.Type == AssetType.RealEstate &&
                        x.MarkedToSell == false)
                ), Times.Once);
            });
    }

    [Test]
    public async Task SellRealEstatePrice_SelectInvalidPrice_StayOnStage([Values("-1", "0", "$0", "test")] string price)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(price);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SellRealEstatePrice>());
        NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, "Invalid price value. Try again."), Times.Once);
        PersonServiceMock.Verify(a => a.UpdateAsset(CurrentUser, It.IsAny<AssetDto>()), Times.Never);
        PersonServiceMock.Verify(a => a.SellAsset(It.IsAny<AssetDto>(), It.IsAny<int>(), CurrentUser), Times.Never);
    }

    [TestCase("2/1", 1, "$100")]
    [TestCase("3/2", 1, "500")]
    [TestCase("2-plex", 2, "$1,000")]
    [TestCase("8-plex", 8, "$5,000")]
    public async Task SellRealEstatePrice_SelectValidValue_Completed(string apparment, int count, string price)
    {
        // Arrange
        var testStage = GetTestStage();
        var payedAmmount = 0;
        Assets.Where(a => a.MarkedToSell).ForEach(a => a.Title = apparment);

        // Act
        await testStage.HandleMessage(price);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        Assets
            .Where(a => a.Type == AssetType.RealEstate && a.MarkedToSell)
            .ForEach(asset =>
            {
                payedAmmount += count * price.AsCurrency();
                PersonServiceMock.Verify(a => a.SellAsset(asset, price.AsCurrency(), CurrentUser), Times.Once);
                PersonServiceMock.Verify(x => x.AddHistory(ActionType.SellRealEstate, price.AsCurrency(), CurrentUser, asset.Id), Times.Once);
            });

        PersonServiceMock.Verify(p => p.Update(It.Is<PersonDto>(x => x.Id == TestPerson.Id && x.Cash == TestPerson.Cash + payedAmmount)), Times.Once);
        NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, "Done."), Times.Once);
    }

    protected override IStage GetTestStage() => GetStage<SellRealEstatePrice>();
}

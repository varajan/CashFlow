using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.MarketStages;
using Moq;
using MoreLinq;

namespace CashFlow.Tests.Stages.SmallCircleTests.MarketStages;

[TestFixture]
public class SellBusinessPriceTests : SellAssetBaseTest
{
    private PersonDto TestPerson => new() { Id = CurrentUser.Id, Cash = 300 };
    private static readonly List<string> AvailablePrices = ["$100", "$500", "$1,000",];

    [SetUp]
    public void TestSetUp()
    {
        AvailableAssetsMock.Setup(a => a.GetAsCurrency(AssetType.BusinessSellPrice)).Returns(AvailablePrices);
        PersonServiceMock.Setup(p => p.Read(CurrentUser)).Returns(TestPerson);
    }

    [Test]
    public void SellBusinessPrice_Question_and_Buttons()
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
    public async Task SellBusinessPrice_CanBeCanceled()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("cancel");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        Assets
            .Where(a => a.MarkedToSell && (a.Type == AssetType.Business || a.Type == AssetType.SmallBusiness))
            .ForEach(asset =>
            {
                PersonServiceMock.Verify(a => a.UpdateAsset(
                    CurrentUser,
                    It.Is<AssetDto>(x =>
                        x.Title == asset.Title &&
                        (x.Type == AssetType.Business || x.Type == AssetType.SmallBusiness) &&
                        x.MarkedToSell == false)
                ), Times.Once);
            });
    }

    [Test]
    public async Task SellBusinessPrice_SelectInvalidPrice_StayOnStage([Values("-1", "0", "$0", "test")] string price)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(price);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SellBusinessPrice>());
        NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, "Invalid price value. Try again."), Times.Once);
        PersonServiceMock.Verify(a => a.UpdateAsset(CurrentUser, It.IsAny<AssetDto>()), Times.Never);
        PersonServiceMock.Verify(a => a.SellAsset(It.IsAny<AssetDto>(), It.IsAny<ActionType>(), It.IsAny<int>(), CurrentUser), Times.Never);
    }

    [TestCase("1")]
    [TestCaseSource(nameof(AvailablePrices))]
    public async Task SellBusinessPrice_SelectValidValue_Completed(string price)
    {
        // Arrange
        var testStage = GetTestStage();
        var payedAmmount = 0;

        // Act
        await testStage.HandleMessage(price);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        Assets
            .Where(a => a.MarkedToSell && (a.Type == AssetType.Business || a.Type == AssetType.SmallBusinessType))
            .ForEach(asset =>
            {
                payedAmmount += price.AsCurrency();
                PersonServiceMock.Verify(a => a.SellAsset(asset, ActionType.SellBusiness, price.AsCurrency(), CurrentUser), Times.Once);
                PersonServiceMock.Verify(x => x.AddHistory(ActionType.SellBusiness, price.AsCurrency(), CurrentUser, asset.Id), Times.Once);
            });

        PersonServiceMock.Verify(p => p.Update(It.Is<PersonDto>(x => x.Id == TestPerson.Id && x.Cash == TestPerson.Cash + payedAmmount)), Times.Exactly(2));
        NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, "Done."), Times.Once);
    }

    protected override IStage GetTestStage() => GetStage<SellBusinessPrice>();
}

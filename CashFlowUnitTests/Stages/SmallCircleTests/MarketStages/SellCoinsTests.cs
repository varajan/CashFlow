using CashFlow.Data.DTOs;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.MarketStages;
using Moq;

namespace CashFlowUnitTests.Stages.SmallCircleTests.MarketStages;

[TestFixture]
public class SellCoinsTests : SellAssetBaseTest
{
    [Test]
    public void SellCoins_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();
        var buttons = new List<string> { "Coin No1", "Coin No2", "Coin No3", "Cancel" };
        var message = "What coins do you want to sell?";

        // Act

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(testStage.Message, Is.EqualTo(message));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        }
    }

    [Test]
    public async Task SellCoins_SelectInvalidOption_StayOnStage([Values("coin", "Coin No4")] string option)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(option);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SellCoins>());
        NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, "Invalid coins title."), Times.Once);
        NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task SellCoins_SelectValidOption_MoveForward([Values("Coin No1", "coin no2", "COIN NO3")] string option)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(option);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SellCoinsPrice>());

        PersonServiceMock.Verify(a => a.UpdateAsset(CurrentUser, It.IsAny<AssetDto>()), Times.Once);
        PersonServiceMock.Verify(a => a.UpdateAsset(CurrentUser, It.Is<AssetDto>(x =>
            x.Title.Contains(option, StringComparison.InvariantCultureIgnoreCase) &&
            x.MarkedToSell)), Times.Once);
    }

    protected override IStage GetTestStage() => GetStage<SellCoins>();
}

using CashFlow.Data.DTOs;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.MarketStages;
using Moq;

namespace CashFlow.Tests.Stages.SmallCircleTests.MarketStages;

[TestFixture]
public class SellLandTests : SellAssetBaseTest
{
    [Test]
    public void SellLand_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();
        var buttons = new List<string> { "#1", "#2", "#3", "Cancel" };
        var message = $"What Land do you want to sell?{NL}*#1* Land No1 Text{NL}*#2* Land No2 Text{NL}*#3* Land No3 Text";

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo(message));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        });
    }

    [Test]
    public async Task SellLand_SelectInvalidOption_StayOnStage([Values("0", "#4")] string option)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(option);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SellLand>());
        CurrentUserMock.Verify(c => c.Notify("Invalid land number."), Times.Once);
        CurrentUserMock.Verify(c => c.Notify(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task SellLand_SelectValidOption_MoveForward([Values(" 1", "#2", "3")] string option)
    {
        // Arrange
        var testStage = GetTestStage();
        var index = option.Replace("#", "").Trim();

        // Act
        await testStage.HandleMessage(option);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SellLandPrice>());

        PersonManagerMock.Verify(a => a.UpdateAsset(CurrentUserMock.Object, It.IsAny<AssetDto>()), Times.Once);
        PersonManagerMock.Verify(a => a.UpdateAsset(CurrentUserMock.Object, It.Is<AssetDto>(x => x.Title.Contains(index) && x.MarkedToSell)), Times.Once);
    }

    protected override IStage GetTestStage() => new SellLand(TermsServiceMock.Object, PersonManagerMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}

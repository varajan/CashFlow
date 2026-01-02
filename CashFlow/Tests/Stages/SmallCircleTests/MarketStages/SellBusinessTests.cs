using CashFlow.Data.DTOs;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.MarketStages;
using Moq;

namespace CashFlow.Tests.Stages.SmallCircleTests.MarketStages;

[TestFixture]
public class SellBusinessTests : SellAssetBaseTest
{
    [Test]
    public void SellBusiness_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();
        var buttons = new List<string> { "#1", "#2", "#3", "#4", "#5", "#6", "Cancel" };
        var message = "What Business do you want to sell?";
        message += $"{NL}*#1* Business No1 Text{NL}*#2* Business No2 Text{NL}*#3* Business No3 Text";
        message += $"{NL}*#4* SmallBusiness No1 Text{NL}*#5* SmallBusiness No2 Text{NL}*#6* SmallBusiness No3 Text";

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo(message));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        });
    }

    [Test]
    public async Task SellBusiness_SelectInvalidOption_StayOnStage([Values("0", "#7")] string option)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(option);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SellBusiness>());
        CurrentUserMock.Verify(c => c.Notify("Invalid business number."), Times.Once);
        CurrentUserMock.Verify(c => c.Notify(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task SellBusiness_SelectValidOption_MoveForward([Values(" 1", "#2", "3")] string option)
    {
        // Arrange
        var testStage = GetTestStage();
        var index = option.Replace("#", "").Trim();

        // Act
        await testStage.HandleMessage(option);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SellBusinessPrice>());

        PersonManagerMock.Verify(a => a.UpdateAsset(It.IsAny<AssetDto>()), Times.Once);
        PersonManagerMock.Verify(a => a.UpdateAsset(It.Is<AssetDto>(x => x.Title.Contains(index) && x.MarkedToSell)), Times.Once);
    }

    protected override IStage GetTestStage() => new SellBusiness(TermsServiceMock.Object, AssetManagerMock.Object, PersonManagerMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}

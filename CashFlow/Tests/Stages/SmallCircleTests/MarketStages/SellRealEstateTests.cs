using CashFlow.Data.DTOs;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.MarketStages;
using Moq;

namespace CashFlow.Tests.Stages.SmallCircleTests.MarketStages;

[TestFixture]
public class SellRealEstateTests : SellAssetBaseTest
{
    [Test]
    public void SellRealEstate_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();
        var buttons = new List<string> { "#1", "#2", "#3", "Cancel" };
        var message = $"What RealEstate do you want to sell?{NL}*#1* RealEstate No1 Text{NL}*#2* RealEstate No2 Text{NL}*#3* RealEstate No3 Text";

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo(message));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        });
    }

    [Test]
    public async Task SellRealEstate_SelectInvalidOption_StayOnStage([Values("0", "#4")] string option)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(option);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SellRealEstate>());
        CurrentUserMock.Verify(c => c.Notify("Invalid Real Estate number."), Times.Once);
        CurrentUserMock.Verify(c => c.Notify(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task SellRealEstate_SelectValidOption_MoveForward([Values(" 1", "#2", "3")] string option)
    {
        // Arrange
        var testStage = GetTestStage();
        var index = option.Replace("#", "").Trim();

        // Act
        await testStage.HandleMessage(option);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SellRealEstatePrice>());

        PersonManagerMock.Verify(a => a.UpdateAsset(It.IsAny<AssetDto>()), Times.Once);
        PersonManagerMock.Verify(a => a.UpdateAsset(It.Is<AssetDto>(x => x.Title.Contains(index) && x.MarkedToSell)), Times.Once);
    }

    protected override IStage GetTestStage() => new SellRealEstate(TermsServiceMock.Object, AssetManagerMock.Object, PersonManagerMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}

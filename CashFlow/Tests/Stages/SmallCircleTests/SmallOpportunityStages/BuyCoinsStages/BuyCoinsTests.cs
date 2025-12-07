using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.BuyCoinsStages;
using Moq;

namespace CashFlow.Tests.Stages.SmallCircleTests.SmallOpportunityStages.BuyCoinsStages;

[TestFixture]
public class BuyCoinsTests : StagesBaseTest
{
    private static readonly string[] Names = ["Uno", "Dos"];

    [SetUp]
    public void Setup()
    {
        AvailableAssetsMock.Setup(x => x.GetAsText(AssetType.CoinTitle, It.IsAny<Language>())).Returns(Names);
        AssetManagerMock.Setup(a => a.ReadAll(AssetType.Coin, CurrentUserMock.Object.Id)).Returns([]);
    }

    [Test]
    public void BuyCoins_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();
        var buttons = Names.OrderBy(x => x).Append("Cancel");

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo("Title:"));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        });
    }

    [Test]
    public async Task BuyCoins_SelectInvalidName_StayOnStage()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("Coin Tres");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BuyCoins>());
    }

    [TestCaseSource(nameof(Names))]
    public async Task BuyCoins_SelectValidName_MoveForward(string title)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(title.ToLower());

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BuyCoinsCount>());

        AssetManagerMock.Verify(a => a.Create(
            It.Is<AssetDto>(x =>
                x.Title == title &&
                x.UserId == CurrentUserMock.Object.Id &&
                x.Type == AssetType.Coin &&
                x.IsDraft)
        ), Times.Once);
    }

    protected override IStage GetTestStage() => new BuyCoins(TermsServiceMock.Object, AvailableAssetsMock.Object, AssetManagerMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}

using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Stages;
using Moq;

namespace CashFlow.Tests.Stages.BuyCoinsStages;

[TestFixture]
public class BuyCoinsTests : StagesBaseTest
{
    private static readonly string[] CoinNames = ["Coin Uno", "Coin Dos"];

    [SetUp]
    public void Setup()
    {
        AvailableAssetsMock.Setup(x => x.GetAsText(AssetType.CoinTitle, It.IsAny<Language>())).Returns(CoinNames);
    }

    [Test]
    public void BuyCoins_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo("Title:"));
            Assert.That(testStage.Buttons, Is.EqualTo(new List<string> { "Coin Uno", "Coin Dos", "Cancel" }));
        });
    }

    [Test]
    public async Task BuyCoins_SelectInvalidCoin_StayOnStage()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("Coin Tres");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BuyCoins>());
    }

    [TestCaseSource(nameof(CoinNames))]
    public async Task BuyCoins_SelectInvalidCoin_MoveForward(string coinName)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(coinName.ToLower());

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BuyCoinsCount>());

        AssetManagerMock.Verify(a => a.Create(
            It.Is<AssetDto>(x =>
                x.Title == coinName &&
                x.UserId == CurrentUserMock.Object.Id &&
                x.Type == AssetType.Coin &&
                x.IsDraft)
        ), Times.Once);
    }

    protected override IStage GetTestStage() => new BuyCoins(TermsServiceMock.Object, AvailableAssetsMock.Object, AssetManagerMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}

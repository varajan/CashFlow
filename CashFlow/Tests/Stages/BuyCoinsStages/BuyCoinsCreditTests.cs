using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Stages;
using Moq;

namespace CashFlow.Tests.Stages.BuyCoinsStages;

[TestFixture]
public class BuyCoinsCreditTests : StagesBaseTest
{
    // BuyCoinsCredit

    private static readonly string[] CoinCounts = ["1", "2", "5"];
    private AssetDto Asset => new() { UserId = CurrentUserMock.Object.Id, Price = 500, Qtty = 5, Type = AssetType.Coin, IsDraft = true };

    [SetUp]
    public void Setup()
    {
        AvailableAssetsMock.Setup(x => x.GetAsText(AssetType.CoinCount, It.IsAny<Language>())).Returns(CoinCounts);
        AssetManagerMock.Setup(a => a.ReadAll(AssetType.Coin, CurrentUserMock.Object.Id)).Returns([Asset]);
    }

    [Test]
    public void BuyCoinsCredit_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();
        var buttons = CoinCounts.Append("Cancel");

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo("How much?"));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        });
    }

    protected override IStage GetTestStage() => new BuyCoinsCredit(
            TermsServiceMock.Object,
            AvailableAssetsMock.Object,
            HistoryManagerMock.Object,
            PersonManagerMock.Object,
            AssetManagerMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}

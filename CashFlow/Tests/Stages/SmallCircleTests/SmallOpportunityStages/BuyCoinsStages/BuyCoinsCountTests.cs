using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.BuyCoinsStages;
using Moq;

namespace CashFlow.Tests.Stages.SmallCircleTests.SmallOpportunityStages.BuyCoinsStages;

[TestFixture]
public class BuyCoinsCountTests : StagesBaseTest
{
    private static readonly string[] Counts = ["1", "2", "5"];
    private AssetDto Asset => new() { UserId = CurrentUser.Id, Type = AssetType.Coin, IsDraft = true };

    [SetUp]
    public void Setup()
    {
        AvailableAssetsMock.Setup(x => x.GetAsText(AssetType.CoinCount, It.IsAny<Language>())).Returns(Counts);
        PersonServiceMock.Setup(a => a.ReadAllAssets(AssetType.Coin, CurrentUser)).Returns([Asset]);
    }

    [Test]
    public void BuyCoinsCount_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();
        var buttons = Counts.Append("Cancel");

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo("How much?"));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        });
    }

    [TestCase("a")]
    [TestCase("-1")]
    [TestCase("0")]
    [TestCase(" ")]
    public async Task BuyCoinsCount_SelectInvalidCount_StayOnStage(string count)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(count);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BuyCoinsCount>());
    }

    [TestCaseSource(nameof(Counts))]
    [TestCase("10")]
    public async Task BuyCoinsCount_SelectValidCount_MoveForward(string count)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(count.ToLower());

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BuyCoinsPrice>());

        PersonServiceMock.Verify(a => a.UpdateAsset(
            CurrentUser,
            It.Is<AssetDto>(x =>
                x.Qtty == count.ToInt() &&
                x.Type == AssetType.Coin &&
                x.IsDraft)
        ), Times.Once);
    }

    protected override IStage GetTestStage() => new BuyCoinsCount(TermsServiceMock.Object, AvailableAssetsMock.Object, PersonServiceMock.Object, UserRepositoryMock.Object)
        .SetCurrentUser(CurrentUser);
}

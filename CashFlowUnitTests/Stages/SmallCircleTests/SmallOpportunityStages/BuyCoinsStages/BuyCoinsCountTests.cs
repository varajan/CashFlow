using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.BuyCoinsStages;
using Moq;

namespace CashFlowUnitTests.Stages.SmallCircleTests.SmallOpportunityStages.BuyCoinsStages;

[TestFixture]
public class BuyCoinsCountTests : StagesBaseTest
{
    private static readonly int[] Counts = [1, 10];
    private AssetDto Asset => new() { UserId = CurrentUser.Id, Type = AssetType.Coin, IsDraft = true };

    [SetUp]
    public void Setup() => PersonServiceMock.Setup(a => a.ReadActiveAssets(AssetType.Coin, CurrentUser)).Returns([Asset]);

    [Test]
    public void BuyCoinsCount_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();
        var buttons = Counts.Select(x => x.ToString()).Append("Cancel");

        // Act

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(testStage.Message, Is.EqualTo("How many?"));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        }
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
    [TestCase(5)]
    public async Task BuyCoinsCount_SelectValidCount_MoveForward(int count)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(count.ToString());

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BuyCoinsPrice>());

        PersonServiceMock.Verify(a => a.UpdateAsset(
            CurrentUser,
            It.Is<AssetDto>(x =>
                x.Qtty == count &&
                x.Type == AssetType.Coin &&
                x.IsDraft)
        ), Times.Once);
    }

    protected override IStage GetTestStage() => GetStage<BuyCoinsCount>();
}

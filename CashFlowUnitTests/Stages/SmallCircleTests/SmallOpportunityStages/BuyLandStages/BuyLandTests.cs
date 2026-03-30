using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages;
using CashFlowUnitTests.Stages;
using Moq;

namespace CashFlowUnitTests.Stages.SmallCircleTests.SmallOpportunityStages.BuyLandStages;

[TestFixture]
public class BuyLandTests : StagesBaseTest
{
    private static readonly string[] Names = ["Uno", "Dos"];

    [SetUp]
    public void Setup()
    {
        AvailableAssetsMock.Setup(x => x.GetAsText(AssetType.LandTitle, It.IsAny<Language>())).Returns(Names);
        PersonServiceMock.Setup(a => a.ReadAllAssets(AssetType.LandTitle, CurrentUser)).Returns([]);
    }

    [Test]
    public void BuyLand_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();
        var buttons = Names.OrderBy(x => x).Append("Cancel");

        // Act

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(testStage.Message, Is.EqualTo("Title:"));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        }
    }

    [Test]
    public async Task BuyLand_SelectInvalidValue_StayOnStage()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("Tres");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BuyLand>());
    }

    [TestCaseSource(nameof(Names))]
    public async Task BuyLand_SelectValidValue_MoveForward(string name)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(name.ToLower());

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BuyLandPrice>());

        PersonServiceMock.Verify(a => a.CreateAsset(
            CurrentUser,
            It.Is<AssetDto>(x =>
                x.Title == name &&
                x.Qtty == 1 &&
                x.UserId == CurrentUser.Id &&
                x.Type == AssetType.Land &&
                x.IsDraft)
        ), Times.Once);
    }

    protected override IStage GetTestStage() => GetStage<BuyLand>();
}

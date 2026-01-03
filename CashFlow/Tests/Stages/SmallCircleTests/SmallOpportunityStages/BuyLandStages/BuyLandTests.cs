using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages;
using Moq;

namespace CashFlow.Tests.Stages.SmallCircleTests.SmallOpportunityStages.BuyLandStages;

[TestFixture]
public class BuyLandTests : StagesBaseTest
{
    private static readonly string[] Names = ["Uno", "Dos"];

    [SetUp]
    public void Setup()
    {
        AvailableAssetsMock.Setup(x => x.GetAsText(AssetType.LandTitle, It.IsAny<Language>())).Returns(Names);
        PersonManagerMock.Setup(a => a.ReadAllAssets(AssetType.LandTitle, CurrentUserMock.Object)).Returns([]);
    }

    [Test]
    public void BuyLand_Question_and_Buttons()
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

        PersonManagerMock.Verify(a => a.CreateAsset(
            It.Is<AssetDto>(x =>
                x.Title == name &&
                x.Qtty == 1 &&
                x.UserId == CurrentUserMock.Object.Id &&
                x.Type == AssetType.Land &&
                x.IsDraft)
        ), Times.Once);
    }

    protected override IStage GetTestStage() => new BuyLand(TermsServiceMock.Object, AvailableAssetsMock.Object, PersonManagerMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}

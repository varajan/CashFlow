using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.BuyLandStages;
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
        AssetManagerMock.Setup(a => a.ReadAll(AssetType.LandTitle, CurrentUserMock.Object.Id)).Returns([]);
    }

    [Test]
    public void BuyLand_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();
        var buttons = Names.Append("Cancel");

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo("Title:"));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        });
    }

    [Test]
    public async Task BuyLand_SelectInvalidName_StayOnStage()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("Coin Tres");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BuyLand>());
    }

    [TestCaseSource(nameof(Names))]
    public async Task BuyLand_SelectValidName_MoveForward(string name)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(name.ToLower());

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BuyLandPrice>());

        AssetManagerMock.Verify(a => a.Create(
            It.Is<AssetDto>(x =>
                x.Title == name &&
                x.UserId == CurrentUserMock.Object.Id &&
                x.Type == AssetType.LandTitle &&
                x.IsDraft)
        ), Times.Once);
    }

    protected override IStage GetTestStage() => new BuyLand(TermsServiceMock.Object, AvailableAssetsMock.Object, AssetManagerMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}

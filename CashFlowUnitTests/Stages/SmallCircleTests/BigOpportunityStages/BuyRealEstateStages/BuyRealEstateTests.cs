using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.BigOpportunityStages;
using Moq;

namespace CashFlow.Tests.Stages.SmallCircleTests.BigOpportunityStages.BuyRealEstateStages;

[TestFixture]
public class BuyRealEstateTests : StagesBaseTest
{
    private static readonly string[] Names = ["2/1", "3/2"];

    [SetUp]
    public void Setup()
    {
        AvailableAssetsMock.Setup(x => x.GetAsText(AssetType.RealEstateBigType, It.IsAny<Language>())).Returns(Names);
        PersonServiceMock.Setup(a => a.ReadAllAssets(AssetType.RealEstateBigType, CurrentUser)).Returns([]);
    }

    [Test]
    public void BuyRealEstate_Question_and_Buttons()
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
    public async Task BuyRealEstate_SelectInvalidValue_StayOnStage()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("4/3");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BuyBigRealEstate>());
    }

    [TestCaseSource(nameof(Names))]
    public async Task BuyRealEstate_SelectValidValue_MoveForward(string title)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(title.ToLower());

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BuyBigRealEstatePrice>());

        PersonServiceMock.Verify(a => a.CreateAsset(
            CurrentUser,
            It.Is<AssetDto>(x =>
                x.Title == title &&
                x.Qtty == 1 &&
                x.UserId == CurrentUser.Id &&
                x.Type == AssetType.RealEstate &&
                x.IsDraft)
        ), Times.Once);
    }

    protected override IStage GetTestStage() => GetStage<BuyBigRealEstate>();
}

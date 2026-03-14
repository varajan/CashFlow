using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.BigOpportunityStages;
using Moq;

namespace CashFlow.Tests.Stages.SmallCircleTests.BigOpportunityStages.BuyBusinessStages;

[TestFixture]
public class BuyBusinessTests : StagesBaseTest
{
    private static readonly string[] Names = ["Shop", "Market"];

    [SetUp]
    public void Setup()
    {
        AvailableAssetsMock.Setup(x => x.GetAsText(AssetType.BusinessType, It.IsAny<Language>())).Returns(Names);
        PersonServiceMock.Setup(a => a.ReadAllAssets(AssetType.BusinessType, CurrentUser)).Returns([]);
    }

    [Test]
    public void BuyBusiness_Question_and_Buttons()
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
    public async Task BuyBusiness_SelectInvalidValue_StayOnStage()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("Farm");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BuyBusiness>());
    }

    [TestCaseSource(nameof(Names))]
    public async Task BuyBusiness_SelectValidValue_MoveForward(string title)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(title.ToLower());

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BuyBusinessPrice>());

        PersonServiceMock.Verify(a => a.CreateAsset(
            CurrentUser,
            It.Is<AssetDto>(x =>
                x.Title == title &&
                x.Qtty == 1 &&
                x.UserId == CurrentUser.Id &&
                x.Type == AssetType.Business &&
                x.IsDraft)
        ), Times.Once);
    }

    protected override IStage GetTestStage() =>
        new BuyBusiness(TermsServiceMock.Object, AvailableAssetsMock.Object, PersonServiceMock.Object, UserRepositoryMock.Object)
        .SetCurrentUser(CurrentUser);
}

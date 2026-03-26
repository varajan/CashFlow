using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Stages;
using CashFlow.Stages.BigCircleStages;
using Moq;

namespace CashFlow.Tests.Stages.BigCircleTests.BuyBigBusinessStages;

[TestFixture]
public class BuyBigBusinessTests : StagesBaseTest
{
    private static readonly string[] Names = ["Shop", "Market"];

    [SetUp]
    public void Setup()
    {
        AvailableAssetsMock.Setup(x => x.GetAsText(AssetType.BigBusinessType, It.IsAny<Language>())).Returns(Names);
        PersonServiceMock.Setup(a => a.ReadAllAssets(AssetType.BigBusinessType, CurrentUser)).Returns([]);
    }

    [Test]
    public void BuyBigBusiness_Question_and_Buttons()
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
    public async Task BuyBigBusiness_SelectInvalidValue_StayOnStage()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("Farm");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BuyBigBusiness>());
    }

    [TestCaseSource(nameof(Names))]
    public async Task BuyBigBusiness_SelectValidValue_MoveForward(string title)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(title.ToLower());

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BuyBigBusinessPrice>());

        PersonServiceMock.Verify(a => a.CreateAsset(
            CurrentUser,
            It.Is<AssetDto>(x =>
                x.Title == title &&
                x.Qtty == 1 &&
                x.UserId == CurrentUser.Id &&
                x.Type == AssetType.BigBusinessType &&
                x.IsDraft)
        ), Times.Once);
    }

    protected override IStage GetTestStage() => GetStage<BuyBigBusiness>();
}

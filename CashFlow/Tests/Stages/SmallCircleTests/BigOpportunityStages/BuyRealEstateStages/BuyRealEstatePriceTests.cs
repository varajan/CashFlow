using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.BigOpportunityStages;
using Moq;

namespace CashFlow.Tests.Stages.SmallCircleTests.BigOpportunityStages.BuyRealEstateStages;

[TestFixture]
public class BuyRealEstatePriceTests : StagesBaseTest
{
    private static readonly string[] Prices = ["$100", "$500"];
    private AssetDto Asset => new() { Id = 123, UserId = CurrentUserMock.Object.Id, Type = AssetType.RealEstate, IsDraft = true };

    private List<AssetDto> AssetsList = [];

    [SetUp]
    public void Setup()
    {
        AssetsList = [];
        AvailableAssetsMock.Setup(x => x.GetAsCurrency(AssetType.RealEstateBigBuyPrice)).Returns(Prices);
        PersonManagerMock.Setup(a => a.ReadAllAssets(AssetType.RealEstate, CurrentUserMock.Object.Id)).Returns([Asset]);
        AssetManagerMock
            .Setup(a => a.Update(It.IsAny<AssetDto>()))
            .Callback<AssetDto>(dto =>
                AssetsList.Add(dto.Clone())
            );
    }

    [Test]
    public void BuyRealEstatePrice_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();
        var buttons = Prices.Append("Cancel");

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo("What is the price?"));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        });
    }

    [TestCase("a")]
    [TestCase("-1")]
    [TestCase("0")]
    [TestCase(" ")]
    [TestCase("$")]
    public async Task BuyRealEstatePrice_SelectInvalidPrice_StayOnStage(string count)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(count);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BuyBigRealEstatePrice>());
    }

    [TestCaseSource(nameof(Prices))]
    [TestCase("1000")]
    public async Task BuyRealEstatePrice_SelectValidCount_MoveForward(string price)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(price);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.NextStage, Is.TypeOf<BuyBigRealEstateFirstPayment>());
            PersonManagerMock.Verify(a => a.UpdateAsset(It.Is<AssetDto>(a => a.IsDraft && a.Price == price.AsCurrency())), Times.Once);
        });
    }

    protected override IStage GetTestStage() => new BuyBigRealEstatePrice(
        TermsServiceMock.Object,
        AvailableAssetsMock.Object,
        PersonManagerMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}

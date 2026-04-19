using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.BigOpportunityStages;
using Moq;

namespace CashFlowUnitTests.Stages.SmallCircleTests.BigOpportunityStages.BuyRealEstateStages;

[TestFixture]
public class BuyRealEstatePriceTests : StagesBaseTest
{
    private static readonly string[] Prices = CashFlow.Data.Consts.Prices.RealEstateBigBuyPrice.AsCurrency().ToArray();
    private AssetDto Asset => new() { Id = 123, UserId = CurrentUser.Id, Type = AssetType.RealEstate, IsDraft = true };

    private List<AssetDto> AssetsList = [];

    [SetUp]
    public void Setup()
    {
        AssetsList = [];
        PersonServiceMock.Setup(a => a.ReadAllAssets(AssetType.RealEstate, CurrentUser)).Returns([Asset]);
        PersonServiceMock
            .Setup(a => a.UpdateAsset(CurrentUser, It.IsAny<AssetDto>()))
            .Callback<UserDto, AssetDto>((user, dto) =>
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
        using (Assert.EnterMultipleScope())
        {
            Assert.That(testStage.Message, Is.EqualTo("What is the price?"));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        }
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
        using (Assert.EnterMultipleScope())
        {
            Assert.That(testStage.NextStage, Is.TypeOf<BuyBigRealEstateFirstPayment>());
            PersonServiceMock.Verify(a => a.UpdateAsset(CurrentUser, It.Is<AssetDto>(a => a.IsDraft && a.Price == price.AsCurrency())), Times.Once);
        }
    }

    protected override IStage GetTestStage() => GetStage<BuyBigRealEstatePrice>();
}

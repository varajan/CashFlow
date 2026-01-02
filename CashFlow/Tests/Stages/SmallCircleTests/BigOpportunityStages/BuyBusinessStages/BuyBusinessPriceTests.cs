using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.BigOpportunityStages;
using Moq;

namespace CashFlow.Tests.Stages.SmallCircleTests.BigOpportunityStages.BuyBusinessStages;

[TestFixture]
public class BuyBusinessPriceTests : StagesBaseTest
{
    private static readonly string[] Prices = ["$100", "$500"];
    private AssetDto Asset => new() { Id = 123, UserId = CurrentUserMock.Object.Id, Type = AssetType.Business, IsDraft = true };

    private List<AssetDto> AssetsList = [];

    [SetUp]
    public void Setup()
    {
        AssetsList = [];
        AvailableAssetsMock.Setup(x => x.GetAsCurrency(AssetType.BusinessBuyPrice)).Returns(Prices);
        AssetManagerMock.Setup(a => a.ReadAll(AssetType.Business, CurrentUserMock.Object.Id)).Returns([Asset]);
        AssetManagerMock
            .Setup(a => a.Update(It.IsAny<AssetDto>()))
            .Callback<AssetDto>(dto =>
                AssetsList.Add(dto.Clone())
            );
    }

    [Test]
    public void BuyBusinessPrice_Question_and_Buttons()
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
    public async Task BuyBusinessPrice_SelectInvalidPrice_StayOnStage(string count)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(count);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BuyBusinessPrice>());
    }

    [TestCaseSource(nameof(Prices))]
    [TestCase("1000")]
    public async Task BuyBusinessPrice_SelectValidCount_MoveForward(string price)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(price);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.NextStage, Is.TypeOf<BuyBusinessFirstPayment>());
            AssetManagerMock.Verify(a => a.Update(It.Is<AssetDto>(a => a.IsDraft && a.Price == price.AsCurrency())), Times.Once);
        });
    }

    protected override IStage GetTestStage() => new BuyBusinessPrice(
        TermsServiceMock.Object,
        AvailableAssetsMock.Object,
        PersonManagerMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}

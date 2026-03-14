using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Interfaces;
using CashFlow.Stages;
using CashFlow.Stages.BigCircleStages;
using Moq;

namespace CashFlow.Tests.Stages.BigCircleTests.BuyBigBusinessStages;

[TestFixture]
public class BuyBigBusinessPriceTests : StagesBaseTest
{
    private static readonly string[] Prices = ["$100,000", "$500,000"];
    private AssetDto Asset => new() { Id = 123, UserId = CurrentUser.Id, Type = AssetType.BigBusinessType, IsDraft = true };

    private List<AssetDto> AssetsList = [];

    [SetUp]
    public void Setup()
    {
        AssetsList = [];
        AvailableAssetsMock.Setup(x => x.GetAsCurrency(AssetType.BigBusinessBuyPrice)).Returns(Prices);
        PersonServiceMock.Setup(a => a.ReadAllAssets(AssetType.BigBusinessType, CurrentUser)).Returns([Asset]);
        PersonServiceMock
            .Setup(a => a.UpdateAsset(CurrentUser, It.IsAny<AssetDto>()))
            .Callback<UserDto, AssetDto>((user, dto) =>
                AssetsList.Add(dto.Clone())
            );
    }

    [Test]
    public void BuyBigBusinessPrice_Question_and_Buttons()
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
    public async Task BuyBigBusinessPrice_SelectInvalidValue_StayOnStage(string count)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(count);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BuyBigBusinessPrice>());
    }

    [TestCaseSource(nameof(Prices))]
    [TestCase("1000")]
    public async Task BuyBigBusinessPrice_SelectValidValue_MoveForward(string price)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(price);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.NextStage, Is.TypeOf<BuyBigBusinessCashFlow>());
            PersonServiceMock.Verify(a => a.UpdateAsset(CurrentUser, It.Is<AssetDto>(a => a.IsDraft && a.Price == price.AsCurrency())), Times.Once);
        });
    }

    protected override IStage GetTestStage() => new BuyBigBusinessPrice(
        TermsServiceMock.Object,
        AvailableAssetsMock.Object,
        PersonServiceMock.Object,
        UserRepositoryMock.Object)
        .SetCurrentUser(CurrentUser);
}

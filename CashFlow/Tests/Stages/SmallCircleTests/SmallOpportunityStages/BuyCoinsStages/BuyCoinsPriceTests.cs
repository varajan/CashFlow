using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Interfaces;
using CashFlow.Extensions;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.BuyCoinsStages;
using Moq;

namespace CashFlow.Tests.Stages.SmallCircleTests.SmallOpportunityStages.BuyCoinsStages;

[TestFixture]
public class BuyCoinsPriceTests : StagesBaseTest
{
    private static readonly string[] Prices = ["$100", "$500"];
    private AssetDto Asset => new() { Id = 123, UserId = CurrentUserMock.Object.Id, Type = AssetType.Coin, Qtty = 5, IsDraft = true };

    private List<AssetDto> AssetsList = [];

    [SetUp]
    public void Setup()
    {
        AssetsList = [];
        AvailableAssetsMock.Setup(x => x.GetAsCurrency(AssetType.CoinBuyPrice)).Returns(Prices);
        PersonManagerMock.Setup(a => a.ReadAllAssets(AssetType.Coin, CurrentUserMock.Object)).Returns([Asset]);
        PersonManagerMock
            .Setup(a => a.UpdateAsset(CurrentUserMock.Object, It.IsAny<AssetDto>()))
            .Callback<ICashFlowUser, AssetDto>((user, dto) =>
                AssetsList.Add(dto.Clone())
            );
    }

    [Test]
    public void BuyCoinsPrice_Question_and_Buttons()
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
    public async Task BuyCoinsPrice_SelectInvalidPrice_StayOnStage(string count)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(count);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BuyCoinsPrice>());
    }

    [TestCaseSource(nameof(Prices))]
    [TestCase("1000")]
    public async Task BuyCoinsPrice_SelectValidCount_MoveForward(string price)
    {
        // Arrange
        var testStage = GetTestStage();
        var person = new PersonDto { Cash = 10_000 };
        var personCash = person.Cash - price.AsCurrency() * Asset.Qtty;

        PersonManagerMock.Setup(x => x.Read(CurrentUserMock.Object)).Returns(person);

        // Act
        await testStage.HandleMessage(price);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.NextStage, Is.TypeOf<Start>());

            Assert.That(AssetsList, Has.Count.EqualTo(2));
            Assert.That(AssetsList[0].Price, Is.EqualTo(price.AsCurrency()));
            Assert.That(AssetsList[0].IsDraft, Is.True);
            Assert.That(AssetsList[1].Price, Is.EqualTo(price.AsCurrency()));
            Assert.That(AssetsList[1].IsDraft, Is.False);

            PersonManagerMock.Verify(m => m.Update(It.Is<PersonDto>(x => x.Cash == personCash)), Times.Once);

            PersonManagerMock.Verify(x => x.AddHistory(
                ActionType.BuyCoins,
                Asset.Qtty,
                It.Is<ICashFlowUser>(x => x.Id == CurrentUserMock.Object.Id),
                Asset.Id
            ), Times.Once);
        });
    }

    [TestCase(100, 1, 100, false)]
    [TestCase(101, 2, 50, false)]
    [TestCase(100, 1, 101, true)]
    [TestCase(101, 2, 51, true)]
    public async Task BuyCoinsPrice_SelectValidCount_MoveForward(int cash, int qtty, int price, bool creditIsNeeded)
    {
        // Arrange
        var testStage = GetTestStage();
        var person = new PersonDto { Cash = cash };
        var nextStage = creditIsNeeded ? typeof(BuyCoinsCredit) : typeof(Start);
        var asset = Asset.Clone();

        asset.Qtty = qtty;
        asset.Price = price;

        PersonManagerMock.Setup(a => a.ReadAllAssets(AssetType.Coin, CurrentUserMock.Object)).Returns([asset]);
        PersonManagerMock.Setup(x => x.Read(CurrentUserMock.Object)).Returns(person);

        // Act
        await testStage.HandleMessage($"${price}");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.NextStage, Is.TypeOf(nextStage));

            Assert.That(AssetsList, Has.Count.EqualTo(creditIsNeeded ? 1 : 2), "Asset update count");
            Assert.That(AssetsList[0].Price, Is.EqualTo(price));
            Assert.That(AssetsList[0].IsDraft, Is.True);

            PersonManagerMock.Verify(m => m.Update(It.IsAny<PersonDto>()), Times.Exactly(creditIsNeeded ? 0 : 1));
            PersonManagerMock.Verify(x => x.AddHistory(ActionType.BuyCoins,
                asset.Qtty,
                It.IsAny<ICashFlowUser>(),
                asset.Id), Times.Exactly(creditIsNeeded ? 0 : 1));
        });
    }

    protected override IStage GetTestStage() => new BuyCoinsPrice(
            TermsServiceMock.Object,
            AvailableAssetsMock.Object,
            PersonManagerMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}

using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.StocksStages;
using CashFlowUnitTests.Stages;
using Moq;

namespace CashFlowUnitTests.Stages.SmallCircleTests.SmallOpportunityStages.StocksStages.BuyStocksStages;

[TestFixture]
public class BuyStocksCountTests : StagesBaseTest
{
    private PersonDto TestPerson => new() { Id = CurrentUser.Id, Cash = 300 };
    private AssetDto Asset => new() { Id = 123, UserId = CurrentUser.Id, Price = 50, Type = AssetType.Stock, IsDraft = true };

    private List<AssetDto> AssetsList = [];

    [SetUp]
    public void Setup()
    {
        PersonServiceMock.Setup(a => a.ReadAllAssets(AssetType.Stock, CurrentUser)).Returns([Asset]);
        PersonServiceMock.Setup(p => p.Read(CurrentUser)).Returns(TestPerson);

        AssetsList = [];
        PersonServiceMock
            .Setup(a => a.UpdateAsset(CurrentUser, It.IsAny<AssetDto>()))
            .Callback<UserDto, AssetDto>((user, dto) =>
                AssetsList.Add(dto.Clone())
            );
    }

    [TestCase(100, 50, "How much?", new string[] { "100", "150", "200", "Cancel" })]
    [TestCase(100, 100, "You can buy up to 1 stocks. How much stocks would you like to buy?", new string[] { "1", "Cancel" })]
    [TestCase(20, 999, "You can buy up to 49 stocks. How much stocks would you like to buy?", new string[] { "49", "Cancel" })]
    [TestCase(20, 1000, "You can buy up to 50 stocks. How much stocks would you like to buy?", new string[] { "50", "Cancel" })]
    [TestCase(20, 1250, "You can buy up to 62 stocks. How much stocks would you like to buy?", new string[] { "50", "62", "Cancel" })]
    [TestCase(999, 1998, "You can buy up to 2 stocks. How much stocks would you like to buy?", new string[] { "2", "Cancel" })]
    [TestCase(1000, 0, "How much?", new string[] { "1", "2", "3", "4", "Cancel" })]
    [TestCase(1000, 1000, "You can buy up to 1 stocks. How much stocks would you like to buy?", new string[] { "1", "Cancel" })]
    [TestCase(1000, 4000, "You can buy up to 4 stocks. How much stocks would you like to buy?", new string[] { "1", "2", "3", "4", "Cancel" })]
    [TestCase(10, 1500, "You can buy up to 150 stocks. How much stocks would you like to buy?", new string[] { "50", "100", "150", "Cancel" })]
    public void ButtonsAndMessage_Test(int price, int cash, string expectedMessage, string[] expectedButtons)
    {
        // Arrange
        var testStage = GetTestStage();

        var asset = new AssetDto
        {
            UserId = CurrentUser.Id,
            Type = AssetType.Stock,
            Price = price,
            IsDraft = true
        };
        PersonServiceMock.Setup(a => a.ReadAllAssets(AssetType.Stock, CurrentUser)).Returns([asset]);

        var person = new PersonDto() { Id = CurrentUser.Id, Cash = cash };
        PersonServiceMock.Setup(p => p.Read(CurrentUser)).Returns(person);

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo(expectedMessage));
            Assert.That(testStage.Buttons, Is.EqualTo(expectedButtons));
        });
    }

    [TestCase("a")]
    [TestCase("-1")]
    [TestCase("0")]
    [TestCase(" ")]
    public async Task BuyStocksCount_SelectInvalidCount_StayOnStage(string count)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(count);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BuyStocksCount>());
    }

    [Test]
    public async Task BuyStocksCount_SelectValidCount_MoveForward([Values("1", "6")] string count)
    {
        // Arrange
        var testStage = GetTestStage();
        var person = TestPerson.Clone();
        var personCash = person.Cash - Asset.Price * count.AsCurrency();

        // Act
        await testStage.HandleMessage(count.ToLower());

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        Assert.That(AssetsList, Has.Count.EqualTo(2));
        Assert.That(AssetsList[0].Qtty, Is.EqualTo(count.AsCurrency()));
        Assert.That(AssetsList[0].IsDraft, Is.True);
        Assert.That(AssetsList[1].Qtty, Is.EqualTo(count.AsCurrency()));
        Assert.That(AssetsList[1].IsDraft, Is.False);

        PersonServiceMock.Verify(m => m.Update(It.Is<PersonDto>(x => x.Cash == personCash)), Times.Once);

        PersonServiceMock.Verify(x => x.AddHistory(
            ActionType.BuyStocks,
            count.AsCurrency(),
            It.Is<UserDto>(x => x.Id == CurrentUser.Id),
            Asset.Id
        ), Times.Once);
    }

    [Test]
    public async Task BuyStocksCount_SelectTooManyCount_MoveToCredit()
    {
        // Arrange
        var testStage = GetTestStage();
        var count = TestPerson.Cash / Asset.Price + 1;

        // Act
        await testStage.HandleMessage($"{count}");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BuyStocksCredit>());

        PersonServiceMock.Verify(m => m.UpdateAsset(CurrentUser, It.Is<AssetDto>(x => x.Id == Asset.Id && x.Qtty == count && x.IsDraft)), Times.Once);
    }

    protected override IStage GetTestStage() => GetStage<BuyStocksCount>();
}

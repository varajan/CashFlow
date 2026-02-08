using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Data.Users;
using CashFlow.Extensions;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.StocksStages;
using Moq;

namespace CashFlow.Tests.Stages.SmallCircleTests.SmallOpportunityStages.StocksStages.BuyStocksStages;

[TestFixture]
public class BuyStocksCountTests : StagesBaseTest
{
    private PersonDto TestPerson => new() { Id = CurrentUserMock.Object.Id, Cash = 300 };
    private AssetDto Asset => new() { Id = 123, UserId = CurrentUserMock.Object.Id, Price = 50, Type = AssetType.Stock, IsDraft = true };
    
    private List<AssetDto> AssetsList = [];

    [SetUp]
    public void Setup()
    {
        PersonManagerMock.Setup(a => a.ReadAllAssets(AssetType.Stock, CurrentUserMock.Object)).Returns([Asset]);
        PersonManagerMock.Setup(p => p.Read(CurrentUserMock.Object)).Returns(TestPerson);

        AssetsList = [];
        PersonManagerMock
            .Setup(a => a.UpdateAsset(CurrentUserMock.Object, It.IsAny<AssetDto>()))
            .Callback<IUser, AssetDto>((user, dto) =>
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
            UserId = CurrentUserMock.Object.Id,
            Type = AssetType.Stock,
            Price = price,
            IsDraft = true
        };
        PersonManagerMock.Setup(a => a.ReadAllAssets(AssetType.Stock, CurrentUserMock.Object)).Returns([asset]);

        var person = new PersonDto() { Id = CurrentUserMock.Object.Id, Cash = cash };
        PersonManagerMock.Setup(p => p.Read(CurrentUserMock.Object)).Returns(person);

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

        PersonManagerMock.Verify(m => m.Update(It.Is<PersonDto>(x => x.Cash == personCash)), Times.Once);

        PersonManagerMock.Verify(x => x.AddHistory(
            ActionType.BuyStocks,
            count.AsCurrency(),
            It.Is<IUser>(x => x.Id == CurrentUserMock.Object.Id),
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

        PersonManagerMock.Verify(m => m.UpdateAsset(CurrentUserMock.Object, It.Is<AssetDto>(x => x.Id == Asset.Id && x.Qtty == count && x.IsDraft)), Times.Once);
    }

    protected override IStage GetTestStage() => new BuyStocksCount(
        TermsServiceMock.Object,
        AvailableAssetsMock.Object,
        PersonManagerMock.Object)
    .SetCurrentUser(CurrentUserMock.Object)
    .SetAllUsers(OtherUsers);
}

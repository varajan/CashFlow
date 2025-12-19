using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.MarketStages;
using Moq;
using MoreLinq;

namespace CashFlow.Tests.Stages.SmallCircleTests.MarketStages;

[TestFixture]
public class MarketTests : StagesBaseTest
{
    [SetUp]
    public void Setup()
    {
        var assetTypes = new[]
        {
            AssetType.RealEstate,
            AssetType.Land,
            AssetType.Business,
            AssetType.Coin,
            AssetType.SmallBusiness
        };

        assetTypes.ForEach(t => AssetManagerMock.Setup(a => a.ReadAll(t, CurrentUserMock.Object.Id)) .Returns([ new AssetDto() ]));
    }

    [Test]
    public void Market_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();
        var buttons = new List<string>
        {
            "Sell Real Estate",
            "Sell Land",
            "Sell Business",
            "Sell Coins",
            "Increase cash flow",
            "Cancel"
        };

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo("What do you want?"));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        });
    }

    [TestCase("Sell Real Estate", typeof(SellRealEstate))]
    [TestCase("Sell Land", typeof(SellLand))]
    [TestCase("Sell Business", typeof(SellBusiness))]
    [TestCase("Sell Coins", typeof(SellCoins))]
    [TestCase("Increase cash flow", typeof(IncreaseCashflow))]
    public async Task Market_SelectValidOption(string message, Type nextStage)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(message);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf(nextStage));
    }

    [Test]
    public async Task Market_Select_SellRealEstate_WithoutAny()
    {
        // Arrange
        var testStage = GetTestStage();
        AssetManagerMock.Setup(a => a.ReadAll(AssetType.RealEstate, CurrentUserMock.Object.Id)).Returns([]);

        // Act
        await testStage.HandleMessage("Sell Real Estate");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());
        CurrentUserMock.Verify(x => x.Notify("You have no properties."), Times.Once);
    }

    [Test]
    public async Task Market_Select_SellLand_WithoutAny()
    {
        // Arrange
        var testStage = GetTestStage();
        AssetManagerMock.Setup(a => a.ReadAll(AssetType.Land, CurrentUserMock.Object.Id)).Returns([]);

        // Act
        await testStage.HandleMessage("Sell Land");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());
        CurrentUserMock.Verify(x => x.Notify("You have no Land."), Times.Once);
    }

    [Test]
    public async Task Market_Select_SellBusiness_WithoutAny()
    {
        // Arrange
        var testStage = GetTestStage();
        AssetManagerMock.Setup(a => a.ReadAll(AssetType.Business, CurrentUserMock.Object.Id)).Returns([]);
        AssetManagerMock.Setup(a => a.ReadAll(AssetType.SmallBusiness, CurrentUserMock.Object.Id)).Returns([]);

        // Act
        await testStage.HandleMessage("Sell Business");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());
        CurrentUserMock.Verify(x => x.Notify("You have no Business."), Times.Once);
    }

    [Test]
    public async Task Market_Select_SellBusiness_WithSmallOnly()
    {
        // Arrange
        var testStage = GetTestStage();
        AssetManagerMock.Setup(a => a.ReadAll(AssetType.SmallBusiness, CurrentUserMock.Object.Id)).Returns([]);

        // Act
        await testStage.HandleMessage("Sell Business");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SellBusiness>());
    }

    [Test]
    public async Task Market_Select_SellBusiness_WithSome()
    {
        // Arrange
        var testStage = GetTestStage();
        AssetManagerMock.Setup(a => a.ReadAll(AssetType.Business, CurrentUserMock.Object.Id)).Returns([]);

        // Act
        await testStage.HandleMessage("Sell Business");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SellBusiness>());
    }

    [Test]
    public async Task Market_Select_SellCoins_WithoutAny()
    {
        // Arrange
        var testStage = GetTestStage();
        AssetManagerMock.Setup(a => a.ReadAll(AssetType.Coin, CurrentUserMock.Object.Id)).Returns([]);

        // Act
        await testStage.HandleMessage("Sell Coins");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());
        CurrentUserMock.Verify(x => x.Notify("You have no coins."), Times.Once);
    }

    [Test]
    public async Task Market_Select_IncreaseCashflow_WithoutAny()
    {
        // Arrange
        var testStage = GetTestStage();
        AssetManagerMock.Setup(a => a.ReadAll(AssetType.SmallBusiness, CurrentUserMock.Object.Id)).Returns([]);

        // Act
        await testStage.HandleMessage("Increase cash flow");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());
        CurrentUserMock.Verify(x => x.Notify("You have no small Business."), Times.Once);
    }

    [Test]
    public async Task Market_SelectInValidOption()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("message");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Market>());
    }

    protected override IStage GetTestStage() => new Market(TermsServiceMock.Object, AssetManagerMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);

    [Test]
    public void NotImplemented()
    {
#if DEBUG
        Assert.Fail("Not Implemented.");
#endif
    }
}
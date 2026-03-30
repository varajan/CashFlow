using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.BankruptcyStages;
using Moq;
using MoreLinq;

namespace CashFlowUnitTests.Stages.SmallCircleTests.BankruptcyStages;

[TestFixture]
public class BankruptcySellAsetsTests : StagesBaseTest
{
    private static List<AssetDto> Assets =>
    [
        new AssetDto { Id = 1, Qtty = 1, Title = "Asset 1", Price = 1_000, CashFlow = 10, IsDeleted = false, Type = AssetType.Business },
        new AssetDto { Id = 2, Qtty = 2, Title = "Asset 2", Price = 2_000, CashFlow = 20, IsDeleted = true , Type = AssetType.RealEstate },
        new AssetDto { Id = 3, Qtty = 3, Title = "Asset 3", Price = 3_000, CashFlow = 10, IsDeleted = false, Type = AssetType.Stock },
        new AssetDto { Id = 4, Qtty = 4, Title = "Asset 4", Price = 4_000, CashFlow = 20, IsDeleted = false, Type = AssetType.RealEstate },
    ];

    private static readonly List<LiabilityDto> Liabilities =
    [
        new() { Type = Liability.Bank_Loan, FullAmount = 3_000, Cashflow = -300, IsBankruptcyDivisible = false },
        new() { Type = Liability.Car_Loan, FullAmount = 10_000, Cashflow = -450, IsBankruptcyDivisible = true },
    ];

    private static PersonDto TestPerson => new()
    {
        Cash = 100,
        Salary = -500,
        Assets = Assets,
        Liabilities = Liabilities,
        Bankruptcy = true
    };

    [SetUp]
    public void Setup() => PersonServiceMock.Setup(p => p.Read(It.IsAny<UserDto>())).Returns(TestPerson);

    [Test]
    public void BankruptcySellAssets_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();
        string[] buttons = ["#1", "#2", "#3", "Stop Game", "History"];
        var message = @"*You're out of money.*
Bank Loan: *$3,000*
Cashflow: *-$1,130*
Cash: *$100*

You have to sell your assets till you cash flow is positive.

What asset do you want to sell?

#1 - *Asset 3* - Price: $4,500, Cashflow: $30
#2 - *Asset 1* - Price: $500, Cashflow: $10
#3 - *Asset 4* - Price: $2,000, Cashflow: $80";

        // Act

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(testStage.Message, Is.EqualTo(message));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        }
    }

    [Test, Ignore("Not applicable")]
    public override Task Stage_CanBeCanceled() => Task.CompletedTask;

    [Test]
    public async Task BankruptcySellAssets_CanNotBeCanceled()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("Cancel");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BankruptcySellAssets>());
    }

    [Test]
    public async Task BankruptcySellAssets_CanStopGame()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("Stop Game");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<StopGame>());
    }

    [Test]
    public async Task BankruptcySellAssets_CanOpenHistory()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("History");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<History>());
    }

    [Test]
    public async Task BankruptcySellAssets_SellAsset_SelectValidAsset()
    {
        // Arrange
        var testStage = GetTestStage();
        var asset = Assets.First(a => a.Title == "Asset 1");
        var sellPrice = asset.Qtty * asset.Price / 2;

        // Act
        await testStage.HandleMessage("#2");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BankruptcySellAssets>());

        PersonServiceMock.Verify(p => p.Update(It.Is<PersonDto>(p => p.Cash == TestPerson.Cash + sellPrice)), Times.Once);
        PersonServiceMock.Verify(p => p.SellAsset(It.Is<AssetDto>(a => a.Title == asset.Title), sellPrice, CurrentUser), Times.Once);
        NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, $"Sale for debts: Asset 1, Price: $500"), Times.Once);
    }

    [TestCase("0")]
    [TestCase("#4")]
    public async Task BankruptcySellAssets_SellAsset_SelectInvalidAsset(string message)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(message);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BankruptcySellAssets>());

        PersonServiceMock.Verify(p => p.Update(It.IsAny<PersonDto>()), Times.Never);
        PersonServiceMock.Verify(p => p.UpdateAsset(CurrentUser, It.IsAny<AssetDto>()), Times.Never);
        PersonServiceMock.Verify(p => p.AddHistory(It.IsAny<ActionType>(), It.IsAny<long>(), It.IsAny<UserDto>()), Times.Never);
    }

    [Test]
    public async Task BankruptcySellAssets_SellAsset_ReduceBankLoan()
    {
        // Arrange
        var testStage = GetTestStage();
        var asset = Assets.First(a => a.Title == "Asset 3");
        var bankLoanAmount = Liabilities.First(l => l.Type == Liability.Bank_Loan).FullAmount;

        // Act
        await testStage.HandleMessage("#1");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BankruptcySellAssets>());

        PersonServiceMock.Verify(p => p.Update(It.Is<PersonDto>(p => p.Cash == TestPerson.Cash - Liabilities[0].FullAmount + Assets[2].Price / 2)), Times.Exactly(2));
        PersonServiceMock.Verify(p => p.SellAsset(It.Is<AssetDto>(a => a.Title == asset.Title), asset.Qtty * asset.Price / 2, CurrentUser), Times.Once);
        PersonServiceMock.Verify(p => p.AddHistory(ActionType.BankLoan, bankLoanAmount, CurrentUser), Times.Once);
    }

    [Test]
    public async Task BankruptcySellAssets_SellAsset_ReduceBankLoan_ProcessBankruptcy()
    {
        // Arrange
        var testStage = GetTestStage();
        var bankLoanAmount = Liabilities.First(l => l.Type == Liability.Bank_Loan).FullAmount;
        var assets = Assets.Clone();
        var person = TestPerson.Clone();

        assets.Skip(1).ForEach(a => a.IsDeleted = true);
        person.Assets = assets;

        PersonServiceMock.Setup(p => p.Read(It.IsAny<UserDto>())).Returns(person);

        // Act
        await testStage.HandleMessage("#1");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Bankruptcy>());
    }

    protected override IStage GetTestStage() => GetStage<BankruptcySellAssets>();
}

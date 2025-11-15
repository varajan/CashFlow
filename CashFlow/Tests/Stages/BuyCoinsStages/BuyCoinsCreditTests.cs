using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Stages;
using Moq;

namespace CashFlow.Tests.Stages.BuyCoinsStages;

[TestFixture]
public class BuyCoinsCreditTests : StagesBaseTest
{
    private PersonDto TestPerson => new() { Id = CurrentUserMock.Object.Id, Cash = 300 };
    private AssetDto Asset => new() { Id = 123, UserId = CurrentUserMock.Object.Id, Qtty = 5, Price = 100, Type = AssetType.Coin, IsDraft = true };

    [SetUp]
    public void Setup()
    {
        AssetManagerMock.Setup(a => a.ReadAll(AssetType.Coin, TestPerson.Id)).Returns([Asset]);
        PersonManagerMock.Setup(p => p.Read(TestPerson.Id)).Returns(TestPerson);
    }

    [Test]
    public async Task BuyCoinsCredit_CanBeCanceled()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("cancel");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        AssetManagerMock.Verify(a => a.Delete(
            It.Is<AssetDto>(x =>
                x.UserId == CurrentUserMock.Object.Id &&
                x.Type == AssetType.Coin)
        ), Times.Once);

        PersonManagerMock.Verify(x => x.Update(It.IsAny<PersonDto>()), Times.Never, "No person data should be updated");
    }

    [Test]
    public void BuyCoinsCredit_Question_and_Buttons()
    {
        // Arrange
        var coinsPrice = Asset.Qtty * Asset.Price;
        var testStage = GetTestStage();

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo($"You don''t have {coinsPrice.AsCurrency()}, but only {TestPerson.Cash.AsCurrency()}"));
            Assert.That(testStage.Buttons, Is.EqualTo(new List<string> { "Get Credit", "Cancel" }));
        });
    }

    [Test]
    public async Task BuyCoinsCredit_InvalidInput_IsIgnored()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("hello-world");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BuyCoinsCredit>());
    }

    [Test]
    public async Task BuyCoinsCredit_ToBank_Confirmed_IsCompleted()
    {
        // Arrange
        var amount = Asset.Qtty * Asset.Price;
        var creditAmount = (int)Math.Ceiling((amount - TestPerson.Cash) / 1_000d) * 1_000;
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("Get credit");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        CurrentUserMock.Verify(u => u.GetCredit(creditAmount), Times.Once);
        HistoryManagerMock.Verify(x => x.Add(ActionType.BuyCoins, Asset.Id, CurrentUserMock.Object), Times.Once);

        AssetManagerMock.Verify(a => a.Update(
            It.Is<AssetDto>(x =>
                x.UserId == TestPerson.Id &&
                x.Type == AssetType.Coin &&
                !x.IsDraft)
        ), Times.Once);

        PersonManagerMock.Verify(p => p.Update(
            It.Is<PersonDto>(x =>
            x.Id == TestPerson.Id &&
            x.Cash == TestPerson.Cash - amount)
            ), Times.Once);
    }

    protected override IStage GetTestStage() => new BuyCoinsCredit(
            TermsServiceMock.Object,
            AvailableAssetsMock.Object,
            HistoryManagerMock.Object,
            PersonManagerMock.Object,
            AssetManagerMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}

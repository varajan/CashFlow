using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Stages;
using CashFlow.Stages.BigCircleStages;
using CashFlowUnitTests.Stages;
using Moq;

namespace CashFlowUnitTests.Stages.BigCircleTests.BuyBigBusinessStages;

[TestFixture]
public class BuyBigBusinessFirstPaymentTests : StagesBaseTest
{
    private static readonly string[] FirstPayments = ["$10,000", "$50,000"];
    private AssetDto Asset => new() { Id = 123, UserId = CurrentUser.Id, Type = AssetType.BigBusinessType, Price = 100_000, Qtty = 1, IsDraft = true };
    private PersonDto TestPerson => new() { Id = CurrentUser.Id, Cash = 200_000 };

    [SetUp]
    public void Setup()
    {
        PersonServiceMock.Setup(p => p.Read(CurrentUser)).Returns(TestPerson);
        PersonServiceMock.Setup(a => a.ReadAllAssets(AssetType.BigBusinessType, CurrentUser)).Returns([Asset]);
        AvailableAssetsMock.Setup(x => x.GetAsCurrency(AssetType.BigBusinessCashFlow)).Returns(FirstPayments);
    }

    [Test]
    public void BuyBigBusinessFirstPayment_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();
        var buttons = FirstPayments.Append("Cancel");

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo("What is the first payment?"));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        });
    }

    [TestCase("-1")]
    [TestCase("0")]
    public async Task BuyBigBusinessFirstPayment_SelectInvalidPrice_StayOnStage(string BuyPrice)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(BuyPrice);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BuyBigBusinessFirstPayment>());
    }

    [TestCaseSource(nameof(FirstPayments))]
    [TestCase("1000")]
    public async Task BuyBigBusinessFirstPayment_SelectValidValue_MoveForward(string BuyPrice)
    {
        // Arrange
        var testStage = GetTestStage();
        var personCash = TestPerson.Cash - BuyPrice.AsCurrency();
        var price = Asset.Price;
        var mortgage = price - BuyPrice.AsCurrency();

        // Act
        await testStage.HandleMessage(BuyPrice);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.NextStage, Is.TypeOf<BuyBigBusinessCashFlow>());
            PersonServiceMock.Verify(a => a.UpdateAsset(
                CurrentUser,
                It.Is<AssetDto>(x =>
                    x.Price == price &&
                    x.Mortgage == mortgage &&
                    x.IsDraft)),
                Times.Once);
        });
    }

    [Test]
    public async Task BuyBigBusinessFirstPayment_NotEnoughCash_Canceled()
    {
        // Arrange
        var testStage = GetTestStage();
        var price = TestPerson.Cash + 1;

        // Act
        await testStage.HandleMessage($"{price}");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());
        NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, "You don't have enough money."), Times.Once);
        PersonServiceMock.Verify(p => p.DeleteAsset(CurrentUser, It.Is<AssetDto>(a => a.IsDraft)), Times.Once);
    }

    protected override IStage GetTestStage() => GetStage<BuyBigBusinessFirstPayment>();
}

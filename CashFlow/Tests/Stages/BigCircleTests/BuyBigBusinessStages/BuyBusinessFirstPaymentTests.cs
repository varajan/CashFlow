using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Stages;
using CashFlow.Stages.BigCircleStages;
using Moq;

namespace CashFlow.Tests.Stages.BigCircleTests.BuyBigBusinessStages;

[TestFixture]
public class BuyBigBusinessFirstPaymentTests : StagesBaseTest
{
    private static readonly string[] FirstPayments = ["$10,000", "$50,000"];
    private AssetDto Asset => new() { Id = 123, UserId = CurrentUserMock.Object.Id, Type = AssetType.BigBusinessType, Price = 100_000, Qtty = 1, IsDraft = true };
    private PersonDto TestPerson => new() { Id = CurrentUserMock.Object.Id, Cash = 200_000 };

    [SetUp]
    public void Setup()
    {
        PersonManagerMock.Setup(p => p.Read(CurrentUserMock.Object)).Returns(TestPerson);
        PersonManagerMock.Setup(a => a.ReadAllAssets(AssetType.BigBusinessType, CurrentUserMock.Object)).Returns([Asset]);
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
            PersonManagerMock.Verify(a => a.UpdateAsset(
                CurrentUserMock.Object,
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
        CurrentUserMock.Verify(u => u.Notify("You don't have enough money."), Times.Once);
        PersonManagerMock.Verify(p => p.DeleteAsset(CurrentUserMock.Object, It.Is<AssetDto>(a => a.IsDraft)), Times.Once);
    }

    protected override IStage GetTestStage() => new BuyBigBusinessFirstPayment(
            TermsServiceMock.Object,
            AvailableAssetsMock.Object,
            PersonManagerMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}

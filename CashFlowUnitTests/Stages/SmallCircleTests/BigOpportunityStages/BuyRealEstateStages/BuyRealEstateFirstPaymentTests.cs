using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.BigOpportunityStages;
using Moq;

namespace CashFlowUnitTests.Stages.SmallCircleTests.BigOpportunityStages.BuyRealEstateStages;

[TestFixture]
public class BuyRealEstateFirstPaymentTests : StagesBaseTest
{
    private static readonly string[] FirstPayments = Prices.RealEstateBigFirstPayment.OrderBy(x => x).AsCurrency().ToArray();
    private AssetDto Asset => new() { Id = 123, UserId = CurrentUser.Id, Type = AssetType.RealEstate, Price = 10_000, Qtty = 1, IsDraft = true };
    private PersonDto TestPerson => new() { Id = CurrentUser.Id, Cash = 10_000 };

    [SetUp]
    public void Setup()
    {
        PersonServiceMock.Setup(p => p.Read(CurrentUser)).Returns(TestPerson);
        PersonServiceMock.Setup(a => a.ReadAllAssets(AssetType.RealEstate, CurrentUser)).Returns([Asset]);
    }

    [Test]
    public void BuyRealEstateFirstPayment_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();
        var buttons = FirstPayments.Append("Cancel");

        // Act

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(testStage.Message, Is.EqualTo("What is the first payment?"));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        }
    }

    [TestCase("-1")]
    public async Task BuyRealEstateFirstPayment_SelectInvalidPrice_StayOnStage(string count)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(count);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BuyBigRealEstateFirstPayment>());
    }

    [TestCaseSource(nameof(FirstPayments))]
    [TestCase("1000")]
    [TestCase("0")]
    public async Task BuyRealEstateFirstPayment_SelectValidCount_MoveForward(string firstPayment)
    {
        // Arrange
        var testStage = GetTestStage();
        var person = new PersonDto { Cash = 200_000 };
        var personCash = person.Cash - firstPayment.AsCurrency();
        var price = Asset.Price;
        var mortgage = price - firstPayment.AsCurrency();

        PersonServiceMock.Setup(x => x.Read(CurrentUser)).Returns(person);

        // Act
        await testStage.HandleMessage(firstPayment);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(testStage.NextStage, Is.TypeOf<BuyBigRealEstateCashFlow>());
            PersonServiceMock.Verify(a => a.UpdateAsset(
                CurrentUser,
                It.Is<AssetDto>(x =>
                    x.Price == price &&
                    x.Mortgage == mortgage &&
                    x.IsDraft)),
                Times.Once);
        }
    }

    [TestCase(100, 100, false)]
    [TestCase(100, 101, true)]
    public async Task BuyRealEstateFirstPayment_SelectValidCount_MoveForward(int cash, int firstPayment, bool creditIsNeeded)
    {
        // Arrange
        var testStage = GetTestStage();
        var person = new PersonDto { Cash = cash };
        var nextStage = creditIsNeeded ? typeof(BuyBigRealEstateCredit) : typeof(BuyBigRealEstateCashFlow);
        var asset = Asset.Clone();
        asset.Price = firstPayment;

        PersonServiceMock.Setup(x => x.Read(CurrentUser)).Returns(person);
        PersonServiceMock.Setup(a => a.ReadAllAssets(AssetType.RealEstate, CurrentUser)).Returns([asset]);

        // Act
        await testStage.HandleMessage($"${firstPayment}");

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(testStage.NextStage, Is.TypeOf(nextStage));
            PersonServiceMock.Verify(a => a.UpdateAsset(CurrentUser, It.Is<AssetDto>(x => x.Price == firstPayment && x.IsDraft)), Times.Once);
        }
    }

    protected override IStage GetTestStage() => GetStage<BuyBigRealEstateFirstPayment>();
}

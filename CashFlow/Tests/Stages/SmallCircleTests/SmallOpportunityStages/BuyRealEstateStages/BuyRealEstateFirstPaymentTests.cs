using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Data.Users;
using CashFlow.Extensions;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages;
using Moq;

namespace CashFlow.Tests.Stages.SmallCircleTests.SmallOpportunityStages.BuyRealEstateStages;

[TestFixture]
public class BuyRealEstateFirstPaymentTests : StagesBaseTest
{
    private static readonly string[] FirstPayments = ["$100", "$500"];
    private AssetDto Asset => new() { Id = 123, UserId = CurrentUserMock.Object.Id, Type = AssetType.RealEstate, Price = 10_000, Qtty = 1, IsDraft = true };
    private PersonDto TestPerson => new() { Id = CurrentUserMock.Object.Id, Cash = 10_000 };

    private List<AssetDto> AssetsList = [];

    [SetUp]
    public void Setup()
    {
        AssetsList = [];
        PersonManagerMock.Setup(p => p.Read(TestPerson.Id)).Returns(TestPerson);
        AvailableAssetsMock.Setup(x => x.GetAsCurrency(AssetType.RealEstateSmallFirstPayment)).Returns(FirstPayments);
        AssetManagerMock.Setup(a => a.ReadAll(AssetType.RealEstate, CurrentUserMock.Object.Id)).Returns([Asset]);
        AssetManagerMock
            .Setup(a => a.Update(It.IsAny<AssetDto>()))
            .Callback<AssetDto>(dto =>
                AssetsList.Add(dto.Clone())
            );
    }

    [Test]
    public void BuyRealEstateFirstPayment_Question_and_Buttons()
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
    public async Task BuyRealEstateFirstPayment_SelectInvalidPrice_StayOnStage(string count)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(count);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BuySmallRealEstateFirstPayment>());
    }

    [TestCaseSource(nameof(FirstPayments))]
    [TestCase("1000")]
    [TestCase("0")]
    public async Task BuyRealEstateFirstPayment_SelectValidCount_MoveForward(string firstPayment)
    {
        // Arrange
        var testStage = GetTestStage();
        var person = new PersonDto { Cash = 10_000 };
        var personCash = person.Cash - firstPayment.AsCurrency();
        var price = Asset.Price;
        var mortgage = price - firstPayment.AsCurrency();

        PersonManagerMock.Setup(x => x.Read(CurrentUserMock.Object.Id)).Returns(person);

        // Act
        await testStage.HandleMessage(firstPayment);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.NextStage, Is.TypeOf<Start>());

            Assert.That(AssetsList, Has.Count.EqualTo(2));
            Assert.That(AssetsList[0].Price, Is.EqualTo(price));
            Assert.That(AssetsList[0].Mortgage, Is.EqualTo(mortgage));
            Assert.That(AssetsList[0].IsDraft, Is.True);

            Assert.That(AssetsList[1].Price, Is.EqualTo(price));
            Assert.That(AssetsList[1].Mortgage, Is.EqualTo(mortgage));
            Assert.That(AssetsList[1].IsDraft, Is.False);

            PersonManagerMock.Verify(m => m.Update(It.Is<PersonDto>(x => x.Cash == personCash)), Times.Once);

            HistoryManagerMock.Verify(m => m.Add(
                ActionType.BuyRealEstate,
                Asset.Id,
                It.Is<IUser>(x => x.Id == CurrentUserMock.Object.Id)
            ), Times.Once);
        });
    }

    [TestCase(100, 100, false)]
    [TestCase(100, 101, true)]
    public async Task BuyRealEstateFirstPayment_SelectValidCount_MoveForward(int cash, int firstPayment, bool creditIsNeeded)
    {
        // Arrange
        var testStage = GetTestStage();
        var person = new PersonDto { Cash = cash };
        var nextStage = creditIsNeeded ? typeof(BuySmallRealEstateCredit) : typeof(Start);
        var asset = Asset.Clone();

        //asset.Qtty = qtty;
        asset.Price = firstPayment;

        AssetManagerMock.Setup(a => a.ReadAll(AssetType.RealEstate, CurrentUserMock.Object.Id)).Returns([asset]);
        PersonManagerMock.Setup(x => x.Read(CurrentUserMock.Object.Id)).Returns(person);

        // Act
        await testStage.HandleMessage($"${firstPayment}");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.NextStage, Is.TypeOf(nextStage));

            Assert.That(AssetsList, Has.Count.EqualTo(creditIsNeeded ? 1 : 2), "Asset update count");
            Assert.That(AssetsList[0].Price, Is.EqualTo(firstPayment));
            Assert.That(AssetsList[0].IsDraft, Is.True);

            PersonManagerMock.Verify(m => m.Update(It.IsAny<PersonDto>()), Times.Exactly(creditIsNeeded ? 0 : 1));
            HistoryManagerMock.Verify(m => m.Add(ActionType.BuyRealEstate,
                asset.Id,
                It.IsAny<IUser>()), Times.Exactly(creditIsNeeded ? 0 : 1));
        });
    }

    protected override IStage GetTestStage() => new BuySmallRealEstateFirstPayment(
            TermsServiceMock.Object,
            AvailableAssetsMock.Object,
            AssetManagerMock.Object,
            HistoryManagerMock.Object,
            PersonManagerMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}

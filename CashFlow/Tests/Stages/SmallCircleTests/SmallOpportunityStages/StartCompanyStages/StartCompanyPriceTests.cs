using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Data.Users;
using CashFlow.Extensions;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages;
using Moq;

namespace CashFlow.Tests.Stages.SmallCircleTests.SmallOpportunityStages.StartCompanyStages;

[TestFixture]
public class StartCompanyPriceTests: StagesBaseTest
{
    private static readonly string[] CompanyPrices = ["$100", "$500"];
    private AssetDto Asset => new() { Id = 123, UserId = CurrentUserMock.Object.Id, Type = AssetType.SmallBusinessType, IsDraft = true };

    private List<AssetDto> AssetsList = [];

    [SetUp]
    public void Setup()
    {
        AssetsList = [];
        AvailableAssetsMock.Setup(x => x.GetAsCurrency(AssetType.SmallBusinessBuyPrice)).Returns(CompanyPrices);
        AssetManagerMock.Setup(a => a.ReadAll(AssetType.SmallBusinessType, CurrentUserMock.Object.Id)).Returns([Asset]);
        AssetManagerMock
            .Setup(a => a.Update(It.IsAny<AssetDto>()))
            .Callback<AssetDto>(dto =>
                AssetsList.Add(dto.Clone())
            );
    }

    [Test]
    public void StartCompanyPrice_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();
        var buttons = CompanyPrices.Append("Cancel");

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
    public async Task StartCompanyPrice_SelectInvalidPrice_StayOnStage(string count)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(count);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<StartCompanyPrice>());
    }

    [TestCaseSource(nameof(CompanyPrices))]
    [TestCase("1000")]
    public async Task StartCompanyPrice_SelectValidCount_MoveForward(string price)
    {
        // Arrange
        var testStage = GetTestStage();
        var person = new PersonDto { Cash = 10_000 };
        var personCash = person.Cash - price.AsCurrency();

        PersonManagerMock.Setup(x => x.Read(CurrentUserMock.Object.Id)).Returns(person);

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

            HistoryManagerMock.Verify(m => m.Add(
                ActionType.StartCompany,
                Asset.Id,
                It.Is<IUser>(x => x.Id == CurrentUserMock.Object.Id)
            ), Times.Once);
        });
    }

    [TestCase(100, 100, false)]
    [TestCase(100, 101, true)]
    public async Task StartCompanyPrice_SelectValidCount_MoveForward(int cash, int price, bool creditIsNeeded)
    {
        // Arrange
        var testStage = GetTestStage();
        var person = new PersonDto { Cash = cash };
        var nextStage = creditIsNeeded ? typeof(StartCompanyCredit) : typeof(Start);
        var asset = Asset.Clone();

        asset.Price = price;

        AssetManagerMock.Setup(a => a.ReadAll(AssetType.Coin, CurrentUserMock.Object.Id)).Returns([asset]);
        PersonManagerMock.Setup(x => x.Read(CurrentUserMock.Object.Id)).Returns(person);

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
            HistoryManagerMock.Verify(m => m.Add(ActionType.StartCompany,
                asset.Id,
                It.IsAny<IUser>()), Times.Exactly(creditIsNeeded ? 0 : 1));
        });
    }

    protected override IStage GetTestStage() => new StartCompanyPrice(
            TermsServiceMock.Object,
            AvailableAssetsMock.Object,
            AssetManagerMock.Object,
            HistoryManagerMock.Object,
            PersonManagerMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}

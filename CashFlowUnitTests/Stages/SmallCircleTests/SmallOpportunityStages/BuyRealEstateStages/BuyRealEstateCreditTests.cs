using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages;
using Moq;

namespace CashFlow.Tests.Stages.SmallCircleTests.SmallOpportunityStages.BuyRealEstateStages;

[TestFixture]
public class BuyRealEstateCreditTests : StagesBaseTest
{
    private PersonDto TestPerson => new() { Id = CurrentUser.Id, Cash = 300 };
    private AssetDto Asset => new() { Id = 123, UserId = CurrentUser.Id, Price = 10_000, Mortgage = 8_500, Qtty = 1, Type = AssetType.RealEstate, IsDraft = true };

    [SetUp]
    public void Setup()
    {
        PersonServiceMock.Setup(a => a.ReadAllAssets(AssetType.RealEstate, CurrentUser)).Returns([Asset]);
        PersonServiceMock.Setup(p => p.Read(CurrentUser)).Returns(TestPerson);
    }

    [Test]
    public async Task BuyRealEstateCredit_CanBeCanceled()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("cancel");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        PersonServiceMock.Verify(a => a.DeleteAsset(
            CurrentUser,
            It.Is<AssetDto>(x =>
                x.UserId == CurrentUser.Id &&
                x.Type == AssetType.RealEstate)
        ), Times.Once);

        PersonServiceMock.Verify(x => x.Update(It.IsAny<PersonDto>()), Times.Never, "No person data should be updated");
    }

    [Test]
    public void BuyRealEstateCredit_Question_and_Buttons()
    {
        // Arrange
        var firstPayment = Asset.Price - Asset.Mortgage;
        var testStage = GetTestStage();

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo($"You don''t have {firstPayment.AsCurrency()}, but only {TestPerson.Cash.AsCurrency()}"));
            Assert.That(testStage.Buttons, Is.EqualTo(new List<string> { "Get Credit", "Cancel" }));
        });
    }

    [Test]
    public async Task BuyRealEstateCredit_InvalidInput_IsIgnored()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("hello-world");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BuySmallRealEstateCredit>());
    }

    [Test]
    public async Task BuyRealEstateCredit_Confirmed_MoveNext()
    {
        // Arrange
        var firstPayment = Asset.Price - Asset.Mortgage;
        var creditAmount = (int)Math.Ceiling((firstPayment - TestPerson.Cash) / 1_000d) * 1_000;
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("Get credit");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BuySmallRealEstateCashFlow>());

        NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, $"You've taken {creditAmount.AsCurrency()} from bank."), Times.Once);
        PersonServiceMock.Verify(a => a.UpdateAsset(CurrentUser, It.IsAny<AssetDto>()), Times.Never);
    }

    protected override IStage GetTestStage() => GetStage<BuySmallRealEstateCredit>();
}

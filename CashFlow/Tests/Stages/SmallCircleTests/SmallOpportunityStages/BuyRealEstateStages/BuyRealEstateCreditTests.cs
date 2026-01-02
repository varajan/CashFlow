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
    private PersonDto TestPerson => new() { Id = CurrentUserMock.Object.Id, Cash = 300 };
    private AssetDto Asset => new() { Id = 123, UserId = CurrentUserMock.Object.Id, Price = 10_000, Mortgage = 8_500, Qtty = 1, Type = AssetType.RealEstate, IsDraft = true };

    [SetUp]
    public void Setup()
    {
        PersonManagerMock.Setup(a => a.ReadAllAssets(AssetType.RealEstate, TestPerson.Id)).Returns([Asset]);
        PersonManagerMock.Setup(p => p.Read(TestPerson.Id)).Returns(TestPerson);
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

        PersonManagerMock.Verify(a => a.DeleteAsset(
            It.Is<AssetDto>(x =>
                x.UserId == CurrentUserMock.Object.Id &&
                x.Type == AssetType.RealEstate)
        ), Times.Once);

        PersonManagerMock.Verify(x => x.Update(It.IsAny<PersonDto>()), Times.Never, "No person data should be updated");
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

        CurrentUserMock.Verify(u => u.GetCredit_OBSOLETE(creditAmount), Times.Once);
        PersonManagerMock.Verify(a => a.UpdateAsset(It.IsAny<AssetDto>()), Times.Never);
    }

    protected override IStage GetTestStage() => new BuySmallRealEstateCredit(
            TermsServiceMock.Object,
            AvailableAssetsMock.Object,
            PersonManagerMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}

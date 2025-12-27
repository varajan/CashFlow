using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.BigOpportunityStages;
using Moq;

namespace CashFlow.Tests.Stages.SmallCircleTests.BigOpportunityStages.BuyBusinessStages;

[TestFixture]
public class BuyBusinessCreditTests : StagesBaseTest
{
    private PersonDto TestPerson => new() { Id = CurrentUserMock.Object.Id, Cash = 300 };
    private AssetDto Asset => new() { Id = 123, UserId = CurrentUserMock.Object.Id, Price = 10_000, Mortgage = 8_500, Qtty = 1, Type = AssetType.Business, IsDraft = true };

    [SetUp]
    public void Setup()
    {
        AssetManagerMock.Setup(a => a.ReadAll(AssetType.Business, TestPerson.Id)).Returns([Asset]);
        PersonManagerMock.Setup(p => p.Read(TestPerson.Id)).Returns(TestPerson);
    }

    [Test]
    public async Task BuyBusinessCredit_CanBeCanceled()
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
                x.Type == AssetType.Business)
        ), Times.Once);

        PersonManagerMock.Verify(x => x.Update(It.IsAny<PersonDto>()), Times.Never, "No person data should be updated");
    }

    [Test]
    public void BuyBusinessCredit_Question_and_Buttons()
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
    public async Task BuyBusinessCredit_InvalidInput_IsIgnored()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("hello-world");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BuyBusinessCredit>());
    }

    [Test]
    public async Task BuyBusinessCredit_Confirmed_MoveNext()
    {
        // Arrange
        var firstPayment = Asset.Price - Asset.Mortgage;
        var creditAmount = (int)Math.Ceiling((firstPayment - TestPerson.Cash) / 1_000d) * 1_000;
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("Get credit");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BuyBusinessCashFlow>());

        CurrentUserMock.Verify(u => u.GetCredit_OBSOLETE(creditAmount), Times.Once);
        AssetManagerMock.Verify(a => a.Update(It.IsAny<AssetDto>()), Times.Never);
    }

    protected override IStage GetTestStage() => new BuyBusinessCredit(
            TermsServiceMock.Object,
            AvailableAssetsMock.Object,
            AssetManagerMock.Object,
            PersonManagerMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}

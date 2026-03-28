using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages;
using CashFlowUnitTests.Stages;
using Moq;

namespace CashFlowUnitTests.Stages.SmallCircleTests.SmallOpportunityStages.StartCompanyStages;

[TestFixture]
public class StartCompanyCreditTests : StagesBaseTest
{
    private PersonDto TestPerson => new() { Id = CurrentUser.Id, Cash = 300 };
    private AssetDto Asset => new() { Id = 123, UserId = CurrentUser.Id, Price = 500, Qtty = 1, Type = AssetType.SmallBusinessType, IsDraft = true };

    [SetUp]
    public void Setup()
    {
        PersonServiceMock.Setup(a => a.ReadAllAssets(AssetType.SmallBusinessType, CurrentUser)).Returns([Asset]);
        PersonServiceMock.Setup(p => p.Read(CurrentUser)).Returns(TestPerson);
    }

    [Test]
    public async Task StartCompanyCredit_CanBeCanceled()
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
                x.Type == AssetType.SmallBusinessType &&
                x.IsDraft)
        ), Times.Once);

        PersonServiceMock.Verify(x => x.Update(It.IsAny<PersonDto>()), Times.Never, "No person data should be updated");
    }

    [Test]
    public void StartCompanyCredit_Question_and_Buttons()
    {
        // Arrange
        var price = Asset.Price;
        var testStage = GetTestStage();

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo($"You don''t have {price.AsCurrency()}, but only {TestPerson.Cash.AsCurrency()}"));
            Assert.That(testStage.Buttons, Is.EqualTo(new List<string> { "Get Credit", "Cancel" }));
        });
    }

    [Test]
    public async Task StartCompanyCredit_InvalidInput_IsIgnored()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("hello-world");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<StartCompanyCredit>());
    }

    [Test]
    public async Task StartCompanyCredit_ToBank_Confirmed_IsCompleted()
    {
        // Arrange
        var amount = Asset.Price;
        var creditAmount = (int)Math.Ceiling((amount - TestPerson.Cash) / 1_000d) * 1_000;
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("Get credit");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, $"You've taken {creditAmount.AsCurrency()} from bank."), Times.Once);

        PersonServiceMock.Verify(x => x.AddHistory(ActionType.StartCompany, Asset.Qtty, CurrentUser, Asset.Id), Times.Once);

        PersonServiceMock.Verify(a => a.UpdateAsset(
            CurrentUser,
            It.Is<AssetDto>(x =>
                x.UserId == TestPerson.Id &&
                x.Type == AssetType.SmallBusinessType &&
                x.IsDraft == false)
        ), Times.Once);

        PersonServiceMock.Verify(p => p.Update(
            It.Is<PersonDto>(x =>
            x.Id == TestPerson.Id &&
            x.Cash == TestPerson.Cash + creditAmount - amount)
            ), Times.Exactly(2));
    }

    protected override IStage GetTestStage() => GetStage<StartCompanyCredit>();
}

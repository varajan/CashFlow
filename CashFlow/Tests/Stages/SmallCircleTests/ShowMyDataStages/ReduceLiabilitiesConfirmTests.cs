using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Interfaces;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.ShowMyDataStages;
using Moq;

namespace CashFlow.Tests.Stages.SmallCircleTests.ShowMyDataStages;

[TestFixture]
public class ReduceLiabilitiesConfirmTests : StagesBaseTest
{
    private List<LiabilityDto> Liabilities =>
    [
        new() { Type = Liability.Car_Loan, FullAmount = 50_000, Cashflow = -5100, MarkedForReduction = true,  AllowsPartialPayment = false, Deleted = false },
        new() { Type = Liability.Boat_Loan, FullAmount = 5_000,  Cashflow = -500,  MarkedForReduction = true,  AllowsPartialPayment = true , Deleted = false },
        new() { Type = Liability.Mortgage, FullAmount = 50_000, Cashflow = -5100, MarkedForReduction = false, AllowsPartialPayment = false, Deleted = true },
        new() { Type = Liability.School_Loan, FullAmount = 5_000,  Cashflow = -500,  MarkedForReduction = false, AllowsPartialPayment = true , Deleted = true },
    ];

    private PersonDto TestPerson => new() { Cash = 50_250, Liabilities = Liabilities };

    [SetUp]
    public void Setup() => PersonManagerMock.Setup(x => x.Read(It.IsAny<ICashFlowUser>())).Returns(TestPerson);

    [Test]
    public void ReduceLiabilitiesConfirm_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo("Reduce Liabilities - Car Loan. Yes?"));
            Assert.That(testStage.Buttons, Is.EqualTo(new[] { "Yes", "Cancel" }));
        });
    }

    [Test, Ignore("Not applicable")]
    public override Task Stage_CanBeCanceled() => Task.CompletedTask;

    [Test]
    public async Task ReduceLiabilitiesConfirm_CanBeCanceled([Values ("Cancel", "No", "123")] string value)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(value);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<ReduceLiabilities>());

        PersonManagerMock.Verify(p => p.AddHistory(ActionType.ReduceLiability, It.IsAny<long>(), CurrentUserMock.Object), Times.Never);
        PersonManagerMock.Verify(p => p.Update(It.IsAny<ICashFlowUser>(), It.IsAny<LiabilityDto>()), Times.Exactly(2));

        PersonManagerMock.Verify(p => p.Update(It.IsAny<ICashFlowUser>(), It.Is<LiabilityDto>(l =>
            l.Type == Liability.Car_Loan &&
            l.MarkedForReduction == false)),
        Times.Once);

        PersonManagerMock.Verify(p => p.Update(It.IsAny<ICashFlowUser>(), It.Is<LiabilityDto>(l =>
            l.Type == Liability.Boat_Loan &&
            l.MarkedForReduction == false)),
        Times.Once);
    }

    [Test]
    public async Task ReduceLiabilitiesConfirm_ReduceLast()
    {
        // Arrange
        var testStage = GetTestStage();

        var liabilities = new List<LiabilityDto>
        {
            new() { Type = Liability.Car_Loan,  FullAmount = 50_000, Cashflow = -5100, MarkedForReduction = true,  AllowsPartialPayment = false, Deleted = false },
            new() { Type = Liability.Bank_Loan, FullAmount = 0, Cashflow = -5000, MarkedForReduction = false, AllowsPartialPayment = false, Deleted = true },
        };
        var amount = liabilities.First().FullAmount;
        var person = new PersonDto { Cash = 100_000, Liabilities = liabilities };
        var liability = liabilities.First();

        PersonManagerMock.Setup(x => x.Read(CurrentUserMock.Object)).Returns(person);

        // Act
        await testStage.HandleMessage("Yes");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        PersonManagerMock.Verify(p => p.AddHistory((ActionType)liability.Type, amount, CurrentUserMock.Object), Times.Once);
        PersonManagerMock.Verify(p => p.Update(CurrentUserMock.Object,
            It.Is<LiabilityDto>(l => l.Type == liability.Type && l.Deleted == true)), Times.Once);
    }

    [Test]
    public async Task ReduceLiabilitiesConfirm_Confirm()
    {
        // Arrange
        var testStage = GetTestStage();

        var liabilities = new List<LiabilityDto>
        {
            new() { Type = Liability.Car_Loan,  FullAmount = 50_000, Cashflow = -5100, MarkedForReduction = true,  AllowsPartialPayment = false, Deleted = false },
            new() { Type = Liability.Bank_Loan, FullAmount = 50_000, Cashflow = -5000, MarkedForReduction = false, AllowsPartialPayment = false, Deleted = false },
        };
        var amount = liabilities.First().FullAmount;
        var person = new PersonDto { Cash = 100_000, Liabilities = liabilities };
        var liability = liabilities.First();

        PersonManagerMock.Setup(x => x.Read(CurrentUserMock.Object)).Returns(person);

        // Act
        await testStage.HandleMessage("Yes");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<ReduceLiabilities>());

        PersonManagerMock.Verify(p => p.AddHistory((ActionType)liability.Type, amount, CurrentUserMock.Object), Times.Once);
        PersonManagerMock.Verify(p => p.Update(CurrentUserMock.Object,
            It.Is<LiabilityDto>(l => l.Type == liability.Type && l.Deleted == true)), Times.Once);
    }

    protected override IStage GetTestStage() => new ReduceLiabilitiesConfirm(TermsServiceMock.Object, PersonManagerMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}

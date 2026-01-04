using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.ShowMyDataStages;
using Moq;

namespace CashFlow.Tests.Stages.SmallCircleTests.ShowMyDataStages;

[TestFixture]
public class GetCreditTests : StagesBaseTest
{
    [Test]
    public void GetCredit_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();
        var buttons = new List<string> { "1000", "2000", "5000", "10 000", "20 000", "Cancel" };

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo("How much?"));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        });
    }

    [TestCase("0")]
    [TestCase("900")]
    [TestCase("1900")]
    public async Task GetCredit_SelectInvalidAmount(string amount)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(amount);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<GetCredit>());
        CurrentUserMock.Verify(u => u.Notify("Invalid amount. The amount must be a multiple of 1000"), Times.Once);
    }

    [TestCase("1000")]
    [TestCase("$2000")]
    public async Task GetCredit_NoCreditExist_SelectValidAmount(string amount)
    {
        // Arrange
        var testStage = GetTestStage();
        var initialCash = 300;
        var person = new PersonDto() { Id = CurrentUserMock.Object.Id, Cash = initialCash };

        PersonManagerMock.Setup(p => p.Read(CurrentUserMock.Object)).Returns(person);

        // Act
        await testStage.HandleMessage(amount);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        CurrentUserMock.Verify(u => u.Notify($"You've taken {amount.AsCurrency().AsCurrency()} from bank."), Times.Once);

        PersonManagerMock.Verify(p => p.Update(
            It.Is<PersonDto>(x =>
            x.Id == person.Id &&
            x.Cash == initialCash + amount.AsCurrency())
            ), Times.Once);

        Assert.That(person.Liabilities.Count, Is.EqualTo(1), "One liability should be added");
        Assert.That(person.Liabilities[0].Name, Is.EqualTo(Liability.Bank_Loan), "Liability name should be 'Bank Loan'");
        Assert.That(person.Liabilities[0].FullAmount, Is.EqualTo(amount.AsCurrency()), "'amount' should be added to 'Bank Loan'");
        Assert.That(person.Liabilities[0].Cashflow, Is.EqualTo(-amount.AsCurrency() / 10), "'percent' should be added to 'Bank Loan'");
    }

    [TestCase("1000")]
    [TestCase("$2000")]
    public async Task GetCredit_CreditExist_SelectValidAmount(string amount)
    {
        // Arrange
        var testStage = GetTestStage();
        var initialCash = 300;
        var initialLoanAmount = 5000;
        var initialLoanCashflow = -500;
        var person = new PersonDto()
        {
            Id = CurrentUserMock.Object.Id,
            Cash = initialCash,
            Liabilities = [new() { Name = Liability.Bank_Loan, FullAmount = initialLoanAmount, Cashflow = initialLoanCashflow } ] };

        PersonManagerMock.Setup(p => p.Read(CurrentUserMock.Object)).Returns(person);

        // Act
        await testStage.HandleMessage(amount);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        CurrentUserMock.Verify(u => u.Notify($"You've taken {amount.AsCurrency().AsCurrency()} from bank."), Times.Once);

        PersonManagerMock.Verify(p => p.Update(
            It.Is<PersonDto>(x =>
            x.Id == person.Id &&
            x.Cash == initialCash + amount.AsCurrency())
            ), Times.Once);

        Assert.That(person.Liabilities.Count, Is.EqualTo(1), "One liability should be added");
        Assert.That(person.Liabilities[0].Name, Is.EqualTo(Liability.Bank_Loan), "Liability name should be 'Bank Loan'");
        Assert.That(person.Liabilities[0].FullAmount, Is.EqualTo(initialLoanAmount + amount.AsCurrency()), "'amount' should be added to 'Bank Loan'");
        Assert.That(person.Liabilities[0].Cashflow, Is.EqualTo(initialLoanCashflow - amount.AsCurrency() / 10), "'percent' should be added to 'Bank Loan'");
    }

    protected override IStage GetTestStage() => new GetCredit(TermsServiceMock.Object, PersonManagerMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}

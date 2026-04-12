using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.ShowMyDataStages;
using Moq;

namespace CashFlowUnitTests.Stages.SmallCircleTests.ShowMyDataStages;

[TestFixture]
public class ReduceLiabilitiesAmountTests : StagesBaseTest
{
    [TestCase(1000, 5000, new string[] { "$1,000", "Cancel" })]
    [TestCase(7000, 6500, new string[] { "$1,000", "$5,000", "$6,000", "Cancel" })]
    [TestCase(15000, 20000, new string[] { "$1,000", "$5,000", "$10,000", "$15,000", "Cancel" })]
    [TestCase(10000, 4000, new string[] { "$1,000", "$4,000", "Cancel" })]
    [TestCase(5000, 5000, new string[] { "$1,000", "$5,000", "Cancel" })]
    public void ReduceLiabilitiesAmount_Question_and_Buttons(int fullAmount, int cash, string[] buttons)
    {
        // Arrange
        var testStage = GetTestStage();
        var liabilities = new List<LiabilityDto>
        {
            new() { Type = Liability.CarLoan, FullAmount = 50_000, Cashflow = -5100, MarkedForReduction = false },
            new() { Type = Liability.BankLoan, FullAmount = fullAmount, Cashflow = -500, MarkedForReduction = true },
        };
        var person = new PersonDto { Cash = cash, Liabilities = liabilities };

        PersonServiceMock.Setup(x => x.Read(CurrentUser)).Returns(person);

        // Act

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(testStage.Message, Is.EqualTo("How many?"));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        }
    }

    [Test]
    public async Task ReduceLiabilitiesAmount_CanBeCanceled()
    {
        // Arrange
        var testStage = GetTestStage();
        var liabilities = new List<LiabilityDto>
        {
            new() { Type = Liability.OtherPayments, FullAmount = 1_000, Cashflow = -100, MarkedForReduction = false },
            new() { Type = Liability.Taxes, FullAmount = 1_000, Cashflow = -100, MarkedForReduction = true },
            new() { Type = Liability.Mortgage, FullAmount = 1_000, Cashflow = -100, MarkedForReduction = true },
            new() { Type = Liability.BankLoan, FullAmount = 1_000, Cashflow = -100, MarkedForReduction = false },
        };

        PersonServiceMock.Setup(x => x.Read(CurrentUser)).Returns(new PersonDto { Liabilities = liabilities });

        // Act
        await testStage.HandleMessage("Cancel");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        PersonServiceMock.Verify(p => p.Update(It.IsAny<UserDto>(), It.IsAny<LiabilityDto>()), Times.Exactly(2));

        PersonServiceMock.Verify(p => p.Update(It.IsAny<UserDto>(), It.Is<LiabilityDto>(l =>
            l.Type == Liability.Taxes &&
            l.MarkedForReduction == false)),
        Times.Once);

        PersonServiceMock.Verify(p => p.Update(It.IsAny<UserDto>(), It.Is<LiabilityDto>(l =>
            l.Type == Liability.Mortgage &&
            l.MarkedForReduction == false)),
        Times.Once);
    }

    [TestCase(10000, 5000, "1500", "Invalid amount. The amount must be a multiple of 1000")]
    [TestCase(10000, 5000, "500", "Invalid amount. The amount must be a multiple of 1000")]
    [TestCase(10000, 5000, "abc", "Invalid amount. The amount must be a multiple of 1000")]
    [TestCase(10000, 2000, "3000", "You don't have *$3,000*, but only *$2,000*")]
    public async Task ReduceLiabilitiesAmount_SelectInvalidValue(int fullAmount, int cash, string value, string errorMessage)
    {
        // Arrange
        var testStage = GetTestStage();
        var liabilities = new List<LiabilityDto>
        {
            new() { Type = Liability.CarLoan, FullAmount = 50_000, Cashflow = -5100, MarkedForReduction = false },
            new() { Type = Liability.BankLoan, FullAmount = fullAmount, Cashflow = -500, MarkedForReduction = true },
        };
        var person = new PersonDto { Cash = cash, Liabilities = liabilities };

        PersonServiceMock.Setup(x => x.Read(CurrentUser)).Returns(person);

        // Act
        await testStage.HandleMessage(value);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<ReduceLiabilitiesAmount>());
        NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, errorMessage), Times.Once);
    }

    [TestCase("1000")]
    [TestCase("2000")]
    [TestCase("10000")]
    public async Task ReduceLiabilitiesAmount_SelectValidValue(string value)
    {
        // Arrange
        var testStage = GetTestStage();
        var liabilities = new List<LiabilityDto>
        {
            new() { Type = Liability.CarLoan, FullAmount = 50_000, Cashflow = -5100, MarkedForReduction = false, AllowsPartialPayment = true },
            new() { Type = Liability.BankLoan, FullAmount = 50_000, Cashflow = -5000, MarkedForReduction = true, AllowsPartialPayment = true },
        };
        var person = new PersonDto { Cash = 100_000, Liabilities = liabilities };
        var amount = Math.Min(value.AsCurrency(), liabilities[1].FullAmount);

        PersonServiceMock.Setup(x => x.Read(CurrentUser)).Returns(person);

        // Act
        await testStage.HandleMessage(value);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<ReduceLiabilities>());

        PersonServiceMock.Verify(p => p.AddHistory(ActionType.BankLoan, amount, CurrentUser), Times.Once);
        PersonServiceMock.Verify(p => p.Update(CurrentUser,
            It.Is<LiabilityDto>(l =>
                l.Type == Liability.BankLoan &&
                l.Deleted == false &&
                l.MarkedForReduction == false &&
                l.FullAmount == 50_000 - amount &&
                l.Cashflow == -5000 + (decimal)(amount * 0.1))
            ),
            Times.Once);
    }

    [TestCase("50000")]
    [TestCase("99000")]
    public async Task ReduceLiabilitiesAmount_ReduceAll(string value)
    {
        // Arrange
        var testStage = GetTestStage();

        var liabilities = new List<LiabilityDto>
        {
            new() { Type = Liability.CarLoan, FullAmount = 50_000, Cashflow = -5100, MarkedForReduction = true, AllowsPartialPayment = true },
            new() { Type = Liability.BankLoan, FullAmount = 50_000, Cashflow = -5000, MarkedForReduction = false, AllowsPartialPayment = true },
        };
        var amount = liabilities.First().FullAmount;
        var person = new PersonDto { Cash = 100_000, Liabilities = liabilities };
        var liability = liabilities.First();

        PersonServiceMock.Setup(x => x.Read(CurrentUser)).Returns(person);

        // Act
        await testStage.HandleMessage(value);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<ReduceLiabilities>());

        PersonServiceMock.Verify(p => p.AddHistory(ActionType.CarLoan, amount, CurrentUser), Times.Once);
        PersonServiceMock.Verify(p => p.Update(CurrentUser,
            It.Is<LiabilityDto>(l => l.Type == liability.Type && l.Deleted == true)), Times.Once);
    }

    [Test]
    public async Task ReduceLiabilitiesAmount_ReduceLast()
    {
        // Arrange
        var testStage = GetTestStage();

        var liabilities = new List<LiabilityDto>
        {
            new() { Type = Liability.CarLoan,  FullAmount = 50_000, Cashflow = -5100, MarkedForReduction = true,  AllowsPartialPayment = true, Deleted = false },
            new() { Type = Liability.BankLoan, FullAmount = 0, Cashflow = -5000, MarkedForReduction = false, AllowsPartialPayment = true, Deleted = true },
        };
        var amount = liabilities.First().FullAmount;
        var person = new PersonDto { Cash = 100_000, Liabilities = liabilities };
        var liability = liabilities.First();

        PersonServiceMock.Setup(x => x.Read(CurrentUser)).Returns(person);

        // Act
        await testStage.HandleMessage($"{amount}");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        PersonServiceMock.Verify(p => p.AddHistory(ActionType.CarLoan, amount, CurrentUser), Times.Once);
        PersonServiceMock.Verify(p => p.Update(CurrentUser,
            It.Is<LiabilityDto>(l => l.Type == liability.Type && l.Deleted == true)), Times.Once);
    }

    protected override IStage GetTestStage() => GetStage<ReduceLiabilitiesAmount>();
}

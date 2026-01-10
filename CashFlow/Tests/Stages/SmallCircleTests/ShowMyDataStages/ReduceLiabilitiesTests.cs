using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Data.Users;
using CashFlow.Extensions;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.ShowMyDataStages;
using Moq;

namespace CashFlow.Tests.Stages.SmallCircleTests.ShowMyDataStages;

[TestFixture]
public class ReduceLiabilitiesTests : StagesBaseTest
{
    private readonly List<LiabilityDto> Liabilities =
    [
        new() { Type = Liability.Car_Loan, FullAmount = 50_000, Cashflow = -5100, AllowsPartialPayment = false, Deleted = false },
        new() { Type = Liability.Boat_Loan, FullAmount = 5_000,  Cashflow = -500,  AllowsPartialPayment = true , Deleted = false },
        new() { Type = Liability.Mortgage, FullAmount = 50_000, Cashflow = -5100, AllowsPartialPayment = false, Deleted = true },
        new() { Type = Liability.School_Loan, FullAmount = 5_000,  Cashflow = -500,  AllowsPartialPayment = true , Deleted = true },
    ];

    private PersonDto TestPerson => new() { Cash = 50_250, Liabilities = Liabilities };

    [SetUp]
    public void Setup() => PersonManagerMock.Setup(x => x.Read(CurrentUserMock.Object)).Returns(TestPerson);

    [Test]
    public void ReduceLiabilities_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();
        var buttons = Liabilities.Select(x => x.Type.AsString()).Append("Cancel");
        var message = $"*Cash:* $50,250{NL}*Car Loan:* $50,000 - $5,100 monthly{NL}*Boat Loan:* $5,000 - $500 monthly";

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo(message));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        });
    }

    [TestCase("Car loan", 50_000)]
    [TestCase("car Loan", 51_000)]
    [TestCase("Boat Loan", 1_000)]
    [TestCase("Boat Loan", 2_000)]
    [TestCase("boat Loan", 5_000)]
    [TestCase("Boat loan", 10_000)]
    public async Task ReduceLiabilities_EnoughCash(string message, int cash)
    {
        // Arrange
        var testStage = GetTestStage();
        var liability = Liabilities.First(l => l.Type.AsString().Equals(message, StringComparison.InvariantCultureIgnoreCase));

        var testPerson = TestPerson.Clone();
        testPerson.Cash = cash;
        PersonManagerMock.Setup(x => x.Read(CurrentUserMock.Object)).Returns(testPerson);

        // Act
        await testStage.HandleMessage(message);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<ReduceLiabilitiesAmount>());

        PersonManagerMock.Verify(p => p.Update(CurrentUserMock.Object,
            It.Is<LiabilityDto>(l => l.Type == liability.Type && l.MarkedForReduction == true)),
            Times.Once);
    }

    [TestCase("Car Loan", 49_999, 50_000)]
    [TestCase("Car Loan", 999, 50_000)]
    [TestCase("Boat Loan", 999, 1000)]
    [TestCase("Boat Loan", 500, 1000)]
    public async Task ReduceLiabilities_NotEnoughCash(string message, int cash, int required)
    {
        // Arrange
        var testStage = GetTestStage();
        var liability = Liabilities.First(l => l.Type.AsString().Equals(message, StringComparison.InvariantCultureIgnoreCase));

        var testPerson = TestPerson.Clone();
        testPerson.Cash = cash;
        PersonManagerMock.Setup(x => x.Read(CurrentUserMock.Object)).Returns(testPerson);

        // Act
        await testStage.HandleMessage(message);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<ReduceLiabilities>());
        PersonManagerMock.Verify(p => p.Update(It.IsAny<IUser>(), It.IsAny<LiabilityDto>()), Times.Never);
        CurrentUserMock.Verify(u => u.Notify($"You don't have {required.AsCurrency()}, but only {cash.AsCurrency()}"), Times.Once);
    }

    [TestCase("Liability")]
    [TestCase("Mortgage")]
    public async Task ReduceLiabilities_InvalidValue(string message)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(message);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<ReduceLiabilities>());
        PersonManagerMock.Verify(p => p.Update(It.IsAny<IUser>(), It.IsAny<LiabilityDto>()), Times.Never);
        CurrentUserMock.Verify(u => u.Notify(It.IsAny<string>()), Times.Never);
    }

    protected override IStage GetTestStage() => new ReduceLiabilities(TermsServiceMock.Object, PersonManagerMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}

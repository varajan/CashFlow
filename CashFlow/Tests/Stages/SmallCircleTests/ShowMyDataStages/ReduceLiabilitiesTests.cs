using CashFlow.Data.DTOs;
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
        new() { Name = "Liability No1", FullAmount = 50_000, Cashflow = -5100, AllowsPartialPayment = false, Deleted = false },
        new() { Name = "Liability No2", FullAmount = 5_000,  Cashflow = -500,  AllowsPartialPayment = true , Deleted = false },
        new() { Name = "Liability No3", FullAmount = 50_000, Cashflow = -5100, AllowsPartialPayment = false, Deleted = true },
        new() { Name = "Liability No4", FullAmount = 5_000,  Cashflow = -500,  AllowsPartialPayment = true , Deleted = true },
    ];

    private PersonDto TestPerson => new() { Cash = 50_250, Liabilities = Liabilities };

    [SetUp]
    public void Setup() => PersonManagerMock.Setup(x => x.Read(CurrentUserMock.Object.Id)).Returns(TestPerson);

    [Test]
    public void ReduceLiabilities_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();
        var buttons = Liabilities.Select(x => x.Name).Append("Cancel");
        var message = $"*Cash:* $50,250{NL}*Liability No1:* $50,000 - $5,100 monthly{NL}*Liability No2:* $5,000 - $500 monthly";

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo(message));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        });
    }

    [TestCase("liability No1", 50_000)]
    [TestCase("Liability no1", 51_000)]
    [TestCase("liability No2", 1_000)]
    [TestCase("Liability no2", 2_000)]
    [TestCase("Liability No2", 5_000)]
    [TestCase("liability no2", 10_000)]
    public async Task ReduceLiabilities_EnoughCash(string message, int cash)
    {
        // Arrange
        var testStage = GetTestStage();
        var liability = Liabilities.First(l => l.Name.Equals(message, StringComparison.OrdinalIgnoreCase)).Name;

        var testPerson = TestPerson.Clone();
        testPerson.Cash = cash;
        PersonManagerMock.Setup(x => x.Read(CurrentUserMock.Object.Id)).Returns(testPerson);

        // Act
        await testStage.HandleMessage(message);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<ReduceLiabilitiesAmount>());

        PersonManagerMock.Verify(p => p.UpdateLiability(CurrentUserMock.Object.Id,
            It.Is<LiabilityDto>(l => l.Name == liability && l.MarkedForReduction == true)),
            Times.Once);
    }

    [TestCase("Liability No1", 49_999, 50_000)]
    [TestCase("Liability no1", 999, 50_000)]
    [TestCase("Liability No2", 999, 1000)]
    [TestCase("Liability No2", 500, 1000)]
    public async Task ReduceLiabilities_NotEnoughCash(string message, int cash, int required)
    {
        // Arrange
        var testStage = GetTestStage();
        var liability = Liabilities.First(l => l.Name.Equals(message, StringComparison.OrdinalIgnoreCase));

        var testPerson = TestPerson.Clone();
        testPerson.Cash = cash;
        PersonManagerMock.Setup(x => x.Read(CurrentUserMock.Object.Id)).Returns(testPerson);

        // Act
        await testStage.HandleMessage(message);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<ReduceLiabilities>());
        PersonManagerMock.Verify(p => p.UpdateLiability(It.IsAny<long>(), It.IsAny<LiabilityDto>()), Times.Never);
        CurrentUserMock.Verify(u => u.Notify($"You don't have {required.AsCurrency()}, but only {cash.AsCurrency()}"), Times.Once);
    }

    [TestCase("Liability")]
    [TestCase("Liability No3")]
    public async Task ReduceLiabilities_InvalidValue(string message)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(message);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<ReduceLiabilities>());
        PersonManagerMock.Verify(p => p.UpdateLiability(It.IsAny<long>(), It.IsAny<LiabilityDto>()), Times.Never);
        CurrentUserMock.Verify(u => u.Notify(It.IsAny<string>()), Times.Never);
    }

    protected override IStage GetTestStage() => new ReduceLiabilities(TermsServiceMock.Object, PersonManagerMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}

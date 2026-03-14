using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.DoodadsStages;
using Moq;

namespace CashFlow.Tests.Stages.SmallCircleTests.DoodadsStages;

public class PayWithCreditCardTests : StagesBaseTest
{
    public static readonly string[] Amounts = ["$1000", "$5000"];

    [SetUp]
    public void Setup()
    {
        AvailableAssetsMock.Setup(x => x.GetAsCurrency(AssetType.MicroCreditAmount)).Returns(Amounts);
    }

    [Test]
    public void Doodads_PayWithCreditCard_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo("How much?"));
            Assert.That(testStage.Buttons, Is.EqualTo(Amounts.Append("Cancel")));
        });
    }

    [TestCase("a")]
    [TestCase("-1")]
    [TestCase("0")]
    [TestCase(" ")]
    [TestCase("$")]
    public async Task Doodads_PayWithCreditCard_SelectInvalidValue_StayOnStage(string amount)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(amount);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<PayWithCreditCard>());
        NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, "Invalid value. Try again."), Times.Once);
    }

    [TestCase("2000")]
    [TestCaseSource(nameof(Amounts))]
    public async Task Doodads_PayWithCreditCard_SelectValidValue_Payed(string amount)
    {
        // Arrange
        var testStage = GetTestStage();

        var initialCredit = 1000;
        var initialExpenses = -10;
        var testPerson = new PersonDto
        {
            Id = CurrentUser.Id,
            Cash = 5000,
            Liabilities =
            [
                new()
                {
                    Type = Liability.Credit_Card,
                    FullAmount = initialCredit,
                    Cashflow = initialExpenses,
                },
                new()
                {
                    Type = Liability.Small_Credit,
                    FullAmount = 2000,
                    Cashflow = -15,
                },
            ],
        };

        PersonServiceMock.Setup(p => p.Read(CurrentUser)).Returns(testPerson);

        // Act
        await testStage.HandleMessage(amount);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        PersonServiceMock.Verify(p => p.Update(It.Is<PersonDto>(person =>
            person.Id == CurrentUser.Id &&
            person.Liabilities.First(l => l.Type == Liability.Credit_Card).FullAmount == initialCredit + amount.AsCurrency() &&
            person.Liabilities.First(l => l.Type == Liability.Credit_Card).Cashflow == initialExpenses - 0.03 * amount.AsCurrency()
        )), Times.Once);

        PersonServiceMock.Verify(x => x.AddHistory(ActionType.MicroCredit, amount.AsCurrency(), CurrentUser), Times.Once);
    }

    protected override IStage GetTestStage() => new PayWithCreditCard(
        TermsServiceMock.Object,
        AvailableAssetsMock.Object,
        PersonServiceMock.Object,
        UserRepositoryMock.Object)
        .SetCurrentUser(CurrentUser);
}

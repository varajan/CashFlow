using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.BankruptcyStages;
using CashFlow.Stages.SmallCircleStages.ShowMyDataStages;
using Moq;

namespace CashFlow.Tests.Stages.SmallCircleTests.ShowMyDataStages;

[TestFixture]
public class GetMoneyTests : StagesBaseTest
{
    [TestCase(0, new[] { "$1,000", "$2,000", "$5,000", "$0", "Cancel" })]
    [TestCase(-100, new[] { "$1,000", "$2,000", "$5,000", "-$100", "Cancel" })]
    [TestCase(1500, new[] { "$1,000", "$2,000", "$5,000", "$1,500", "Cancel" })]
    [TestCase(2000, new[] { "$1,000", "$2,000", "$5,000", "Cancel" })]
    public void GetMoney_Question_and_Buttons(int cashFlow, string[] buttons)
    {
        // Arrange
        var testStage = GetTestStage();
        var testPerson = new PersonDto { Salary = cashFlow };
        PersonManagerMock.Setup(p => p.Read(CurrentUserMock.Object)).Returns(testPerson);

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo($"Your Cashflow is *{cashFlow.AsCurrency()}*. How much should you get?"));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        });
    }

    [TestCase("1000", 0)]
    [TestCase("$2,000", 100)]
    [TestCase("$3,500", -2000)]
    public async Task GetMoney_PositiveCash(string message, int cashAmount)
    {
        // Arrange
        var testStage = GetTestStage();
        var amount = message.AsCurrency();
        PersonManagerMock.Setup(p => p.Read(CurrentUserMock.Object)).Returns(new PersonDto { Cash = cashAmount });

        // Act
        await testStage.HandleMessage(message);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        PersonManagerMock.Verify(p => p.Update(It.Is<PersonDto>(pr =>
            pr.Bankruptcy == false &&
            pr.Cash == cashAmount + amount)),
            Times.Once);

        CurrentUserMock.Verify(u => u.Notify($"Ok, you've got *{amount.AsCurrency()}*"), Times.Once);
    }

    [TestCase("-100", 99)]
    [TestCase("-$250", 200)]
    public async Task GetMoney_NegativeCash(string message, int cashAmount)
    {
        // Arrange
        var testStage = GetTestStage();
        var amount = message.AsCurrency();
        var testPerson = new PersonDto { Cash = cashAmount, Salary = -cashAmount };

        PersonManagerMock.Setup(p => p.Read(CurrentUserMock.Object)).Returns(testPerson);

        // Act
        await testStage.HandleMessage(message);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Bankruptcy>());

        PersonManagerMock.Verify(h => h.AddHistory(ActionType.Bankruptcy, 0, CurrentUserMock.Object), Times.Once);
        CurrentUserMock.Verify(u => u.Notify(It.IsAny<string>()), Times.Once);
        CurrentUserMock.Verify(u => u.Notify("Debt restructuring. Car loans, small loans and credit card halved."), Times.Once);
    }

    protected override IStage GetTestStage() => new GetMoney(TermsServiceMock.Object, PersonManagerMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}

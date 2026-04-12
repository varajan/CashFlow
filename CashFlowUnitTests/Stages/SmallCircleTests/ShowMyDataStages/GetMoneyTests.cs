using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.BankruptcyStages;
using CashFlow.Stages.SmallCircleStages.ShowMyDataStages;
using Moq;

namespace CashFlowUnitTests.Stages.SmallCircleTests.ShowMyDataStages;

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
        PersonServiceMock.Setup(p => p.Read(CurrentUser)).Returns(testPerson);

        // Act

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(testStage.Message, Is.EqualTo($"Your Cashflow is *{cashFlow.AsCurrency()}*. How many should you get?"));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        }
    }

    [TestCase("1000", 0)]
    [TestCase("$2,000", 100)]
    [TestCase("$3,500", -2000)]
    public async Task GetMoney_PositiveCash(string message, int cashAmount)
    {
        // Arrange
        var testStage = GetTestStage();
        var amount = message.AsCurrency();
        PersonServiceMock.Setup(p => p.Read(CurrentUser)).Returns(new PersonDto { Cash = cashAmount });

        // Act
        await testStage.HandleMessage(message);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        PersonServiceMock.Verify(p => p.Update(It.Is<PersonDto>(pr =>
            pr.Bankruptcy == false &&
            pr.Cash == cashAmount + amount)),
            Times.Once);

        NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, $"Ok, you've got *{amount.AsCurrency()}*"), Times.Once);
    }

    [TestCase("-100", 99)]
    [TestCase("-$250", 200)]
    public async Task GetMoney_NegativeCash(string message, int cashAmount)
    {
        // Arrange
        var testStage = GetTestStage();
        var amount = message.AsCurrency();
        var testPerson = new PersonDto { Cash = cashAmount, Salary = -cashAmount };

        PersonServiceMock.Setup(p => p.Read(CurrentUser)).Returns(testPerson);

        // Act
        await testStage.HandleMessage(message);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Bankruptcy>());

        PersonServiceMock.Verify(h => h.AddHistory(ActionType.Bankruptcy, 0, CurrentUser), Times.Once);
        NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, It.IsAny<string>()), Times.Once);
        NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, "Debt restructuring. Car loans, small loans and credit card halved."), Times.Once);
    }

    protected override IStage GetTestStage() => GetStage<GetMoney>();
}

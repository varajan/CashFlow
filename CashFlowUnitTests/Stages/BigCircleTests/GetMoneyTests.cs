using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.ShowMyDataStages;
using Moq;

namespace CashFlowUnitTests.Stages.BigCircleTests;

[TestFixture]
public class GetMoneyTests : StagesBaseTest
{
    private const int CashAmount = 1_000_000;
    private const int CashFlow = 500_000;

    private static PersonDto Person => new()
    {
        BigCircle = true,
        Cash = CashAmount,
        InitialCashFlow = 500_000,
        TargetCashFlow = 1_000_000,
    };

    [SetUp]
    public void Setup() => PersonServiceMock.Setup(x => x.Read(CurrentUser)).Returns(Person);

    [Test]
    public void GetMoney_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();
        var buttons = new[]
        {
            "$50,000",
            "$100,000",
            "$200,000",
            "$500,000",
            "Cancel"
        };

        // Act

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(testStage.Message, Is.EqualTo($"Your Cashflow is *{CashFlow.AsCurrency()}*. How many should you get?"));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        }
    }

    [TestCase("$100,000")]
    [TestCase("200000")]
    [TestCase("$3500")]
    public async Task GetMoney_ValidValue(string message)
    {
        // Arrange
        var testStage = GetTestStage();
        var amount = message.AsCurrency();

        // Act
        await testStage.HandleMessage(message);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        PersonServiceMock.Verify(p => p.Update(It.Is<PersonDto>(pr =>
            pr.Bankruptcy == false &&
            pr.Cash == CashAmount + amount)),
            Times.Once);

        NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, $"Ok, you've got *{amount.AsCurrency()}*"), Times.Once);
    }

    [TestCase("-$100,000")]
    [TestCase("-200000")]
    [TestCase("0")]
    public async Task GetMoney_InvalidValue(string message)
    {
        // Arrange
        var testStage = GetTestStage();
        var amount = message.AsCurrency();

        // Act
        await testStage.HandleMessage(message);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<GetMoney>());

        PersonServiceMock.Verify(p => p.Update(It.IsAny<PersonDto>()), Times.Never);
        NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, $"Invalid value. Try again."), Times.Once);
    }

    protected override IStage GetTestStage() => GetStage<GetMoney>();
}
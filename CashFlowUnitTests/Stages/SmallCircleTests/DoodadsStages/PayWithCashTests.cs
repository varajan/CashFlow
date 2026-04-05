using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.DoodadsStages;
using CashFlowUnitTests.Stages;
using Moq;

namespace CashFlowUnitTests.Stages.SmallCircleTests.DoodadsStages;

[TestFixture]
public class PayWithCashTests : StagesBaseTest
{
    private static readonly string[] Amounts = ["$100", "$500"];

    [SetUp]
    public void Setup()
    {
        AvailableAssetsMock.Setup(x => x.GetAsCurrency(AssetType.SmallGiveMoney)).Returns(Amounts);
    }

    [Test]
    public void Doodads_PayWithCash_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(testStage.Message, Is.EqualTo("How many?"));
            Assert.That(testStage.Buttons, Is.EqualTo(Amounts.Append("Cancel")));
        }
    }

    [TestCase("a")]
    [TestCase("-1")]
    [TestCase("0")]
    [TestCase(" ")]
    [TestCase("$")]
    public async Task Doodads_PayWithCash_SelectInvalidValue_StayOnStage(string amount)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(amount);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<PayWithCash>());
        NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, "Invalid value. Try again."), Times.Once);
    }

    [TestCase(0, 10, 1000)]
    [TestCase(100, 101, 1000)]
    [TestCase(100, 1500, 2000)]
    [TestCase(100, 100, 0)]
    public async Task Doodads_PayWithCash_SelectValidValue_Payed(int cash, int amount, int credit)
    {
        // Arrange
        var testStage = GetTestStage();

        var creditMessage = string.Format("You've taken *{0}* from bank.", credit.AsCurrency());

        PersonServiceMock.Setup(p => p.Read(CurrentUser)).Returns(new PersonDto { Id = CurrentUser.Id, Cash = cash });

        // Act
        await testStage.HandleMessage($"{amount}");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        PersonServiceMock.Verify(p => p.Update(It.Is<PersonDto>(person =>
            person.Id == CurrentUser.Id &&
            person.Cash == cash + credit - amount
        )), Times.AtLeastOnce);

        PersonServiceMock.Verify(x => x.AddHistory(ActionType.PayMoney, amount, CurrentUser), Times.Once);
        NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, creditMessage), Times.Exactly(cash < amount ? 1 : 0));
    }

    protected override IStage GetTestStage() => GetStage<PayWithCash>();
}

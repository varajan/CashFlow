using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.DoodadsStages;
using Moq;

namespace CashFlow.Tests.Stages.SmallCircleTests.DoodadsStages;

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
    public async Task Doodads_PayWithCash_SelectInvalidValue_StayOnStage(string amount)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(amount);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<PayWithCash>());
        CurrentUserMock.Verify(u => u.Notify("Invalid value. Try again."), Times.Once);
    }

    [TestCase(0, 10, 1000)]
    [TestCase(100, 101, 1000)]
    [TestCase(100, 1500, 2000)]
    [TestCase(100, 100, 0)]
    public async Task Doodads_PayWithCash_SelectValidValue_Payed(int cash, int amount, int credit)
    {
        // Arrange
        var testStage = GetTestStage();

        var creditMessage = string.Format("You've taken {0} from bank.", credit.AsCurrency());

        PersonManagerMock.Setup(p => p.Read(CurrentUserMock.Object.Id)).Returns(new PersonDto { Id = CurrentUserMock.Object.Id, Cash = cash });

        // Act
        await testStage.HandleMessage($"{amount}");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        PersonManagerMock.Verify(p => p.Update(It.Is<PersonDto>(person =>
            person.Id == CurrentUserMock.Object.Id &&
            person.Cash == cash - amount
        )), Times.Once);

        HistoryManagerMock.Verify(h => h.Add(ActionType.PayMoney, amount, CurrentUserMock.Object), Times.Once);
        CurrentUserMock.Verify(u => u.Notify(creditMessage), Times.Exactly(cash < amount ? 1 : 0));
        CurrentUserMock.Verify(u => u.GetCredit(credit), Times.Exactly(cash < amount ? 1 : 0));
    }

    protected override IStage GetTestStage() => new PayWithCash(
        TermsServiceMock.Object,
        AvailableAssetsMock.Object,
        PersonManagerMock.Object,
        HistoryManagerMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}
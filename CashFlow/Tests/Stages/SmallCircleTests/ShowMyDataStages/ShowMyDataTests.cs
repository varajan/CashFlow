using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Data.Users;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.ShowMyDataStages;
using Moq;

namespace CashFlow.Tests.Stages.SmallCircleTests.ShowMyDataStages;

[TestFixture]
public class ShowMyDataTests : StagesBaseTest
{
    [Test]
    public void ShowMyData_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();
        var description = "Test User person full description";
        var buttons = new List<string>
        {
            "Get Money",
            "Get Credit",
            "Charity - Pay 10%",
            "Reduce Liabilities",
            "Stop Game",
            "Main menu",
        };

        PersonManagerMock.Setup(p => p.GetDescription(CurrentUserMock.Object.Id)).Returns(description);

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo(description));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        });
    }

    [Test]
    public void NotImplemented()
    {
#if DEBUG
        Assert.Fail("Not Implemented.");
#endif
    }

    [TestCase("Cancel", typeof(ShowMyData))]
    [TestCase("Stop", typeof(ShowMyData))]
    public async Task ShowMyData_SelectInvalidOption(string message, Type nextStage)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(message);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf(nextStage));
    }

    [TestCase("Get Money", typeof(GetMoney))]
    [TestCase("Get Credit", typeof(GetCredit))]
    [TestCase("Reduce Liabilities", typeof(ReduceLiabilities))]
    [TestCase("Stop Game", typeof(StopGame))]
    [TestCase("Main menu", typeof(Start))]
    public async Task ShowMyData_SelectValidOption(string message, Type nextStage)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(message);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf(nextStage));
    }

    [TestCase(5, 5, 0, "You don't have $1, but only $0")]
    [TestCase(0, 10, 0, "You don't have $1, but only $0")]
    [TestCase(10, 0, 0, "You don't have $1, but only $0")]
    [TestCase(90, 10, 9, "You don't have $10, but only $9")]
    [TestCase(101, 0, 9, "You don't have $10, but only $9")]
    [TestCase(99, 0, 8, "You don't have $9, but only $8")]
    [TestCase(0, 10, 1, "You've payed $1, now you can use two dice in next 3 turns.")]
    [TestCase(90, 10, 10, "You've payed $10, now you can use two dice in next 3 turns.")]
    [TestCase(99, 1, 15, "You've payed $10, now you can use two dice in next 3 turns.")]
    public async Task Charity_Notification_Test(int cashFlow, int salary, int cash, string message)
    {
        // Arrange
        var testStage = GetTestStage();
        var payed = message.Contains("payed") ? 1 : 0;
        var amount = (cashFlow + salary) / 10;
        var testPerson = new PersonDto
        {
            Id = CurrentUserMock.Object.Id,
            Cash = cash,
            Salary = salary,
            Assets = [ new() { CashFlow = cashFlow } ],
        };

        PersonManagerMock.Setup(p => p.Read(CurrentUserMock.Object.Id)).Returns(testPerson);

        // Act
        await testStage.HandleMessage("Charity - Pay 10%");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());
        CurrentUserMock.Verify(u => u.Notify(message), Times.Once);

        HistoryManagerMock.Verify(h => h.Add(It.IsAny<ActionType>(), It.IsAny<long>(), It.IsAny<IUser>()), Times.Exactly(payed));
        HistoryManagerMock.Verify(h => h.Add(ActionType.Charity, amount, CurrentUserMock.Object), Times.Exactly(payed));

        PersonManagerMock.Verify(p => p.Update(It.IsAny<PersonDto>()), Times.Exactly(payed));
        PersonManagerMock.Verify(p => p.Update(It.Is<PersonDto>(p => p.Cash == cash - amount)), Times.Exactly(payed));
    }

    [Test, Ignore("Not applicable")]
    public override Task Stage_CanBeCanceled() => Task.CompletedTask;

    protected override IStage GetTestStage() => new ShowMyData(TermsServiceMock.Object, PersonManagerMock.Object, HistoryManagerMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}
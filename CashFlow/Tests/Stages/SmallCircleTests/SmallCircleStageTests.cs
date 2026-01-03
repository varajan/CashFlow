using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Stages;
using CashFlow.Stages.BigCircleStages;
using CashFlow.Stages.SmallCircleStages;
using CashFlow.Stages.SmallCircleStages.BankruptcyStages;
using CashFlow.Stages.SmallCircleStages.BigOpportunityStages;
using CashFlow.Stages.SmallCircleStages.DoodadsStages;
using CashFlow.Stages.SmallCircleStages.MarketStages;
using CashFlow.Stages.SmallCircleStages.SendMoneyStages;
using CashFlow.Stages.SmallCircleStages.ShowMyDataStages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages;
using Moq;

namespace CashFlow.Tests.Stages.SmallCircleTests;

[TestFixture]
public class SmallCircleStageTests : StagesBaseTest
{
    private PersonDto TestPerson => new() { Id = CurrentUserMock.Object.Id, Cash = 100 };

    [SetUp]
    public void Setup()
    {
        PersonManagerMock.Setup(p => p.Read(CurrentUserMock.Object)).Returns(TestPerson);
    }

    [Test, Ignore("Not applicable")]
    public override Task Stage_CanBeCanceled() => Task.CompletedTask;

    [Test]
    public void SmallCircle_Question_and_Buttons([Values] bool isHistoryEmpty, [Values] bool isReadyForBigCircle)
    {
        // Arrange
        var testStage = GetTestStage();
        var testPerson = TestPerson.Clone();
        var description = $"{testPerson.Id} has {testPerson.Cash}";

        List<string> buttons = isHistoryEmpty ? ["Show my Data", "Friends"] : ["Show my Data", "Friends", "History"];
        buttons.AddRange(["Small Opportunity", "Big Opportunity", "Doodads", "Market", "Downsize", "Baby", "Pay Check", "Give Money"]);
        if (isReadyForBigCircle) { buttons.Add("Go to Big Circle"); }

        HistoryManagerMock.Setup(x => x.IsEmpty(CurrentUserMock.Object.Id)).Returns(isHistoryEmpty);
        testPerson.ReadyForBigCircle = isReadyForBigCircle;
        PersonManagerMock.Setup(p => p.Read(CurrentUserMock.Object)).Returns(testPerson);
        PersonManagerMock.Setup(p => p.GetDescription(CurrentUserMock.Object)).Returns(description);

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo(description));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        });
    }

    [Test]
    public async Task SmallCircle_ShowMyData()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("Show my data");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<ShowMyData>());
    }

    [Test]
    public async Task SmallCircle_Friends([Values] bool noActiveUsers)
    {
        // Arrange
        if (noActiveUsers)
        {
            OtherUsers = OtherUsers.Where(x => !x.IsActive).ToList();
        }
        
        var testStage = GetTestStage();
        var expectedNextStage = noActiveUsers ? typeof(SmallCircle) : typeof(Friends);

        // Act
        await testStage.HandleMessage("friends");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf(expectedNextStage));

        if (noActiveUsers)
        {
            CurrentUserMock.Verify(u => u.Notify("There are no other players."), Times.Once);
        }
    }

    [Test]
    public async Task SmallCircle_History([Values] bool isHistoryEmpty)
    {
        // Arrange
        var testStage = GetTestStage();
        var expectedNextStage = isHistoryEmpty ? typeof(SmallCircle) : typeof(History);

        HistoryManagerMock.Setup(x => x.IsEmpty(CurrentUserMock.Object.Id)).Returns(isHistoryEmpty);

        // Act
        await testStage.HandleMessage("history");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf(expectedNextStage));
    }

    [Test]
    public async Task SmallCircle_SmallOpportunity()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("Small opportunity");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SmallOpportunity>());
    }

    [Test]
    public async Task SmallCircle_BigOpportunity()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("Big opportunity");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BigOpportunity>());
    }

    [Test]
    public async Task SmallCircle_Doodads()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("Doodads");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Doodads>());
    }

    [Test]
    public async Task SmallCircle_Market()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("Market");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Market>());
    }

    [Test]
    public async Task SmallCircle_Downsize_EnoughCash()
    {
        // Arrange
        var testStage = GetTestStage();

        var downsizeAmount = 100;
        var testPerson = TestPerson.Clone();
        var message = $"You were fired. You've payed total amount of your expenses: {downsizeAmount.AsCurrency()} and lose 2 turns.";

        testPerson.Expenses = new() { Taxes = downsizeAmount };
        PersonManagerMock.Setup(p => p.Read(CurrentUserMock.Object)).Returns(testPerson);

        // Act
        await testStage.HandleMessage("Downsize");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SmallCircle>());

        CurrentUserMock.Verify(u => u.Notify(message), Times.Once);
        CurrentUserMock.Verify(u => u.Notify(It.IsAny<string>()), Times.Once);
        CurrentUserMock.Verify(u => u.GetCredit_OBSOLETE(It.IsAny<int>()), Times.Never);
        PersonManagerMock.Verify(p => p.Update(It.Is<PersonDto>(pr => pr.Id == TestPerson.Id && pr.Cash == TestPerson.Cash - downsizeAmount)), Times.Once);
        HistoryManagerMock.Verify(h => h.Add(ActionType.Downsize, downsizeAmount, CurrentUserMock.Object), Times.Once);
    }

    [Test]
    public async Task SmallCircle_Downsize_NotEnoughCash()
    {
        // Arrange
        var testStage = GetTestStage();

        var downsizeAmount = 101;
        var testPerson = TestPerson.Clone();
        var message = $"You were fired. You've payed total amount of your expenses: {downsizeAmount.AsCurrency()} and lose 2 turns.";

        testPerson.Expenses = new() { Taxes = downsizeAmount };
        PersonManagerMock.Setup(p => p.Read(CurrentUserMock.Object)).Returns(testPerson);

        // Act
        await testStage.HandleMessage("Downsize");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SmallCircle>());

        CurrentUserMock.Verify(u => u.Notify(message), Times.Once);
        CurrentUserMock.Verify(u => u.Notify($"You've taken {1000.AsCurrency()} from bank."), Times.Once);
        CurrentUserMock.Verify(u => u.GetCredit_OBSOLETE(1000), Times.Once);
        PersonManagerMock.Verify(p => p.Update(It.Is<PersonDto>(pr => pr.Id == TestPerson.Id && pr.Cash == TestPerson.Cash - downsizeAmount)), Times.Once);
        HistoryManagerMock.Verify(h => h.Add(ActionType.Downsize, downsizeAmount, CurrentUserMock.Object), Times.Once);
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    public async Task SmallCircle_Baby(int children)
    {
        // Arrange
        var testStage = GetTestStage();

        var testPerson = TestPerson.Clone();
        testPerson.Profession = "Parent";
        testPerson.Expenses = new() { Children = children, PerChild = 50 };
        PersonManagerMock.Setup(p => p.Read(CurrentUserMock.Object)).Returns(testPerson);

        var expenses = testPerson.Expenses.PerChild * (children + 1);
        var message = $"{testPerson.Profession}, you have {expenses.AsCurrency()} children expenses and {children+1} children.";

        // Act
        await testStage.HandleMessage("baby");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SmallCircle>());

        CurrentUserMock.Verify(u => u.Notify(message), Times.Once);
        PersonManagerMock.Verify(p => p.Update(It.Is<PersonDto>(pr => pr.Expenses.Children == children + 1)), Times.Once);
        HistoryManagerMock.Verify(h => h.Add(ActionType.Child, children + 1, CurrentUserMock.Object), Times.Once);
    }

    [TestCase(3)]
    public async Task SmallCircle_Baby_LimitReached(int children)
    {
        // Arrange
        var testStage = GetTestStage();
        var message = "You're lucky parent of three children. You don't need one more.";

        var testPerson = TestPerson.Clone();
        testPerson.Profession = "Parent";
        testPerson.Expenses = new() { Children = children, PerChild = 50 };
        PersonManagerMock.Setup(p => p.Read(CurrentUserMock.Object)).Returns(testPerson);

        // Act
        await testStage.HandleMessage("baby");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SmallCircle>());

        CurrentUserMock.Verify(u => u.Notify(message), Times.Once);
        PersonManagerMock.Verify(p => p.Update(It.IsAny<PersonDto>()), Times.Never);
        HistoryManagerMock.Verify(h => h.Add(It.IsAny<ActionType>(), It.IsAny<int>(), CurrentUserMock.Object), Times.Never);
    }

    [TestCase(0, 0)]
    [TestCase(1, 1)]
    [TestCase(1, -1)]
    [TestCase(1, -2)]
    [TestCase(-1, 1)]
    public async Task SmallCircle_PayCheck_CanPay(int cashFlow, int cashAmount)
    {
        // Arrange
        var testStage = GetTestStage();
        var testPerson = TestPerson.Clone();
        testPerson.CashFlow = cashFlow;
        testPerson.Cash = cashAmount;
        PersonManagerMock.Setup(p => p.Read(CurrentUserMock.Object)).Returns(testPerson);

        // Act
        await testStage.HandleMessage("Pay check");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SmallCircle>());

        PersonManagerMock.Verify(p => p.Update(It.Is<PersonDto>(pr =>
            pr.Id == TestPerson.Id &&
            pr.Bankruptcy == false &&
            pr.Cash == cashAmount + cashFlow)),
            Times.Once);

        CurrentUserMock.Verify(u => u.Notify($"Ok, you've got *{cashFlow.AsCurrency()}*"), Times.Once);
    }

    [TestCase(-2, 1)]
    [TestCase(-1, 0)]
    public async Task SmallCircle_PayCheck_CannotPay(int cashFlow, int cashAmount)
    {
        // Arrange
        var testStage = GetTestStage();
        var testPerson = TestPerson.Clone();
        testPerson.CashFlow = cashFlow;
        testPerson.Cash = cashAmount;
        PersonManagerMock.Setup(p => p.Read(CurrentUserMock.Object)).Returns(testPerson);

        // Act
        await testStage.HandleMessage("Pay check");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Bankruptcy>());

        PersonManagerMock.Verify(h => h.AddHistory(ActionType.Bankruptcy, 0, CurrentUserMock.Object), Times.Once);
        PersonManagerMock.Verify(p => p.Update(It.IsAny<PersonDto>()), Times.Never);
        CurrentUserMock.Verify(u => u.Notify(It.IsAny<string>()), Times.Once);
        CurrentUserMock.Verify(u => u.Notify("Debt restructuring. Car loans, small loans and credit card halved."), Times.Once);
    }

    [Test]
    public async Task SmallCircle_GiveMoney()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("Give money");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SendMoney>());
    }

    [Test]
    public async Task SmallCircle_GoToBigCircle_WhenReady()
    {
        // Arrange
        var testStage = GetTestStage();
        var testPerson = TestPerson.Clone();
        var assets = new List<AssetDto>
        {
            new() { Id = 1, CashFlow = 200 },
            new() { Id = 2, CashFlow = 300 },
        };
        var initialCashFlow = assets.Sum(a => a.CashFlow);

        testPerson.ReadyForBigCircle = true;
        TestPerson.Assets = assets;

        PersonManagerMock.Setup(p => p.Read(CurrentUserMock.Object)).Returns(testPerson);

        // Act
        await testStage.HandleMessage("Go to Big Circle");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BigCircle>());
        PersonManagerMock.Verify(p => p.Update(It.Is<PersonDto>(pr =>
            pr.BigCircle == true &&
            pr.InitialCashFlow == initialCashFlow &&
            pr.Cash == TestPerson.Cash + initialCashFlow)), Times.Never);
    }

    [Test]
    public async Task SmallCircle_GoToBigCircle_WhenNotReady()
    {
        // Arrange
        var testStage = GetTestStage();
        var testPerson = TestPerson.Clone();
        testPerson.ReadyForBigCircle = false;
        PersonManagerMock.Setup(p => p.Read(CurrentUserMock.Object)).Returns(testPerson);

        // Act
        await testStage.HandleMessage("Go to Big Circle");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SmallCircle>());
        PersonManagerMock.Verify(p => p.Update(It.IsAny<PersonDto>()), Times.Never);
    }

    [Test]
    public async Task SmallCircle_CanNotBeCanceled()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("cancel");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SmallCircle>());
    }

    protected override IStage GetTestStage() => new SmallCircle(TermsServiceMock.Object, HistoryManagerMock.Object, PersonManagerMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}

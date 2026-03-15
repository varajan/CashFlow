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
    private PersonDto TestPerson => new() { Id = CurrentUser.Id, Cash = 100 };

    [SetUp]
    public void Setup() => PersonServiceMock.Setup(p => p.Read(CurrentUser)).Returns(TestPerson);

    [Test, Ignore("Not applicable")]
    public override Task Stage_CanBeCanceled() => Task.CompletedTask;

    [TestCase(true, false, 0, 100)]
    [TestCase(false, false, 100, 100)]
    [TestCase(true, true, 101, 100)]
    [TestCase(false, false, 0, 0)]
    public void SmallCircle_Question_and_Buttons(bool isHistoryEmpty, bool isReadyForBigCircle, int income, int expenses)
    {
        // Arrange
        var testStage = GetTestStage();
        var testPerson = TestPerson.Clone();
        var description = $"{testPerson.Id} has {testPerson.Cash}";
        var assets = new List<AssetDto> { new() { Id = 1, Qtty = 1, CashFlow = income } };

        testPerson.Assets = assets;
        testPerson.Children = 1;
        testPerson.PerChild = expenses;

        List<string> buttons = isHistoryEmpty ? ["Show my Data", "Friends"] : ["Show my Data", "Friends", "History"];
        buttons.AddRange(["Small Opportunity", "Big Opportunity", "Doodads", "Market", "Downsize", "Baby", "Paycheck", "Give Money"]);
        if (isReadyForBigCircle) { buttons.Add("Go to Big Circle"); }

        PersonServiceMock.Setup(x => x.IsHistoryEmpty(CurrentUser)).Returns(isHistoryEmpty);
        PersonServiceMock.Setup(p => p.Read(CurrentUser)).Returns(testPerson);
        PersonServiceMock.Setup(p => p.GetDescription(CurrentUser, true)).Returns(description);

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
            OtherUsers = OtherUsers.Where(x => !x.IsActive()).ToList();
            UserRepositoryMock.Setup(r => r.GetAll()).Returns(OtherUsers.Append(CurrentUser).ToList());
        }

        var testStage = GetTestStage();
        var expectedNextStage = noActiveUsers ? typeof(SmallCircle) : typeof(Friends);

        // Act
        await testStage.HandleMessage("friends");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf(expectedNextStage));

        if (noActiveUsers)
        {
            NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, "There are no other players."), Times.Once);
        }
    }

    [Test]
    public async Task SmallCircle_History([Values] bool isHistoryEmpty)
    {
        // Arrange
        var testStage = GetTestStage();
        var expectedNextStage = isHistoryEmpty ? typeof(SmallCircle) : typeof(History);

        PersonServiceMock.Setup(x => x.IsHistoryEmpty(CurrentUser)).Returns(isHistoryEmpty);

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

        testPerson.Liabilities = [new() { Cashflow = -downsizeAmount }];
        PersonServiceMock.Setup(p => p.Read(CurrentUser)).Returns(testPerson);

        // Act
        await testStage.HandleMessage("Downsize");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SmallCircle>());

        NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, message), Times.Once);
        NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, It.IsAny<string>()), Times.Once);
        NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, It.Is<string>(msg => Regex.IsMatch(msg, @"You've taken .* from bank\."))), Times.Never);

        PersonServiceMock.Verify(p => p.Update(It.Is<PersonDto>(pr => pr.Id == TestPerson.Id && pr.Cash == TestPerson.Cash - downsizeAmount)), Times.Once);
        PersonServiceMock.Verify(x => x.AddHistory(ActionType.Downsize, downsizeAmount, CurrentUser), Times.Once);
    }

    [Test]
    public async Task SmallCircle_Downsize_NotEnoughCash()
    {
        // Arrange
        var testStage = GetTestStage();

        var downsizeAmount = 101;
        var creditAmount = 1_000;
        var testPerson = TestPerson.Clone();
        var message = $"You were fired. You've payed total amount of your expenses: {downsizeAmount.AsCurrency()} and lose 2 turns.";

        testPerson.Liabilities = [ new() { Cashflow = -downsizeAmount} ];
        PersonServiceMock.Setup(p => p.Read(CurrentUser)).Returns(testPerson);

        // Act
        await testStage.HandleMessage("Downsize");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SmallCircle>());

        NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, message), Times.Once);
        NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, $"You've taken {creditAmount.AsCurrency()} from bank."), Times.Once);
        PersonServiceMock.Verify(p => p.Update(It.Is<PersonDto>(pr =>
            pr.Id == TestPerson.Id &&
            pr.Cash == TestPerson.Cash + creditAmount - downsizeAmount)),
            Times.Once);
        PersonServiceMock.Verify(x => x.AddHistory(ActionType.Downsize, downsizeAmount, CurrentUser), Times.Once);
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
        testPerson.Children = children;
        testPerson.PerChild = 50;
        PersonServiceMock.Setup(p => p.Read(CurrentUser)).Returns(testPerson);

        var expenses = testPerson.PerChild * (children + 1);
        var message = $"{testPerson.Profession}, you have {expenses.AsCurrency()} children expenses and {children+1} children.";

        // Act
        await testStage.HandleMessage("baby");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SmallCircle>());

        NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, message), Times.Once);
        PersonServiceMock.Verify(p => p.Update(It.Is<PersonDto>(pr => pr.Children == children + 1)), Times.Once);
        PersonServiceMock.Verify(x => x.AddHistory(ActionType.Child, children + 1, CurrentUser), Times.Once);
    }

    [TestCase(3)]
    public async Task SmallCircle_Baby_LimitReached(int children)
    {
        // Arrange
        var testStage = GetTestStage();
        var message = "You're lucky parent of three children. You don't need one more.";

        var testPerson = TestPerson.Clone();
        testPerson.Profession = "Parent";
        testPerson.Children = children;
        PersonServiceMock.Setup(p => p.Read(CurrentUser)).Returns(testPerson);

        // Act
        await testStage.HandleMessage("baby");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SmallCircle>());

        NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, message), Times.Once);
        PersonServiceMock.Verify(p => p.Update(It.IsAny<PersonDto>()), Times.Never);
        PersonServiceMock.Verify(x => x.AddHistory(It.IsAny<ActionType>(), It.IsAny<int>(), CurrentUser), Times.Never);
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
        testPerson.Salary = cashFlow;
        testPerson.Cash = cashAmount;
        PersonServiceMock.Setup(p => p.Read(CurrentUser)).Returns(testPerson);

        // Act
        await testStage.HandleMessage("Paycheck");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SmallCircle>());

        PersonServiceMock.Verify(p => p.Update(It.Is<PersonDto>(pr =>
            pr.Id == TestPerson.Id &&
            pr.Bankruptcy == false &&
            pr.Cash == cashAmount + cashFlow)),
            Times.Once);

        NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, $"Ok, you've got *{cashFlow.AsCurrency()}*"), Times.Once);
    }

    [TestCase(-2, 1)]
    [TestCase(-1, 0)]
    public async Task SmallCircle_PayCheck_CannotPay(int cashFlow, int cashAmount)
    {
        // Arrange
        var testStage = GetTestStage();
        var testPerson = TestPerson.Clone();
        testPerson.Salary = cashFlow;
        testPerson.Cash = cashAmount;
        PersonServiceMock.Setup(p => p.Read(CurrentUser)).Returns(testPerson);

        // Act
        await testStage.HandleMessage("Paycheck");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Bankruptcy>());

        PersonServiceMock.Verify(h => h.AddHistory(ActionType.Bankruptcy, 0, CurrentUser), Times.Once);
        NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, It.IsAny<string>()), Times.Once);
        NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, "Debt restructuring. Car loans, small loans and credit card halved."), Times.Once);
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
            new() { Id = 1, Qtty = 1, CashFlow = 200 },
            new() { Id = 2, Qtty = 1, CashFlow = 300 },
        };
        var initialCashFlow = assets.Sum(a => a.CashFlow) * 100;

        testPerson.Assets = assets;

        PersonServiceMock.Setup(p => p.Read(CurrentUser)).Returns(testPerson);

        // Act
        await testStage.HandleMessage("Go to Big Circle");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BigCircle>());

        PersonServiceMock.Verify(p => p.Update(It.Is<PersonDto>(pr =>
            pr.InitialCashFlow == initialCashFlow &&
            pr.BigCircle == true &&
            pr.Cash == TestPerson.Cash + initialCashFlow &&
            pr.TargetCashFlow == initialCashFlow + 50_000)), Times.Once);
        PersonServiceMock.Verify(x => x.AddHistory(ActionType.GoToBigCircle, initialCashFlow, CurrentUser), Times.Once);
    }

    [Test]
    public async Task SmallCircle_GoToBigCircle_WhenNotReady()
    {
        // Arrange
        var testStage = GetTestStage();
        var testPerson = TestPerson.Clone();
        PersonServiceMock.Setup(p => p.Read(CurrentUser)).Returns(testPerson);

        // Act
        await testStage.HandleMessage("Go to Big Circle");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SmallCircle>());
        PersonServiceMock.Verify(p => p.Update(It.IsAny<PersonDto>()), Times.Never);
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

    protected override IStage GetTestStage() => GetStage<SmallCircle>();
}

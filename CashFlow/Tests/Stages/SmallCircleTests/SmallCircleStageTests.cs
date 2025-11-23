using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages;
using CashFlow.Stages.SmallCircleStages.BigOpportunityStages;
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
        PersonManagerMock.Setup(p => p.Read(TestPerson.Id)).Returns(TestPerson);
        //HistoryManagerMock.Setup(x => x.Read(CurrentUserMock.Object.Id)).Returns(Records);
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
        buttons.AddRange(["Small Opportunity", "Big Opportunity", "Downsize", "Baby", "Pay Check", "Give Money"]);
        if (isReadyForBigCircle) { buttons.Add("Go to Big Circle"); }

        HistoryManagerMock.Setup(x => x.IsEmpty(CurrentUserMock.Object.Id)).Returns(isHistoryEmpty);
        testPerson.ReadyForBigCircle = isReadyForBigCircle;
        PersonManagerMock.Setup(p => p.Read(testPerson.Id)).Returns(testPerson);
        PersonManagerMock.Setup(p => p.GetDescription(testPerson.Id)).Returns(description);

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
    public async Task SmallCircle_Downsize()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("Downsize");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BigOpportunity>());
    }


    //buttons.AddRange "Downsize", "Baby", "Pay Check", "Give Money"]);
    //if (isReadyForBigCircle) { buttons.Add("Go to Big Circle"); }

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

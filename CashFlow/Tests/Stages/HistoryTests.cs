using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Stages;
using Moq;

namespace CashFlow.Tests.Stages;

[TestFixture]
public class HistoryTests : StagesBaseTest
{
    private List<HistoryDto> Records =
    [
        new HistoryDto { Id = 3, Action = ActionType.Credit,  Value = 1000, Description = "Credit: 1000" },
        new HistoryDto { Id = 2, Action = ActionType.GetMoney, Value = 200, Description = "GetMoney: 200" },
        new HistoryDto { Id = 1, Action = ActionType.PayMoney, Value = 100, Description = "PayMoney: 100" },
    ];

    [SetUp]
    public void Setup()
    {
        HistoryManagerMock.Setup(x => x.Read(CurrentUserMock.Object.Id)).Returns(Records);
    }

    [Test]
    public void History_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();
        var history = string.Join(Environment.NewLine, Records.Select(x => x.Description));
        var buttons = new List<string> { "Rollback last action", "Cancel" };

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo(history));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        });
    }

    [Test]
    public void History_NoRecords_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();
        var history = "No records found.";
        var buttons = new List<string> { "Cancel" };

        HistoryManagerMock.Setup(x => x.Read(CurrentUserMock.Object.Id)).Returns([]);

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo(history));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        });
    }

    [Test]
    public async Task History_CanBeCanceled()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("cancel");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        HistoryManagerMock.Verify(a => a.Rollback(It.IsAny<HistoryDto>()), Times.Never);
    }

    [Test]
    public async Task History_CanRollBackLatestAction()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("Rollback last action");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<History>());

        HistoryManagerMock.Verify(a => a.Rollback(It.IsAny<HistoryDto>()), Times.Once);
        HistoryManagerMock.Verify(a => a.Rollback(It.Is<HistoryDto>(x => x.Id == Records.First().Id)), Times.Once);
        PersonManagerMock.Verify(x => x.Update(It.IsAny<PersonDto>()), Times.Never, "No person data should be updated");
    }

    [Test]
    public async Task History_CanRollBackLastAction()
    {
        // Arrange
        var testStage = GetTestStage();
        HistoryManagerMock
            .SetupSequence(x => x.Read(CurrentUserMock.Object.Id))
            .Returns([Records.Last()])
            .Returns([])
            .Returns([]);

        // Act
        await testStage.HandleMessage("Rollback last action");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        HistoryManagerMock.Verify(a => a.Rollback(It.IsAny<HistoryDto>()), Times.Once);
        HistoryManagerMock.Verify(a => a.Rollback(It.Is<HistoryDto>(x => x.Id == Records.Last().Id)), Times.Once);
        CurrentUserMock.Verify(u => u.Notify("No records found."), Times.Once);
    }

    protected override IStage GetTestStage() => new History(TermsServiceMock.Object, HistoryManagerMock.Object, PersonManagerMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}

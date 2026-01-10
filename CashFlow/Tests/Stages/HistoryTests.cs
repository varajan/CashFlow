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
        new HistoryDto { Date = new DateTime(2025, 12, 30), Action = ActionType.Credit,  Value = 1000, Description = "• Credit: 1000" },
        new HistoryDto { Date = new DateTime(2025, 12, 30), Action = ActionType.GetMoney, Value = 200, Description = "• GetMoney: 200" },
        new HistoryDto { Date = new DateTime(2025, 12, 30), Action = ActionType.PayMoney, Value = 100, Description = "• PayMoney: 100" },
    ];

    [SetUp]
    public void Setup() => PersonManagerMock.Setup(x => x.ReadHistory(CurrentUserMock.Object)).Returns(Records);

    [Test]
    public void History_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();
        var history = string.Join(Environment.NewLine, Records.Select(x => x.Description));
        var buttons = new List<string> { "Rollback last action", "Main menu" };

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
        var buttons = new List<string> { "Main menu" };

        PersonManagerMock.Setup(x => x.ReadHistory(CurrentUserMock.Object)).Returns([]);
        PersonManagerMock.Setup(x => x.IsHistoryEmpty(CurrentUserMock.Object)).Returns(true);

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

        PersonManagerMock.Verify(x => x.RollbackHistory(It.IsAny<PersonDto>(), It.IsAny<HistoryDto>()), Times.Never);
    }

    [Test]
    public async Task History_CanGoToMainMenu()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("Main Menu");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        PersonManagerMock.Verify(x => x.RollbackHistory(It.IsAny<PersonDto>(), It.IsAny<HistoryDto>()), Times.Never);
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

        PersonManagerMock.Verify(x => x.RollbackHistory(It.IsAny<PersonDto>(), It.IsAny<HistoryDto>()), Times.Once);
        PersonManagerMock.Verify(x => x.RollbackHistory(It.IsAny<PersonDto>(), It.Is<HistoryDto>(x => x.Date == Records.First().Date)), Times.Once);
        PersonManagerMock.Verify(x => x.Update(It.IsAny<PersonDto>()), Times.Never, "No person data should be updated");
    }

    [Test]
    public async Task History_CanRollBackLastAction()
    {
        // Arrange
        var testStage = GetTestStage();
        PersonManagerMock
            .SetupSequence(x => x.ReadHistory(CurrentUserMock.Object))
            .Returns([Records.Last()])
            .Returns([])
            .Returns([]);

        // Act
        await testStage.HandleMessage("Rollback last action");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        PersonManagerMock.Verify(x => x.RollbackHistory(It.IsAny<PersonDto>(), It.IsAny<HistoryDto>()), Times.Once);
        PersonManagerMock.Verify(x => x.RollbackHistory(It.IsAny<PersonDto>(), It.Is<HistoryDto>(x => x.Date == Records.Last().Date)), Times.Once);
        CurrentUserMock.Verify(u => u.Notify("No records found."), Times.Once);
    }

    protected override IStage GetTestStage() => new History(TermsServiceMock.Object, PersonManagerMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}

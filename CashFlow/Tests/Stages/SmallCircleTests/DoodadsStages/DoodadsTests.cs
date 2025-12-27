using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.DoodadsStages;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Moq;

namespace CashFlow.Tests.Stages.SmallCircleTests.DoodadsStages;

[TestFixture]
public class DoodadsTests : StagesBaseTest
{
    [Test]
    public void Doodads_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo("What do you want?"));
            Assert.That(testStage.Buttons, Is.EqualTo(new List<string>
            {
                "Pay with Cash",
                "Pay with Credit Card",
                "Buy a boat",
                "Cancel"
            }));
        });
    }

    [TestCase("Pay with cash", typeof(PayWithCash))]
    [TestCase("Pay with Credit card", typeof(PayWithCreditCard))]
    public async Task Doodads_ValidCommand(string message, Type nextStage)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(message);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf(nextStage));
    }

    [Test]
    public async Task Doodads_InvalidCommand()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("Pay");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Doodads>());
    }

    [Test]
    public async Task Doodads_BuyBoat([Values(700, 1000, 1100)] int cash)
    {
        // Arrange
        var firstPayment = 1_000;
        var testStage = GetTestStage();

        var botMessage = string.Format(
            "You've bot a boat for {0} in credit, first payment is {1}, monthly payment is {2}",
            18_000.AsCurrency(),
            firstPayment.AsCurrency(),
            (-340).AsCurrency());
        var creditMessage = string.Format("You've taken {0} from bank.", firstPayment.AsCurrency());

        PersonManagerMock.Setup(p => p.Read(CurrentUserMock.Object.Id)).Returns(new PersonDto { Id = CurrentUserMock.Object.Id, Cash = cash });
        AssetManagerMock.Setup(a => a.ReadAll(It.IsAny<AssetType>(), CurrentUserMock.Object.Id)).Returns([]);

        // Act
        await testStage.HandleMessage("Buy a boat");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        AssetManagerMock.Verify(a => a.Create(It.Is<AssetDto>(asset =>
            asset.UserId == CurrentUserMock.Object.Id &&
            asset.CashFlow == -340 &&
            asset.Price == 18_000 &&
            asset.Mortgage == 17_000 &&
            asset.IsDraft == false
        )), Times.Once);

        PersonManagerMock.Verify(p => p.Update(It.Is<PersonDto>(person =>
            person.Id == CurrentUserMock.Object.Id &&
            person.Cash == cash - firstPayment
        )), Times.Once);

        HistoryManagerMock.Verify(h => h.Add(ActionType.BuyBoat, 18_000, CurrentUserMock.Object), Times.Once);
        CurrentUserMock.Verify(u => u.Notify(botMessage), Times.Once);
        CurrentUserMock.Verify(u => u.Notify(creditMessage), Times.Exactly(cash < firstPayment ? 1 : 0));
        CurrentUserMock.Verify(u => u.GetCredit_OBSOLETE(firstPayment), Times.Exactly(cash < firstPayment ? 1 : 0));
    }

    [Test]
    public async Task Doodads_BuyBoat_Again()
    {
        // Arrange
        var testStage = GetTestStage();

        AssetManagerMock.Setup(a => a.ReadAll(It.IsAny<AssetType>(), CurrentUserMock.Object.Id))
            .Returns([new AssetDto { Type = AssetType.Boat }]);

        // Act
        await testStage.HandleMessage("Buy a boat");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());
        CurrentUserMock.Verify(u => u.Notify("You already have a boat."), Times.Once);
        PersonManagerMock.Verify(p => p.Update(It.IsAny<PersonDto>()), Times.Never);
        HistoryManagerMock.Verify(h => h.Add(It.IsAny<ActionType>(), It.IsAny<long>(), CurrentUserMock.Object), Times.Never);
    }

    protected override IStage GetTestStage() => new Doodads(
        TermsServiceMock.Object,
        AssetManagerMock.Object,
        PersonManagerMock.Object,
        HistoryManagerMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}

using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.DoodadsStages;
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
        var endCash = cash < firstPayment ? cash : cash - firstPayment;

        var botMessage = string.Format(
            "You've bot a boat for {0} in credit, first payment is {1}, monthly payment is {2}",
            18_000.AsCurrency(),
            firstPayment.AsCurrency(),
            340.AsCurrency());
        var creditMessage = string.Format("You've taken {0} from bank.", firstPayment.AsCurrency());

        PersonServiceMock.Setup(p => p.Read(CurrentUser)).Returns(new PersonDto { Id = CurrentUser.Id, Cash = cash });
        PersonServiceMock.Setup(a => a.ReadAllAssets(It.IsAny<AssetType>(), CurrentUser)).Returns([]);

        // Act
        await testStage.HandleMessage("Buy a boat");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        PersonServiceMock.Verify(a => a.CreateAsset(CurrentUser, It.Is<AssetDto>(asset =>
            asset.UserId == CurrentUser.Id &&
            asset.CashFlow == -340 &&
            asset.Price == 18_000 &&
            asset.Mortgage == 17_000 &&
            asset.IsDraft == false
        )), Times.Once);

        PersonServiceMock.Verify(p => p.Update(It.Is<PersonDto>(person =>
            person.Id == CurrentUser.Id &&
            person.Cash == endCash
        )), Times.AtLeastOnce);

        PersonServiceMock.Verify(x => x.AddHistory(ActionType.BuyBoat, 18_000, CurrentUser), Times.Once);
        NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, botMessage), Times.Once);
        NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, creditMessage), Times.Exactly(cash < firstPayment ? 1 : 0));
    }

    [Test]
    public async Task Doodads_BuyBoat_Again()
    {
        // Arrange
        var testStage = GetTestStage();

        PersonServiceMock.Setup(a => a.ReadAllAssets(It.IsAny<AssetType>(), CurrentUser))
            .Returns([new AssetDto { Type = AssetType.Boat }]);

        // Act
        await testStage.HandleMessage("Buy a boat");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());
        NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, "You already have a boat."), Times.Once);
        PersonServiceMock.Verify(p => p.Update(It.IsAny<PersonDto>()), Times.Never);
        PersonServiceMock.Verify(x => x.AddHistory(It.IsAny<ActionType>(), It.IsAny<long>(), CurrentUser), Times.Never);
    }

    protected override IStage GetTestStage() => new Doodads(TermsServiceMock.Object, PersonServiceMock.Object, UserRepositoryMock.Object)
        .SetCurrentUser(CurrentUser);
}

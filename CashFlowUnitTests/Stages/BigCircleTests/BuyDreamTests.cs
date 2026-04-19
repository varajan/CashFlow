using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Stages;
using CashFlow.Stages.BigCircleStages;
using Moq;

namespace CashFlowUnitTests.Stages.BigCircleTests;

[TestFixture]
public class BuyDreamTests : StagesBaseTest
{
    private const int Cash = 100_000;
    private PersonDto TestPerson => new() { Id = CurrentUser.Id, Cash = Cash, BigCircle = true };

    [SetUp]
    public void Setup() => PersonServiceMock.Setup(p => p.Read(CurrentUser)).Returns(TestPerson);

    [Test]
    public void BuyDream_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(testStage.Message, Is.EqualTo("What is the price?"));
            Assert.That(testStage.Buttons, Is.EqualTo(BuyPrice.BigDream.OrderBy(x => x).AsCurrency().Append("Cancel")));
        }
    }

    [TestCase("0")]
    [TestCase("a")]
    [TestCase("12a")]
    public async Task BuyDream_InvalidAmount(string amount)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(amount);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<BuyDream>());
        NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, "Invalid price value. Try again."), Times.Once, "Message to user should be sent");
        PersonServiceMock.Verify(x => x.Update(It.IsAny<PersonDto>()), Times.Never, "No person data should be updated");
    }

    [TestCase(Cash)]
    [TestCase(Cash - 100)]
    public async Task BuyDream_AvailableAmount_IsCompleted(int price)
    {
        // Arrange
        var activeUsers = OtherUsers.Where(u => u.Name.Contains("Active")).Append(CurrentUser);

        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage($"{price}");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        PersonServiceMock.Verify(p => p.Update(
            It.Is<PersonDto>(x =>
            x.Id == TestPerson.Id &&
            x.Cash == TestPerson.Cash - price &&
            x.BoughtDream == true)
            ), Times.Once);

        PersonServiceMock.Verify(x => x.AddHistory(ActionType.BuyDream, price, CurrentUser), Times.Once);
    }

    [Test]
    public async Task BuyDream_UnavailableAmount_NotCompleted()
    {
        // Arrange
        var price = Cash + 1;
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage($"{price}");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        PersonServiceMock.Verify(x => x.Update(It.IsAny<PersonDto>()), Times.Never, "No person data should be updated");
        NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, "You don't have enough money."), Times.Once);
    }


    protected override IStage GetTestStage() => GetStage<BuyDream>();
}

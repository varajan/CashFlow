using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages;
using Moq;

namespace CashFlow.Tests.Stages;

[TestFixture]
public class ChooseProfessionTests : StagesBaseTest
{
    [Test, Ignore("Not applicable")]
    public override Task Stage_CanBeCanceled() => Task.CompletedTask;

    [Test]
    public async Task ChooseProfession_CanNotBeCanceled()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("cancel");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<ChooseProfession>());
    }

    [Test]
    public async Task ChooseProfession_CannotChooseInvalidProfession()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("Software Developer");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<ChooseProfession>());
    }

    [Test]
    public async Task ChooseProfession_CanSelectRandom()
    {
        // Arrange
        var testStage = GetTestStage();
        var personMock = new Mock<IPerson>();

        // Act
        await testStage.HandleMessage("random");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SmallCircle>());
        PersonManagerMock.Verify(p => p.Create(It.IsAny<string>(), CurrentUserMock.Object));
    }

    [Test]
    public async Task ChooseProfession_CanSelectProfession()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("TeaCher");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SmallCircle>());
        PersonManagerMock.Verify(p => p.Create("Teacher", CurrentUserMock.Object));
    }

    [Test]
    public void ChooseProfession_CanSelectAnyProfession()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo("Choose your *profession*"));
            Assert.That(testStage.Buttons, Is.EqualTo(new List<string>
            {
                "Business manager",
                "Car mechanic",
                "Doctor",
                "Engineer",
                "Janitor",
                "Lawyer",
                "Nurse",
                "Pilot",
                "Police officer",
                "Secretary",
                "Teacher",
                "Track driver",
                "Random"
            }));
        });
    }

    protected override IStage GetTestStage() => new ChooseProfession(TermsServiceMock.Object, PersonManagerMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}

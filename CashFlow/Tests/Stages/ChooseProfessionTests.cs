using CashFlow.Stages;
using Moq;

namespace CashFlow.Tests.Stages;

[TestFixture]
public class ChooseProfessionTests : StagesBaseTest
{
    [Test]
    public async Task ChooseProfession_CannotCancel()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("Cancel");

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

        // Act
        await testStage.HandleMessage("Random");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SmallCircle>());
        CurrentUserMock.Verify(u => u.Person.Create("Random"), Times.Once);
    }

    [Test]
    public async Task ChooseProfession_CanSelectProfession()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("Teacher");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<SmallCircle>());
        CurrentUserMock.Verify(u => u.Person.Create("Teacher"), Times.Once);
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

    private ChooseProfession GetTestStage() => new(OtherUsers, CurrentUserMock.Object, TermsServiceMock.Object, LoggerMock.Object, AssetsMock.Object);

}

using CashFlow;
using CashFlow.Data.DTOs;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages;
using CashFlow.Stages.SmallCircleStages.ShowMyDataStages;
using Moq;
using MoreLinq;
using System.Reflection;

namespace CashFlowUnitTests.Stages;

[TestFixtureSource(nameof(Stages))]
public class ReadyForBigCircleTests(Type stageType) : StagesBaseTest
{
    public static IEnumerable<Type?> Stages =>
            AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a =>
                {
                    try
                    {
                        return a.GetTypes();
                    }
                    catch (ReflectionTypeLoadException e)
                    {
                        return e.Types.Where(t => t != null)!;
                    }
                })
                .Where(t => typeof(IStage).IsAssignableFrom(t) && t is { IsClass: true, IsAbstract: false });


    private readonly List<Type> ShowMessageStages = [typeof(SmallCircle), typeof(ReduceLiabilities)];

    [Test]
    public async Task NotifyUsers_CanGoToBigCircle([Values] bool isReady)
    {
        // Arrange
        var testStage = ((IStage)ServicesProvider.Get(stageType)).SetCurrentUser(CurrentUser);
        var messageIsExpected = isReady && ShowMessageStages.Contains(stageType);

        var message = $"{CurrentUser.Name}' income is greater, then expenses. {CurrentUser.Name} is ready for Big Circle.";
        var activeUsers = OtherUsers.Where(u => u.Name.Contains("Active")).Append(CurrentUser);

        var testPerson = new PersonDto { Id = CurrentUser.Id, Cash = 100 };
        var assets = new List<AssetDto>
        {
            new() { Id = 1, Qtty = 1, CashFlow = 200 },
            new() { Id = 2, Qtty = 1, CashFlow = 300 },
        };
        testPerson.Assets = isReady ? assets : [];

        PersonServiceMock.Setup(p => p.Read(CurrentUser)).Returns(testPerson);

        // Act
        await testStage.BeforeStage();

        // Assert
        activeUsers.ForEach(u => NotifyServiceMock.Verify(n => n.Notify(u.Id, message), messageIsExpected ? Times.Once : Times.Never));
    }

    [Test, Ignore("Not applicable")]
    public override Task Stage_CanBeCanceled() => Task.CompletedTask;

    protected override IStage GetTestStage() => throw new NotImplementedException();
}

using CashFlow.Loggers;
using CashFlow.Data;
using CashFlow.Data.Users;
using CashFlow.Data.Users.UserData.PersonData;
using Moq;
using CashFlow.Stages;

namespace CashFlow.Tests.Stages;

public abstract class StagesBaseTest
{
    protected Mock<IUser> CurrentUserMock;
    protected List<IUser> OtherUsersMock;
    protected Mock<IAssetManager> AssetManagerMock;
    protected Mock<ITermsService> TermsServiceMock;
    protected Mock<ILogger> LoggerMock;
    protected Mock<IAvailableAssets> AssetsMock;

    protected abstract IStage GetTestStage();

    [SetUp]
    public void SetUp()
    {
        ServicesProvider.Init();

        CurrentUserMock = GetUserMock("Myself", true, Circle.Small);
        AssetManagerMock = new Mock<IAssetManager>();
        TermsServiceMock = new Mock<ITermsService>();
        LoggerMock = new Mock<ILogger>();
        AssetsMock = new Mock<IAvailableAssets>();
        OtherUsersMock =
            [
                GetUserMock("1st Active on Small Circle", true, Circle.Small).Object,
                GetUserMock("1st Active on Big Circle", true, Circle.Big).Object,
                GetUserMock("1st Inactive on Small Circle", false, Circle.Small).Object,
                GetUserMock("1st Inactive on Big Circle", false, Circle.Big).Object,
                GetUserMock("2nd Active on Small Circle", true, Circle.Small).Object,
                GetUserMock("2nd Active on Big Circle", true, Circle.Big).Object,
                GetUserMock("2nd Inactive on Small Circle", false, Circle.Small).Object,
                GetUserMock("2nd Inactive on Big Circle", false, Circle.Big).Object,
            ];

        TermsServiceMock
            .Setup(t => t.Get(It.IsAny<int>(), It.IsAny<IUser>(), It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns((int id, IUser user, string defaultValue, object[] args) => string.Format(defaultValue, args));
    }

    [Test]
    public virtual async Task Stage_CanBeCanceled()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("Cancel");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());
    }

    protected static Mock<IUser> GetUserMock(string name, bool isActive, Circle cirle)
    {
        var user = new Mock<IUser>();
        var person = new Mock<IPerson>();
        var assets = new Mock<IAssets>();

        person.SetupGet(p => p.Circle).Returns(cirle);
        person.SetupGet(p => p.Assets).Returns(assets.Object);

        user.SetupGet(u => u.Id).Returns(123);
        user.SetupGet(u => u.IsActive).Returns(isActive);
        user.SetupGet(u => u.Name).Returns(name);
        user.SetupGet(u => u.Person).Returns(person.Object);

        return user;
    }
}

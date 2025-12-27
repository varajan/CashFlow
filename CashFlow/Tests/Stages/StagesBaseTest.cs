using CashFlow.Data;
using CashFlow.Data.DTOs;
using CashFlow.Data.Users;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Interfaces;
using CashFlow.Stages;
using Moq;

namespace CashFlow.Tests.Stages;

public abstract class StagesBaseTest
{
    protected Mock<IUser> CurrentUserMock;
    protected List<IUser> OtherUsers;
    protected Mock<IAssetManager> AssetManagerMock;
    protected Mock<IAvailableAssets> AvailableAssetsMock;
    protected Mock<IPersonManager> PersonManagerMock;
    protected Mock<IHistoryManager> HistoryManagerMock;
    protected Mock<ITermsService> TermsServiceMock;
    protected Mock<ILogger> LoggerMock;
    protected Mock<IAvailableAssets> AssetsMock;

    protected abstract IStage GetTestStage();

    protected string NL => Environment.NewLine;

    [SetUp]
    public void SetUp()
    {
        ServicesProvider.Init();

        AssetManagerMock = new Mock<IAssetManager>();
        AvailableAssetsMock = new Mock<IAvailableAssets>();
        PersonManagerMock = new Mock<IPersonManager>();
        HistoryManagerMock = new Mock<IHistoryManager>();
        TermsServiceMock = new Mock<ITermsService>();
        LoggerMock = new Mock<ILogger>();
        AssetsMock = new Mock<IAvailableAssets>();

        CurrentUserMock = GetUserMock(10, "Test User", true, Circle.Small, PersonManagerMock);
        OtherUsers =
            [
                GetUserMock(1, "1st Active on Small Circle", true, Circle.Small, PersonManagerMock).Object,
                GetUserMock(2, "1st Active on Big Circle", true, Circle.Big, PersonManagerMock).Object,
                GetUserMock(3, "1st Inactive on Small Circle", false, Circle.Small, PersonManagerMock).Object,
                GetUserMock(4, "1st Inactive on Big Circle", false, Circle.Big, PersonManagerMock).Object,
                GetUserMock(5, "2nd Active on Small Circle", true, Circle.Small, PersonManagerMock).Object,
                GetUserMock(6, "2nd Active on Big Circle", true, Circle.Big, PersonManagerMock).Object,
                GetUserMock(7, "2nd Inactive on Small Circle", false, Circle.Small, PersonManagerMock).Object,
                GetUserMock(8, "2nd Inactive on Big Circle", false, Circle.Big, PersonManagerMock).Object,
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

    protected static Mock<IUser> GetUserMock(long id, string name, bool isActive, Circle cirle, Mock<IPersonManager> personManagerMock)
    {
        var user = new Mock<IUser>();
        var person = new Mock<IPerson>();
        var assets = new Mock<IAssets>();

        person.SetupGet(p => p.Circle).Returns(cirle);
        person.SetupGet(p => p.Assets).Returns(assets.Object);

        user.SetupGet(u => u.Id).Returns(id);
        user.SetupGet(u => u.IsActive).Returns(isActive);
        user.SetupGet(u => u.Name).Returns(name);
        user.SetupGet(u => u.Description).Returns($"{name} at {cirle} circle");
        user.SetupGet(u => u.Person_OBSOLETE).Returns(person.Object);

        var testPerson = new PersonDto { Id = id, Cash = 100, BigCircle = cirle == Circle.Big };
        personManagerMock.Setup(p => p.Read(user.Object.Id)).Returns(testPerson);

        return user;
    }
}

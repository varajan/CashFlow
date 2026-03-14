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
    protected Mock<IAvailableAssets> AvailableAssetsMock;
    protected Mock<IPersonManager> PersonManagerMock;
    protected Mock<ITermsService> TermsServiceMock;
    protected Mock<ILogger> LoggerMock;
    protected Mock<IAvailableAssets> AssetsMock;

    protected abstract IStage GetTestStage();

    protected string NL => Environment.NewLine;

    [SetUp]
    public void SetUp()
    {
        ServicesProvider.Init();

        AvailableAssetsMock = new Mock<IAvailableAssets>();
        PersonManagerMock = new Mock<IPersonManager>();
        TermsServiceMock = new Mock<ITermsService>();
        LoggerMock = new Mock<ILogger>();
        AssetsMock = new Mock<IAvailableAssets>();

        CurrentUserMock = GetUserMock(10, "Test User", true, Circle.Small);
        OtherUsers =
            [
                GetUserMock(1, "1st Active on Small Circle", true, Circle.Small).Object,
                GetUserMock(2, "1st Active on Big Circle", true, Circle.Big).Object,
                GetUserMock(3, "1st Inactive on Small Circle", false, Circle.Small).Object,
                GetUserMock(4, "1st Inactive on Big Circle", false, Circle.Big).Object,
                GetUserMock(5, "2nd Active on Small Circle", true, Circle.Small).Object,
                GetUserMock(6, "2nd Active on Big Circle", true, Circle.Big).Object,
                GetUserMock(7, "2nd Inactive on Small Circle", false, Circle.Small).Object,
                GetUserMock(8, "2nd Inactive on Big Circle", false, Circle.Big).Object,
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

    protected Mock<IUser> GetUserMock(long id, string name, bool isActive, Circle cirle)
    {
        var user = new Mock<IUser>();
        user.SetupGet(u => u.Id).Returns(id);
        user.SetupGet(u => u.IsActive).Returns(isActive);
        user.SetupGet(u => u.Name).Returns(name);
        user.SetupGet(u => u.Description).Returns($"{name} at {cirle} circle");

        var testPerson = new PersonDto { Id = id, Cash = 100, BigCircle = cirle == Circle.Big };
        PersonManagerMock.Setup(p => p.Read(user.Object)).Returns(testPerson);

        return user;
    }
}

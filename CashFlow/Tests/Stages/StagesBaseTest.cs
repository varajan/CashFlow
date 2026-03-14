using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Interfaces;
using CashFlow.Stages;
using Moq;

namespace CashFlow.Tests.Stages;

public abstract class StagesBaseTest
{
    protected Mock<ICashFlowUser> CurrentUserMock;
    protected List<ICashFlowUser> OtherUsers;
    protected Mock<IAvailableAssetsRepository> AvailableAssetsMock;
    protected Mock<IPersonService> PersonManagerMock;
    protected Mock<ITermsRepository> TermsServiceMock;
    protected Mock<ILogger> LoggerMock;
    protected Mock<IAvailableAssetsRepository> AssetsMock;

    protected abstract IStage GetTestStage();

    protected string NL => Environment.NewLine;

    [SetUp]
    public void SetUp()
    {
        ServicesProvider.Init();

        AvailableAssetsMock = new Mock<IAvailableAssetsRepository>();
        PersonManagerMock = new Mock<IPersonService>();
        TermsServiceMock = new Mock<ITermsRepository>();
        LoggerMock = new Mock<ILogger>();
        AssetsMock = new Mock<IAvailableAssetsRepository>();

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
            .Setup(t => t.Get(It.IsAny<int>(), It.IsAny<ICashFlowUser>(), It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns((int id, ICashFlowUser user, string defaultValue, object[] args) => string.Format(defaultValue, args));
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

    protected Mock<ICashFlowUser> GetUserMock(long id, string name, bool isActive, Circle cirle)
    {
        var user = new Mock<ICashFlowUser>();
        user.SetupGet(u => u.Id).Returns(id);
        user.SetupGet(u => u.IsActive).Returns(isActive);
        user.SetupGet(u => u.Name).Returns(name);
        user.SetupGet(u => u.Description).Returns($"{name} at {cirle} circle");

        var testPerson = new PersonDto { Id = id, Cash = 100, BigCircle = cirle == Circle.Big };
        PersonManagerMock.Setup(p => p.Read(user.Object)).Returns(testPerson);

        return user;
    }
}

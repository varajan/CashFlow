using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Interfaces;
using CashFlow.Stages;
using Moq;

namespace CashFlow.Tests.Stages;

public abstract class StagesBaseTest
{
    protected UserDto CurrentUser;
    protected List<UserDto> OtherUsers;
    protected Mock<IAvailableAssetsRepository> AvailableAssetsMock;
    protected Mock<IPersonService> PersonServiceMock;
    protected Mock<INotifyService> NotifyServiceMock;
    protected Mock<IUserRepository> UserRepositoryMock;
    protected Mock<IPersonRepository> PersonRepositoryMock;
    protected Mock<ITermsRepository> TermsServiceMock;
    protected Mock<ILogger> LoggerMock;
    protected Mock<IAvailableAssetsRepository> AssetsMock;

    protected abstract IStage GetTestStage();

    protected string NL => Environment.NewLine;

    [SetUp]
    public void SetUp()
    {
        AvailableAssetsMock = new Mock<IAvailableAssetsRepository>();
        UserRepositoryMock = new Mock<IUserRepository>();
        PersonServiceMock = new Mock<IPersonService>();
        PersonRepositoryMock = new Mock<IPersonRepository>();
        TermsServiceMock = new Mock<ITermsRepository>();
        NotifyServiceMock = new Mock<INotifyService>();
        LoggerMock = new Mock<ILogger>();
        AssetsMock = new Mock<IAvailableAssetsRepository>();

        ServicesProvider.Init(NotifyServiceMock.Object, PersonRepositoryMock.Object);

        CurrentUser = GetUser(10, "Test User", true, Circle.Small);
        OtherUsers =
            [
                GetUser(1, "1st Active on Small Circle", true, Circle.Small),
                GetUser(2, "1st Active on Big Circle", true, Circle.Big),
                GetUser(3, "1st Inactive on Small Circle", false, Circle.Small),
                GetUser(4, "1st Inactive on Big Circle", false, Circle.Big),
                GetUser(5, "2nd Active on Small Circle", true, Circle.Small),
                GetUser(6, "2nd Active on Big Circle", true, Circle.Big),
                GetUser(7, "2nd Inactive on Small Circle", false, Circle.Small),
                GetUser(8, "2nd Inactive on Big Circle", false, Circle.Big),
            ];

        UserRepositoryMock.Setup(r => r.GetAll()).Returns(OtherUsers.Append(CurrentUser).ToList());

        TermsServiceMock
            .Setup(t => t.Get(It.IsAny<int>(), It.IsAny<UserDto>(), It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns((int id, UserDto user, string defaultValue, object[] args) => string.Format(defaultValue, args));
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

    protected UserDto GetUser(long id, string name, bool isActive, Circle cirle)
    {
        var user = new UserDto
        {
            Id = id,
            Name = name
        };

        var testPerson = new PersonDto
        {
            Id = id,
            Cash = 100,
            BigCircle = cirle == Circle.Big,
            LastActive = isActive ? DateTime.Now : DateTime.Now.AddHours(-1)
        };

        UserRepositoryMock.Setup(r => r.Get(id)).Returns(user);
        PersonServiceMock.Setup(p => p.Read(user)).Returns(testPerson);
        PersonRepositoryMock.Setup(p => p.Get(id)).Returns(testPerson);

        return user;
    }
}

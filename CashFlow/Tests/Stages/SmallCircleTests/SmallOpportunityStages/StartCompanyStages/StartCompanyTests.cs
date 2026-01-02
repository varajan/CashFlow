using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages;
using Moq;

namespace CashFlow.Tests.Stages.SmallCircleTests.SmallOpportunityStages.StartCompanyStages;

[TestFixture]
public class StartCompanyTests : StagesBaseTest
{
    private static readonly string[] Names = ["Uno", "Dos"];

    [SetUp]
    public void Setup()
    {
        AvailableAssetsMock.Setup(x => x.GetAsText(AssetType.SmallBusinessType, It.IsAny<Language>())).Returns(Names);
        PersonManagerMock.Setup(a => a.ReadAllAssets(AssetType.SmallBusinessType, CurrentUserMock.Object.Id)).Returns([]);
    }

    [Test]
    public void StartCompany_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();
        var buttons = Names.OrderBy(x => x).Append("Cancel");

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo("Title:"));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        });
    }

    [Test]
    public async Task StartCompany_SelectInvalidValue_StayOnStage()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("Tres");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<StartCompany>());
    }

    [TestCaseSource(nameof(Names))]
    public async Task StartCompany_SelectValidValue_MoveForward(string companyName)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(companyName.ToLower());

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<StartCompanyPrice>());

        PersonManagerMock.Verify(a => a.CreateAsset(
            It.Is<AssetDto>(x =>
                x.Title == companyName &&
                x.Qtty == 1 &&
                x.UserId == CurrentUserMock.Object.Id &&
                x.Type == AssetType.SmallBusinessType &&
                x.IsDraft)
        ), Times.Once);
    }

    protected override IStage GetTestStage() => new StartCompany(TermsServiceMock.Object, AvailableAssetsMock.Object, PersonManagerMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}

using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.StartCompanyStages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.StartCompanyStages;
using Moq;

namespace CashFlow.Tests.Stages.SmallCircleTests.SmallOpportunityStages.StartCompanyStages;

[TestFixture]
public class StartCompanyTests : StagesBaseTest
{
    private static readonly string[] CompanyNames = ["Company Uno", "Company Dos"];

    [SetUp]
    public void Setup()
    {
        AvailableAssetsMock.Setup(x => x.GetAsText(AssetType.SmallBusinessType, It.IsAny<Language>())).Returns(CompanyNames);
        AssetManagerMock.Setup(a => a.ReadAll(AssetType.SmallBusinessType, CurrentUserMock.Object.Id)).Returns([]);
    }

    [Test]
    public void StartCompany_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();
        var buttons = CompanyNames.Append("Cancel");

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo("Title:"));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        });
    }

    [Test]
    public async Task StartCompany_SelectInvalidName_StayOnStage()
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage("Company Tres");

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<StartCompany>());
    }

    [TestCaseSource(nameof(CompanyNames))]
    public async Task StartCompany_SelectValidName_MoveForward(string companyName)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(companyName.ToLower());

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<StartCompanyPrice>());

        AssetManagerMock.Verify(a => a.Create(
            It.Is<AssetDto>(x =>
                x.Title == companyName &&
                x.UserId == CurrentUserMock.Object.Id &&
                x.Type == AssetType.SmallBusinessType &&
                x.IsDraft)
        ), Times.Once);
    }

    protected override IStage GetTestStage() => new StartCompany(TermsServiceMock.Object, AvailableAssetsMock.Object, AssetManagerMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}

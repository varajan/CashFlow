using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.MarketStages;
using Moq;
using MoreLinq;

namespace CashFlow.Tests.Stages.SmallCircleTests.MarketStages;

[TestFixture]
public class IncreaseCashflowTests : SellAssetBaseTest
{
    private PersonDto TestPerson => new() { Id = CurrentUserMock.Object.Id, Cash = 300 };
    private static readonly List<string> AvailableValues = ["$100", "$500", "$1,000",];

    [SetUp]
    public void IncreaseCashFlowSetup()
    {
        AvailableAssetsMock.Setup(a => a.GetAsCurrency(AssetType.IncreaseCashFlow)).Returns(AvailableValues);
        PersonManagerMock.Setup(p => p.Read(TestPerson.Id)).Returns(TestPerson);
    }

    [Test]
    public void IncreaseCashflow_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();
        var buttons = AvailableValues.Append("Cancel");

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo("What is the cash flow?"));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        });
    }

    [Test]
    public async Task IncreaseCashflow_SelectInvalidValue_StayOnStage([Values("-1", "0", "$0", "test")] string value)
    {
        // Arrange
        var testStage = GetTestStage();

        // Act
        await testStage.HandleMessage(value);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<IncreaseCashflow>());
        CurrentUserMock.Verify(u => u.Notify("Invalid value. Try again."), Times.Once);
        AssetManagerMock.Verify(a => a.Update(It.IsAny<AssetDto>()), Times.Never);
    }

    [TestCase("1")]
    [TestCaseSource(nameof(AvailableValues))]
    public async Task IncreaseCashflow_SelectValidValue_Completed(string value)
    {
        // Arrange
        var testStage = GetTestStage();
        var assetCashflow = Assets.ToDictionary(a => a.Id, a => a.CashFlow);

        // Act
        await testStage.HandleMessage(value);

        // Assert
        Assert.That(testStage.NextStage, Is.TypeOf<Start>());

        Assets.Where(a => a.Type == AssetType.SmallBusiness)
            .ForEach(asset =>
            {
                AssetManagerMock.Verify(a => a.Update(It.Is<AssetDto>(a =>
                    a.Id == asset.Id &&
                    a.CashFlow == assetCashflow[a.Id] + value.AsCurrency()))
                    , Times.Once);

                HistoryManagerMock.Verify(h => h.Add(ActionType.IncreaseCashFlow, asset.Id, CurrentUserMock.Object), Times.Once);
            });

        CurrentUserMock.Verify(u => u.Notify("Done."), Times.Once);
    }

    protected override IStage GetTestStage() => new IncreaseCashflow(
        TermsServiceMock.Object,
        AvailableAssetsMock.Object,
        AssetManagerMock.Object,
        HistoryManagerMock.Object,
        PersonManagerMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}
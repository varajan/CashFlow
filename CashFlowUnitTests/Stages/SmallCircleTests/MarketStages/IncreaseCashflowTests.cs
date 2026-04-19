using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.MarketStages;
using Moq;
using MoreLinq;

namespace CashFlowUnitTests.Stages.SmallCircleTests.MarketStages;

[TestFixture]
public class IncreaseCashflowTests : SellAssetBaseTest
{
    private PersonDto TestPerson => new() { Id = CurrentUser.Id, Cash = 300 };
    private static readonly List<string> AvailableValues = Prices.IncreaseCashFlow.OrderBy(x => x).AsCurrency().ToList();

    [SetUp]
    public void TestSetUp() => PersonServiceMock.Setup(p => p.Read(CurrentUser)).Returns(TestPerson);

    [Test]
    public void IncreaseCashflow_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();
        var buttons = AvailableValues.Append("Cancel");

        // Act

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(testStage.Message, Is.EqualTo("What is the cash flow?"));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        }
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
        NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, "Invalid value. Try again."), Times.Once);
        PersonServiceMock.Verify(a => a.UpdateAsset(CurrentUser, It.IsAny<AssetDto>()), Times.Never);
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

        Assets.Where(a => a.Type == AssetType.SmallBusinessType)
            .ForEach(asset =>
            {
                PersonServiceMock.Verify(a => a.UpdateAsset(CurrentUser, It.Is<AssetDto>(a =>
                    a.Id == asset.Id &&
                    a.CashFlow == assetCashflow[a.Id] + value.AsCurrency()))
                    , Times.Once);

                PersonServiceMock.Verify(x => x.AddHistory(ActionType.IncreaseCashFlow, value.AsCurrency(), CurrentUser, asset.Id), Times.Once);
            });

        NotifyServiceMock.Verify(n => n.Notify(CurrentUser.Id, "Done."), Times.Once);
    }

    protected override IStage GetTestStage() => GetStage<IncreaseCashflow>();
}

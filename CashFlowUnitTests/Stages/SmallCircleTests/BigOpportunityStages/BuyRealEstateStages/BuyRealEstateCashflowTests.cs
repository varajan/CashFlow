using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Stages;
using CashFlow.Stages.SmallCircleStages.BigOpportunityStages;
using Moq;

namespace CashFlowUnitTests.Stages.SmallCircleTests.BigOpportunityStages.BuyRealEstateStages;

[TestFixture]
public class BuyRealEstateCashflowTests : StagesBaseTest
{
    private static readonly string[] CashFlows = Cashflow.RealEstateBig.AsCurrency().ToArray();
    private AssetDto Asset => new() { Id = 123, UserId = CurrentUser.Id, Type = AssetType.RealEstate, Price = 10_000, Qtty = 1, IsDraft = true };
    private PersonDto TestPerson => new() { Id = CurrentUser.Id, Cash = 10_000 };

    [SetUp]
    public void Setup()
    {
        PersonServiceMock.Setup(p => p.Read(CurrentUser)).Returns(TestPerson);
        PersonServiceMock.Setup(a => a.ReadActiveAssets(AssetType.RealEstate, CurrentUser)).Returns([Asset]);
    }

    [Test]
    public void BuyRealEstateCashflow_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();
        var buttons = CashFlows.Append("Cancel");

        // Act

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(testStage.Message, Is.EqualTo("What is the cash flow?"));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        }
    }

    [TestCaseSource(nameof(CashFlows))]
    [TestCase("1000")]
    [TestCase("0")]
    public async Task BuyRealEstateCashflow_SelectValidCount_Done(string cashflow)
    {
        // Arrange
        var testStage = GetTestStage();
        var person = new PersonDto { Cash = 10_000 };
        var personCash = person.Cash - Asset.Price - Asset.Mortgage;

        PersonServiceMock.Setup(x => x.Read(CurrentUser)).Returns(person);

        // Act
        await testStage.HandleMessage(cashflow);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(testStage.NextStage, Is.TypeOf<Start>());

            PersonServiceMock.Verify(a => a.UpdateAsset(
                CurrentUser,
                It.Is<AssetDto>(x =>
                    x.CashFlow == cashflow.AsCurrency() &&
                    x.IsDraft == false)),
                Times.Once);

            PersonServiceMock.Verify(m => m.Update(It.Is<PersonDto>(x => x.Cash == personCash)), Times.Once);
            PersonServiceMock.Verify(x => x.AddHistory(
                ActionType.BuyRealEstate,
                cashflow.AsCurrency(),
                It.Is<UserDto>(x => x.Id == CurrentUser.Id),
                Asset.Id
            ), Times.Once);
        }
    }

    protected override IStage GetTestStage() => GetStage<BuyBigRealEstateCashFlow>();
}

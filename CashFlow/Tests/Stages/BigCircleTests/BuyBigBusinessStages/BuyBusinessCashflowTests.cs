using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Data.Users;
using CashFlow.Extensions;
using CashFlow.Stages;
using CashFlow.Stages.BigCircleStages;
using Moq;

namespace CashFlow.Tests.Stages.BigCircleTests.BuyBigBusinessStages;

[TestFixture]
public class BuyBigBusinessCashflowTests : StagesBaseTest
{
    private static readonly string[] CashFlows = ["$1,000", "$5,000"];
    private AssetDto Asset => new() { Id = 123, UserId = CurrentUserMock.Object.Id, Type = AssetType.BigBusinessType, Price = 10_000, Qtty = 1, IsDraft = true };
    private PersonDto TestPerson => new() { Id = CurrentUserMock.Object.Id, Cash = 10_000 };

    [SetUp]
    public void Setup()
    {
        PersonManagerMock.Setup(p => p.Read(CurrentUserMock.Object)).Returns(TestPerson);
        AvailableAssetsMock.Setup(x => x.GetAsCurrency(AssetType.BigBusinessCashFlow)).Returns(CashFlows);
        PersonManagerMock.Setup(a => a.ReadAllAssets(AssetType.BigBusinessType, CurrentUserMock.Object)).Returns([Asset]);
    }

    [Test]
    public void BuyBigBusinessCashflow_Question_and_Buttons()
    {
        // Arrange
        var testStage = GetTestStage();
        var buttons = CashFlows.Append("Cancel");

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.Message, Is.EqualTo("What is the cash flow?"));
            Assert.That(testStage.Buttons, Is.EqualTo(buttons));
        });
    }

    [TestCaseSource(nameof(CashFlows))]
    [TestCase("1000")]
    public async Task BuyBigBusinessCashflow_SelectValidValue_Done(string cashflow)
    {
        // Arrange
        var testStage = GetTestStage();
        var person = new PersonDto { Cash = 10_000 };
        var personCash = person.Cash - Asset.Price - Asset.Mortgage;

        PersonManagerMock.Setup(x => x.Read(CurrentUserMock.Object)).Returns(person);

        // Act
        await testStage.HandleMessage(cashflow);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testStage.NextStage, Is.TypeOf<Start>());

            PersonManagerMock.Verify(a => a.UpdateAsset(
                It.Is<AssetDto>(x =>
                    x.CashFlow == cashflow.AsCurrency() &&
                    x.IsDraft == false)),
                Times.Once);

            PersonManagerMock.Verify(m => m.Update(It.Is<PersonDto>(x => x.Cash == personCash)), Times.Once);
            HistoryManagerMock.Verify(m => m.Add(
                ActionType.BuyBusiness,
                Asset.Id,
                It.Is<IUser>(x => x.Id == CurrentUserMock.Object.Id)
            ), Times.Once);
        });
    }

    protected override IStage GetTestStage() => new BuyBigBusinessCashFlow(
            TermsServiceMock.Object,
            AvailableAssetsMock.Object,
            HistoryManagerMock.Object,
            PersonManagerMock.Object)
        .SetCurrentUser(CurrentUserMock.Object)
        .SetAllUsers(OtherUsers);
}

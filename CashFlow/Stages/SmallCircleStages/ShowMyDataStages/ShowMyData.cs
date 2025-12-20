using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.ShowMyDataStages;

public class ShowMyData(ITermsService termsService) : BaseStage(termsService)
{
    public override string Message => CurrentUser.Description;

    public override List<string> Buttons =>
    [
        Terms.Get(32, CurrentUser, "Get Money"),
        Terms.Get(34, CurrentUser, "Get Credit"),
        Terms.Get(90, CurrentUser, "Charity - Pay 10%"),
        Terms.Get(40, CurrentUser, "Reduce Liabilities"),
        Terms.Get(41, CurrentUser, "Stop Game"),
        //Terms.Get(zzz, CurrentUser, "Select language"),
        Terms.Get(102, CurrentUser, "Main menu"),
        Cancel
    ];

    public async override Task HandleMessage(string message)
    {
        throw new NotImplementedException();
    }
}
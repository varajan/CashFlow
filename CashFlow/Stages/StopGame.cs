using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Interfaces;

namespace CashFlow.Stages;

public class StopGame(ITermsService termsService, IPersonManager personManager, IAssetManager assetManager, IHistoryManager historyManager)
    : ConfirmStage(termsService, personManager, 3, "Are you sure want to stop current game?")
{
    protected IAssetManager AssetManager { get; } = assetManager;
    protected IHistoryManager HistoryManager { get; } = historyManager;

    protected override Task OnConfirmed()
    {
        HistoryManager.Clear(CurrentUser.Id);
        AssetManager.DeleteAll(CurrentUser.Id);
        PersonManager.Delete(CurrentUser.Id);
        NextStage = New<Start>();

        return Task.CompletedTask;
    }
}

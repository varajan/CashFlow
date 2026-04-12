using CashFlow.Data.Consts;
using CashFlow.Interfaces;

namespace CashFlow.Stages;

public class GameMenu(ITranslationService termsService, IUserService userService, IPersonService personManager, IUserRepository userRepository)
    : BaseStage(termsService, userService, personManager, userRepository)
{
    public override string Message => TranslationService.Get(Terms.WhatDoYouWant, CurrentUser);
    public override IEnumerable<string> Buttons => [StopGame, TranslationService.Get(Terms.Language, CurrentUser), Cancel];

    public override Task HandleMessage(string message)
    {
        if (IsCanceled(message) || MessageEquals(message, Terms.MainMenu))
        {
            NextStage = New<Start>();
            return Task.CompletedTask;
        }

        if (MessageEquals(message, Terms.StopGame))
        {
            NextStage = New<StopGame>();
            return Task.CompletedTask;
        }

        if (MessageEquals(message, Terms.Language))
        {
            NextStage = New<ChooseLanguage>();
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}

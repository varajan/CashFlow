using CashFlow;
using CashFlow.Data.DTOs;
using CashFlow.Interfaces;
using CashFlow.Stages;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CashFlowBot;

public class CashFlowBot
{
    private static ILogger Logger => ServicesProvider.Get<ILogger>();
    private static IUserRepository UserRepository => ServicesProvider.Get<IUserRepository>();

    private static void Main()
    {
        //    ServicePointManager.ServerCertificateValidationCallback += (_, _, _, _) => true;
        ServicesProvider.AddApplicationServices();

        var botToken = new BotIdProvider(Logger).InitializeToken();
        var botClient = new TelegramBotClient(botToken);
        using var cts = new CancellationTokenSource();
        var receiverOptions = new ReceiverOptions { AllowedUpdates = [] };
        var notifyService = new TelegramBotNotifyService(botClient);

        ServicesProvider.Add<INotifyService>(notifyService);

        botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            errorHandler: HandleErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
        );

        Console.WriteLine("Starting Bot.");
        Console.ReadKey();
        cts.Cancel();
    }

    private static Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken token)
    {
        Logger.Log(exception);
        Console.WriteLine($"Error: {exception.Message}");
        return Task.CompletedTask;
    }

    private static async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken token)
    {
        if (update.Message is not { } message)
            return;
        if (message.Text is not { } messageText)
            return;

        Logger.Log($"{message.Chat.Id} - {message.Chat.Username} - {message.Text}");
        Console.WriteLine($"Received a message from {message.From?.Username}: {messageText}");
        await bot.SendChatAction(message.Chat.Id, ChatAction.Typing, cancellationToken: token);

        try
        {
            var user = UserRepository.Get(message.Chat.Id);
            var stage = user is null || user.StageName is null
                ? GetStartSage(message)
                : BaseStage.GetCurrentStage(user);

            await stage.HandleMessage(message.Text.Trim());
            await stage.NextStage.BeforeStage();
            await stage.NextStage.SetButtons();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Logger.Log(e);
        }
    }

    private static IStage GetStartSage(Message message)
    {
        var userName = $"{message.From?.FirstName} {message.From?.LastName}".Trim();
        userName = string.IsNullOrEmpty(userName) ? message.From?.Username : userName;
        var user = new UserDto
        {
            Id = message.Chat.Id,
            Name = userName,
        };

        UserRepository.Save(user);

        return ServicesProvider.Get<ChooseLanguage>().SetCurrentUser(user);
    }
}
using CashFlow;
using CashFlow.Data.DTOs;
using CashFlow.Interfaces;
using CashFlow.Stages;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CashFlowBot;

public class CashFlowBot
{
    private static ILogger Logger => ServicesProvider.Get<ILogger>();
    private static IUserRepository UserRepository => ServicesProvider.Get<IUserRepository>();

    private static string BotToken
    {
        get
        {
            try
            {
                var pattern = @"^\d{10}:[a-zA-Z0-9-_]{35}$";
                var botIdFile = $"{AppDomain.CurrentDomain.BaseDirectory}/BotID.txt";
                var token = File.ReadAllLines(botIdFile).FirstOrDefault(x => !string.IsNullOrEmpty(x));

                if (string.IsNullOrEmpty(token)) throw new ArgumentException("id is null or empty");
                if (!Regex.IsMatch(token, pattern)) throw new InvalidDataException("Invalid bot ID");

                return token;
            }
            catch (Exception)
            {
                var howTo = $"{AppDomain.CurrentDomain.BaseDirectory}\\index.html";
                Process.Start(new ProcessStartInfo("cmd", $"/c start {howTo}") { CreateNoWindow = true });
                throw;
            }
        }
    }

    private static void Main()
    {
        //    ServicePointManager.ServerCertificateValidationCallback += (_, _, _, _) => true;

        var botClient = new TelegramBotClient(BotToken);
        using var cts = new CancellationTokenSource();
        var receiverOptions = new ReceiverOptions { AllowedUpdates = [] };
        var notifyService = new TelegramBotNotifyService(botClient);

        ServicesProvider.AddApplicationServices();
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
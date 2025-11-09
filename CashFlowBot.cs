using CashFlowBot.Data;
using CashFlowBot.Data.DataBase;
using CashFlowBot.Extensions;
using CashFlowBot.Loggers;
using CashFlowBot.Stages;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using CashFlowBot.Data.Users;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Collections.Generic;

namespace CashFlowBot;

public class CashFlowBot
{
    private static readonly FileLogger Logger = new();
    private static readonly SQLiteDataBase DataBase = new(Logger);
    private static readonly TermsService TermsService = new(DataBase);
    private static readonly Assets Assets = new(DataBase);

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
            var notifyService = new TelegramBotNotifyService(bot, message.Chat.Id);
            var user = new CashFlowUsersUser(DataBase, notifyService, message.Chat.Id);
            var users = GetOtherUsers(bot, user);
            var stage = user.Exists
                ? BaseStage.GetCurrentStage(users, user, TermsService, Logger, Assets)
                : GetStartSage(message, user, users);

            await stage.HandleMessage(message.Text.Trim());
            await stage.NextStage.SetButtons();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Logger.Log(e);
        }
    }

    private static IStage GetStartSage(Message message, IUser user, List<IUser> users)
    {
        var userName = $"{message.From?.FirstName} {message.From?.LastName}".Trim();
        userName = string.IsNullOrEmpty(userName) ? message.From?.Username : userName;

        user.Create();
        user.Name = userName;
        return new Start(users, user, TermsService, Logger, Assets);
    }

    private static List<IUser> GetOtherUsers(ITelegramBotClient bot, IUser currentUser) =>
        DataBase
            .GetColumn("SELECT ID FROM Users")
            .ToLong()
            .Where(x => x != currentUser.Id)
            .Select(x => (IUser)new CashFlowUsersUser(DataBase, new TelegramBotNotifyService(bot, x), x))
            .ToList();
}
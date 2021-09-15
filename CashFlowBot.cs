﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using CashFlowBot.Data;
using CashFlowBot.Extensions;
using CashFlowBot.Models;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace CashFlowBot
{
    public class CashFlowBot
    {
        private static readonly TelegramBotClient Bot = new("1991657067:AAGyDAK1xfqrfIEAFIKNsRjWOvy9owiKU40");

        public static void Main()
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            Bot.OnMessage       += BotOnMessageReceived;
            Bot.OnMessageEdited += BotOnMessageReceived;
            Bot.OnReceiveError  += BotOnReceiveError;

            Bot.StartReceiving(Array.Empty<UpdateType>());

            Console.WriteLine("Starting Bot.");
            Console.ReadLine();

            Bot.StopReceiving();
        }

        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            try
            {
                var message = messageEventArgs.Message;
                var user = new User(message.Chat.Id);

                Logger.Log($"{message.Chat.Id} - {message.Chat.Username} - {message.Text}");

                await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                if (message.Type != MessageType.Text) return;

                // Make user admin
                var makeAdmin = Regex.Match(message.Text, @"Make (\d+) admin");
                if (makeAdmin.Success)
                {
                    var userId = makeAdmin.Groups[1].Value.ToInt();
                    var usr = new User(userId);

                    if (!usr.Exists) return;
                    if (usr.IsAdmin) return;
                    if (!user.IsAdmin) return;

                    usr.IsAdmin = true;

                    Actions.Cancel(Bot, user.Id);
                    return;
                }
                // Make user admin

                switch (message.Text.ToLower().Trim())
                {
                    case "/start":
                        Actions.Start(Bot, user.Id, message.Chat.Username);
                        return;

                    case "get money":
                        Actions.Ask(Bot, user.Id, Stage.GetMoney,
                            $"Your Cash Flow is *{user.Person.CashFlow.AsCurrency()}*. How much should you get?", "$1 000", "$2 000", "$5 000", user.Person.CashFlow.AsCurrency());
                        return;

                    case "give money":
                        var giveMoney = AvailableAssets.Get(AssetType.GiveMoney).AsCurrency().ToArray();

                        Actions.Ask(Bot, user.Id, Stage.GiveMoney, "How much would you give?", giveMoney);
                        return;

                    case "add child":
                        Actions.Ask(Bot, user.Id, Stage.GetChild,
                            $"Hey {user.Person.Profession}, your have {user.Person.Expenses.Children} children. Get one more?", "Yes");
                        return;

                    case "stop game":
                    case "/clear":
                        Actions.Ask(Bot, user.Id, Stage.StopGame, "Are you sure want to stop current game?", "Yes");
                        return;

                    case "yes":
                        Actions.Confirm(Bot, user.Id);
                        return;

                    case "cancel":
                    case "/cancel":
                        Actions.Cancel(Bot, user.Id);
                        return;

                    case "reduce liabilities":
                        Actions.ReduceLiabilities(Bot, user.Id);
                        return;

                    case "mortgage":
                        Actions.ReduceLiabilities(Bot, user.Id, Stage.ReduceMortgage);
                        return;

                    case "school loan":
                        Actions.ReduceLiabilities(Bot, user.Id, Stage.ReduceSchoolLoan);
                        return;

                    case "car loan":
                        Actions.ReduceLiabilities(Bot, user.Id, Stage.ReduceCarLoan);
                        return;

                    case "credit card":
                        Actions.ReduceLiabilities(Bot, user.Id, Stage.ReduceCreditCard);
                        return;

                    case "bank loan":
                        Actions.ReduceLiabilities(Bot, user.Id, Stage.ReduceBankLoan);
                        return;

                    case "show my data":
                        Actions.ShowData(Bot, user.Id);
                        return;

                    case "get credit":
                        Actions.GetCredit(Bot, user.Id);
                        return;

                    case "buy stocks":
                        Actions.BuyStocks(Bot, user.Id);
                        return;

                    case "sell stocks":
                        Actions.SellStocks(Bot, user.Id);
                        return;

                    case "buy property":
                        Actions.BuyProperty(Bot, user.Id);
                        return;

                    case "sell property":
                        Actions.SellProperty(Bot, user.Id);
                        return;

                    case "admin":
                        if (user.IsAdmin)
                        {
                            Actions.AdminMenu(Bot, user);
                        }
                        else
                        {
                            Actions.NotifyAdmins(Bot, user);
                        }
                        return;

                    case "bring down":
                        if (user.IsAdmin)
                        {
                            Actions.Ask(Bot, user.Id, Stage.AdminBringDown, "Are you sure want to shut BOT down?", "Yes");
                        }
                        return;

                    case "logs":
                        if (!user.IsAdmin) return;

                        Actions.Ask(Bot, user.Id, Stage.AdminLogs, "Which log would you like to get?", "Full", "Top");
                        return;

                    case "full":
                        if (!user.IsAdmin) return;

                        if (user.Stage == Stage.AdminLogs)
                        {
                            await using var stream = File.Open(Logger.LogFile, FileMode.Open);
                            var fts = new InputOnlineFile(stream, "logs.txt");
                            await Bot.SendDocumentAsync(user.Id, fts);
                            Actions.AdminMenu(Bot, user);
                        }
                        return;

                    case "top":
                        if (!user.IsAdmin) return;

                        if (user.Stage == Stage.AdminLogs)
                        {
                            Bot.SendMessage(user.Id, Logger.Top, ParseMode.Default);
                        }
                        Actions.AdminMenu(Bot, user);
                        return;

                    case "users":
                        if (!user.IsAdmin) return;

                        Actions.ShowUsers(Bot, user);
                        return;
                }

                switch (user.Stage)
                {
                    case Stage.GetProfession:
                        Actions.SetProfession(Bot, user.Id, message.Text.Trim().ToLower());
                        return;

                    case Stage.GetCredit:
                        Actions.GetCredit(Bot, user.Id, message.Text.Trim());
                        return;

                    case Stage.GetMoney:
                        Actions.GetMoney(Bot, user.Id, message.Text.Trim());
                        return;

                    case Stage.GiveMoney:
                        Actions.GiveMoney(Bot, user.Id, message.Text.Trim());
                        return;

                    case Stage.ReduceMortgage:
                    case Stage.ReduceSchoolLoan:
                    case Stage.ReduceCarLoan:
                    case Stage.ReduceCreditCard:
                    case Stage.ReduceBankLoan:
                        Actions.PayCredit(Bot, user.Id, message.Text.Trim(), user.Stage);
                        return;

                    case Stage.BuyStocksTitle:
                    case Stage.BuyStocksPrice:
                    case Stage.BuyStocksQtty:
                        Actions.BuyStocks(Bot, user.Id, message.Text.Trim());
                        return;

                    case Stage.SellStocksTitle:
                    case Stage.SellStocksPrice:
                        Actions.SellStocks(Bot, user.Id, message.Text.Trim());
                        return;

                    case Stage.BuyPropertyTitle:
                    case Stage.BuyPropertyPrice:
                    case Stage.BuyPropertyFirstPayment:
                    case Stage.BuyPropertyCashFlow:
                        Actions.BuyProperty(Bot, user.Id, message.Text.Trim());
                        return;

                    case Stage.SellPropertyTitle:
                    case Stage.SellPropertyPrice:
                        Actions.SellProperty(Bot, user.Id, message.Text.Trim());
                        return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Logger.Log(e);
            }
        }

        private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Console.WriteLine($"Received error: {receiveErrorEventArgs.ApiRequestException.ErrorCode} — {receiveErrorEventArgs.ApiRequestException.Message}");
        }
    }
}

using System;
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
using Terms = CashFlowBot.DataBase.Terms;

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

                    Actions.Cancel(Bot, user);
                    return;
                }
                // Make user admin

                switch (message.Text.ToLower().Trim())
                {
                    case "en":
                    case "ua":
                        user.Language = message.Text.ToUpper().Trim().ParseEnum<Language>();

                        if (user.Person.Exists)
                        {
                            Actions.Cancel(Bot, user);
                        }
                        else
                        {
                            Actions.Start(Bot, user);
                        }
                        return;

                    case "/start":
                        Actions.Start(Bot, user, message.Chat.Username);
                        return;

                    // Term 32: Get Money
                    case "get money":
                    case "отримати гроші":
                        Actions.Ask(Bot, user, Stage.GetMoney,
                        Terms.Get(0, user, "Your Cash Flow is *{0}*. How much should you get?", user.Person.CashFlow.AsCurrency()),
                        "$1 000", "$2 000", "$5 000", user.Person.CashFlow.AsCurrency());
                        return;

                    // Term 33: Give Money
                    case "give money":
                    case "заплатити гроші":
                        var giveMoney = AvailableAssets.Get(AssetType.GiveMoney).AsCurrency().ToArray();

                        Actions.Ask(Bot, user, Stage.GiveMoney,
                        Terms.Get(1, user, "How much would you give?"), giveMoney);
                        return;

                    // Term 39: Add Child
                    case "add child":
                    case "завести дитину":
                        if (user.Person.Expenses.Children == 3)
                        {
                            Bot.SendMessage(user.Id,Terms.Get(57, user, "You're lucky parent of three children. You don't need one more."));
                            return;
                        }

                        Actions.Ask(Bot, user, Stage.GetChild,
                            Terms.Get(2, user, "Hey {0}, your have {1} children. Get one more?", user.Person.Profession, user.Person.Expenses.Children),
                        Terms.Get(4, user, "Yes"));
                        return;

                    // Term 41: Stop Game
                    case "stop game":
                    case "закінчити гру":
                    case "/clear":
                        Actions.Ask(Bot, user, Stage.StopGame,
                        Terms.Get(3, user, "Are you sure want to stop current game?"), Terms.Get(4, user, "Yes"));
                        return;

                    // Term 4 - YES
                    case "yes":
                    case "так":
                        Actions.Confirm(Bot, user);
                        return;

                    // Term 6 - CANCEL
                    case "cancel":
                    case "скасувати":
                    case "/cancel":
                        Actions.Cancel(Bot, user);
                        return;

                    // Term 40: Reduce Liabilities
                    case "reduce liabilities":
                    case "зменшити пасиви":
                        Actions.ReduceLiabilities(Bot, user);
                        return;

                    // Term 43: Mortgage
                    case "mortgage":
                    case "іпотека":
                        Actions.ReduceLiabilities(Bot, user, Stage.ReduceMortgage);
                        return;

                    // Term 44: School Loan
                    case "school loan":
                    case "кредит на освіту":
                        Actions.ReduceLiabilities(Bot, user, Stage.ReduceSchoolLoan);
                        return;

                    // Term 45: Car Loan
                    case "car loan":
                    case "кредит на авто":
                        Actions.ReduceLiabilities(Bot, user, Stage.ReduceCarLoan);
                        return;

                    // Term 46: Credit Card
                    case "credit card":
                    case "кредитна картка":
                        Actions.ReduceLiabilities(Bot, user, Stage.ReduceCreditCard);
                        return;

                    // Term 47: Bank Loan
                    case "bank loan":
                    case "банківська позика":
                        Actions.ReduceLiabilities(Bot, user, Stage.ReduceBankLoan);
                        return;

                    // Term 31: Show my Data
                    case "show my data":
                    case "мої дані":
                        Actions.ShowData(Bot, user);
                        return;

                    // Term 34: Get Credit
                    case "get credit":
                    case "взяти кредит":
                        Actions.GetCredit(Bot, user);
                        return;

                    // Term 35: Buy Stocks
                    case "buy stocks":
                    case "купити акції":
                        Actions.BuyStocks(Bot, user);
                        return;

                    // Term 36: Sell Stocks
                    case "sell stocks":
                    case "продати акції":
                        Actions.SellStocks(Bot, user);
                        return;

                    // Term 37: Buy Property
                    case "buy property":
                    case "купити нерухомість":
                        Actions.BuyProperty(Bot, user);
                        return;

                    // Term 38: Sell Property
                    case "sell property":
                    case "продати нерухомість":
                        Actions.SellProperty(Bot, user);
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
                            Actions.Ask(Bot, user, Stage.AdminBringDown, "Are you sure want to shut BOT down?", Terms.Get(4, user, "Yes"));
                        }
                        return;

                    case "logs":
                        if (!user.IsAdmin) return;

                        Actions.Ask(Bot, user, Stage.AdminLogs, "Which log would you like to get?", "Full", "Top");
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
                        Actions.SetProfession(Bot, user, message.Text.Trim().ToLower());
                        return;

                    case Stage.GetCredit:
                        Actions.GetCredit(Bot, user, message.Text.Trim());
                        return;

                    case Stage.GetMoney:
                        Actions.GetMoney(Bot, user, message.Text.Trim());
                        return;

                    case Stage.GiveMoney:
                        Actions.GiveMoney(Bot, user, message.Text.Trim());
                        return;

                    case Stage.ReduceMortgage:
                    case Stage.ReduceSchoolLoan:
                    case Stage.ReduceCarLoan:
                    case Stage.ReduceCreditCard:
                    case Stage.ReduceBankLoan:
                        Actions.PayCredit(Bot, user, message.Text.Trim(), user.Stage);
                        return;

                    case Stage.BuyStocksTitle:
                    case Stage.BuyStocksPrice:
                    case Stage.BuyStocksQtty:
                        Actions.BuyStocks(Bot, user, message.Text.Trim());
                        return;

                    case Stage.SellStocksTitle:
                    case Stage.SellStocksPrice:
                        Actions.SellStocks(Bot, user, message.Text.Trim());
                        return;

                    case Stage.BuyPropertyTitle:
                    case Stage.BuyPropertyPrice:
                    case Stage.BuyPropertyFirstPayment:
                    case Stage.BuyPropertyCashFlow:
                        Actions.BuyProperty(Bot, user, message.Text.Trim());
                        return;

                    case Stage.SellPropertyTitle:
                    case Stage.SellPropertyPrice:
                        Actions.SellProperty(Bot, user, message.Text.Trim());
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

using System;
using System.Collections.Generic;
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
#if DEBUG
        private static readonly TelegramBotClient Bot = new("1357607824:AAFbYG2hjms9b3mtlphXMiwRHEjIA13nJF8");
#else
        private static readonly TelegramBotClient Bot = new("1991657067:AAGyDAK1xfqrfIEAFIKNsRjWOvy9owiKU40");
#endif

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
                    case "de":
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

                    // Term 79: Pay Check
                    case "pay check":
                    case "зарплатня":
                    case "gehalt":
                        Actions.GetMoney(Bot, user, user.Person.CashFlow.AsCurrency());
                        return;

                    // Term 39: Baby
                    case "baby":
                    case "дитина":
                    case "kind":
                        if (user.Person.Expenses.Children == 3)
                        {
                            Bot.SendMessage(user.Id, Terms.Get(57, user, "You're lucky parent of three children. You don't need one more."));
                            return;
                        }

                        user.Person.Expenses.Children++;
                        user.History.Add(ActionType.Child, user.Person.Expenses.Children);

                        Actions.SmallCircleButtons(Bot, user,
                        Terms.Get(user.Person.Expenses.Children == 1 ? 20 : 25,
                        user, "{0}, you have {1} children expenses and {2} children.",
                        user.Person.Profession, user.Person.Expenses.ChildrenExpenses.AsCurrency(), user.Person.Expenses.Children.ToString()));
                        return;

                    // Term 80: Downsize
                    case "downsize":
                    case "звільнення":
                    case "entlassung":
                        Actions.Downsize(Bot, user);
                        return;

                    #region My Data

                    // Term 31: Show my Data
                    case "show my data":
                    case "мої дані":
                    case "meine info":
                        Actions.MyData(Bot, user);
                        return;

                    // todo
                    // Term 2: History
                    case "history":
                        Actions.History(Bot, user);
                        return;

                    // Term 34: Get Credit
                    case "get credit":
                    case "взяти кредит":
                    case "kredit bekomen":
                        switch (user.Stage)
                        {
                            case Stage.BuyRealEstateFirstPayment:
                                user.Stage = Stage.BuyRealEstateCredit;
                                Actions.BuyRealEstate(Bot, user, string.Empty);
                                return;

                            case Stage.BuyBusinessFirstPayment:
                                user.Stage = Stage.BuyBusinessCredit;
                                Actions.BuyBusiness(Bot, user, string.Empty);
                                return;

                            default:
                                Actions.GetCredit(Bot, user);
                                return;
                        }

                    // Term 32: Get Money
                    case "get money":
                    case "отримати гроші":
                    case "geld bekomen":
                        var buttons = user.Person.BigCircle
                            ? new[] { "$50 000", "$100 000", "$200 000", user.Person.CurrentCashFlow.AsCurrency() }
                            : new[] { "$1 000", "$2 000", "$5 000", user.Person.CashFlow.AsCurrency() };

                        Actions.Ask(Bot, user, Stage.GetMoney,
                        Terms.Get(0, user, "Your Cash Flow is *{0}*. How much should you get?",
                        user.Person.BigCircle ? user.Person.CurrentCashFlow.AsCurrency() : user.Person.CashFlow.AsCurrency()), buttons);
                        return;

                    // Term 33: Give Money
                    case "give money":
                    case "заплатити гроші":
                    case "geld geben":
                    // Term 95: Pay with Cash
                    case "pay with cash":
                    case "оплатити готівкою":
                    case "mit bargeld zahlen":
                        var giveMoney = AvailableAssets.Get(user.Person.BigCircle ? AssetType.BigGiveMoney : AssetType.SmallGiveMoney).AsCurrency().ToArray();

                        Actions.Ask(Bot, user, Stage.GiveMoney,
                        Terms.Get(21, user, "How much?"), giveMoney);
                        return;

                    #region Reduce Liabilities
                    // Term 40: Reduce Liabilities
                    case "reduce liabilities":
                    case "зменшити борги":
                    case "verbindlichkeiten reduzieren":
                        Actions.ReduceLiabilities(Bot, user);
                        return;

                    // Term 43: Mortgage
                    case "mortgage":
                    case "іпотека":
                    case "hypothek":
                        Actions.ReduceLiabilities(Bot, user, Stage.ReduceMortgage);
                        return;

                    // Term 44: School Loan
                    case "school loan":
                    case "кредит на освіту":
                    case "schuldarlehen":
                        Actions.ReduceLiabilities(Bot, user, Stage.ReduceSchoolLoan);
                        return;

                    // Term 45: Car Loan
                    case "car loan":
                    case "кредит на авто":
                    case "autokredit":
                        Actions.ReduceLiabilities(Bot, user, Stage.ReduceCarLoan);
                        return;

                    // Term 46: Credit Card
                    case "credit card":
                    case "кредитна картка":
                    case "kreditkarte":
                        Actions.ReduceLiabilities(Bot, user, Stage.ReduceCreditCard);
                        return;

                    // Term 92: Small Credit
                    case "small credit":
                    case "мікрокредит":
                    case "klein kredit":
                        Actions.ReduceLiabilities(Bot, user, Stage.ReduceSmallCredit);
                        return;

                    // Term 47: Bank Loan
                    case "bank loan":
                    case "банківська позика":
                    case "bankkredit":
                        Actions.ReduceLiabilities(Bot, user, Stage.ReduceBankLoan);
                        return;
                    #endregion

                    // Term 90: Charity - Pay 10%
                    case "charity - pay 10%":
                    case "благодійність - віддати 10%":
                    case "nächstenliebe - 10% zahlen":
                        Actions.Charity(Bot, user);
                        return;

                    // Term 41: Stop Game
                    case "stop game":
                    case "закінчити гру":
                    case "spiel beenden":
                    case "/clear":
                        Actions.Ask(Bot, user, Stage.StopGame,
                        Terms.Get(3, user, "Are you sure want to stop current game?"), Terms.Get(4, user, "Yes"));
                        return;

                    #endregion

                    #region Small opportunities

                    // Term 81: Small Opportunity
                    case "small opportunity":
                    case "мала можливість":
                    case "kleine chance":
                        Actions.SmallOpportunity(Bot, user);
                        return;

                    // Term 35: Buy Stocks
                    case "buy stocks":
                    case "купити акції":
                    case "aktien kaufen":
                        Actions.BuyStocks(Bot, user);
                        return;

                    // Term 36: Sell Stocks
                    case "sell stocks":
                    case "продати акції":
                    case "aktien verkaufen":
                        Actions.SellStocks(Bot, user);
                        return;

                    // Term 37: Buy Real Estate
                    case "buy real estate":
                    case "купити нерухомість":
                    case "immobilien kaufen":
                        Actions.BuyRealEstate(Bot, user);
                        return;

                    // Term 82: Stocks 2 to 1
                    case "stocks 2 to 1":
                    case "акції 2 до 1":
                    case "aktien 2 -> 1":
                        user.Stage = Stage.Stocks2to1;
                        Actions.MultiplyStocks(Bot, user);
                        return;

                    // Term 83: Stocks 1 to 2
                    case "stocks 1 to 2":
                    case "акції 1 до 2":
                    case "aktien 1 -> 2":
                        user.Stage = Stage.Stocks1to2;
                        Actions.MultiplyStocks(Bot, user);
                        return;

                    #endregion

                    #region Big opportunities

                    // Term 84: Big Opportunity
                    case "big opportunity":
                    case "велика можливість":
                    case "große chance":
                        Actions.BigOpportunity(Bot, user);
                        return;

                    // Term 74: Buy Business
                    case "buy business":
                    case "купити підприємство":
                    case "geschäft kaufen":
                        Actions.BuyBusiness(Bot, user);
                        return;

                    // Term 94: Buy Land
                    case "buy land":
                    case "купити землю":
                    case "land kaufen":
                        Actions.BuyLand(Bot, user);
                        return;


                    #endregion

                    #region Doodads
                    // Term 86: Doodads
                    case "doodads":
                    case "дрібнички":
                        Actions.Doodads(Bot, user);
                        return;

                    // Term 96: Pay with Credit Card
                    case "Pay with Credit Card":
                    case "оплатити кредиткою":
                    case "mit kreditkarte zahlen":
                        Actions.PayWithCreditCard(Bot, user);
                        return;

                    #endregion

                    #region Market

                    // Term 85: Market
                    case "market":
                    case "ринок":
                    case "markt":
                        Actions.Market(Bot, user);
                        return;

                    // Term 38: Sell Real Estate
                    case "sell real estate":
                    case "продати нерухомість":
                    case "immobilien verkaufen":
                        Actions.SellRealEstate(Bot, user);
                        return;

                    // Term 75: Sell Business
                    case "sell business":
                    case "продати підприємство":
                    case "geschäft verkaufen":
                        Actions.SellBusiness(Bot, user);
                        return;

                    // Term 98 : Sell Land
                    case "sell land":
                    case "продати землю":
                    case "land verkaufen":
                        Actions.SellLand(Bot, user);
                        return;

                    #endregion

                    // Term 69: Divorce
                    case "divorce":
                    case "розлучення":
                    case "die ehescheidung":
                        Actions.Divorce(Bot, user);
                        return;

                    // Term 1: Go to Big Circle
                    case "go to big circle":
                    case "перейти до великого кола":
                    case "eintreten den großen kreis":
                        Actions.GoToBigCircle(Bot, user);
                        return;

                    // Term 70: Tax Audit
                    // Term 71: Lawsuit
                    case "tax audit":
                    case "lawsuit":
                    case "die steuerprüfung":
                    case "die klage":
                    case "податкова перевірка":
                    case "судовий процес":
                        Actions.TaxAudit(Bot, user);
                        return;

                    // Term 4 - YES
                    case "yes":
                    case "так":
                    case "ja":
                        Actions.Confirm(Bot, user);
                        return;

                    // todo ROLLBACK
                    // Term 109: Rollback last action
                    case "rollback last action":
                        Actions.Ask(Bot, user, Stage.Rollback,
                        Terms.Get(110, user, "Are you sure want to rollback last action?"), Terms.Get(4, user, "Yes"));
                        return;

                    // Term 6: Cancel
                    case "main menu": // todo
                    case "cancel":
                    case "скасувати":
                    case "absagen":
                    case "/cancel":
                        Actions.Cancel(Bot, user);
                        return;

                    #region Admin
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
                        if (!user.IsAdmin) break;

                        Actions.Ask(Bot, user, Stage.AdminBringDown, "Are you sure want to shut BOT down?", "Yes", "Back");
                        return;

                    case "logs":
                        if (!user.IsAdmin) break;

                        Actions.Ask(Bot, user, Stage.AdminLogs, "Which log would you like to get?", "Full", "Top", "Back");
                        return;

                    case "full":
                        if (!user.IsAdmin) break;

                        if (user.Stage == Stage.AdminLogs)
                        {
                            await using var stream = File.Open(Logger.LogFile, FileMode.Open);
                            var fts = new InputOnlineFile(stream, "logs.txt");
                            await Bot.SendDocumentAsync(user.Id, fts);
                            Actions.AdminMenu(Bot, user);
                        }
                        return;

                    case "top":
                        if (!user.IsAdmin) break;

                        if (user.Stage == Stage.AdminLogs)
                        {
                            Bot.SendMessage(user.Id, Logger.Top, ParseMode.Default);
                        }
                        Actions.AdminMenu(Bot, user);
                        return;

                    case "users":
                        if (!user.IsAdmin) break;

                        Actions.ShowUsers(Bot, user);
                        return;

                    case "back":
                        if (!user.IsAdmin) break;

                        Actions.AdminMenu(Bot, user);
                        return;

                    case "available assets":
                        if (!user.IsAdmin) break;

                        var assets = new List<string>();

                        foreach (var type in Enum.GetValues(typeof(AssetType)))
                        {
                            var assetType = type.ToString().ParseEnum<AssetType>();
                            var count = AvailableAssets.Get(assetType).Count;

                            if (count > 0) assets.Add($"{type} - {count}");
                        }

                        Actions.Ask(Bot, user, Stage.AdminAvailableAssets, "What types to show?",
                        assets.Append("All").Append("Back").ToArray());
                        return;
                    #endregion
                }

                switch (user.Stage)
                {
                    case Stage.Stocks1to2:
                    case Stage.Stocks2to1:
                        Actions.MultiplyStocks(Bot, user, message.Text.Trim());
                        return;

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

                    case Stage.MicroCreditAmount:
                    case Stage.MicroCreditMonthly:
                        Actions.PayWithCreditCard(Bot, user, message.Text.Trim());
                        return;

                    case Stage.ReduceMortgage:
                    case Stage.ReduceSchoolLoan:
                    case Stage.ReduceCarLoan:
                    case Stage.ReduceCreditCard:
                    case Stage.ReduceSmallCredit:
                    case Stage.ReduceBankLoan:
                        Actions.PayCredit(Bot, user, message.Text.Trim());
                        return;

                    case Stage.BuyStocksTitle:
                    case Stage.BuyStocksPrice:
                    case Stage.BuyStocksQtty:
                        Actions.BuyStocks(Bot, user, message.Text.Trim());
                        return;

                    case Stage.BuyLandPrice:
                    case Stage.BuyLandTitle:
                        Actions.BuyLand(Bot, user, message.Text.Trim());
                        return;

                    case Stage.SellStocksTitle:
                    case Stage.SellStocksPrice:
                        Actions.SellStocks(Bot, user, message.Text.Trim());
                        return;

                    case Stage.BuyRealEstateTitle:
                    case Stage.BuyRealEstatePrice:
                    case Stage.BuyRealEstateFirstPayment:
                    case Stage.BuyRealEstateCashFlow:
                        Actions.BuyRealEstate(Bot, user, message.Text.Trim());
                        return;

                    case Stage.SellRealEstateTitle:
                    case Stage.SellRealEstatePrice:
                        Actions.SellRealEstate(Bot, user, message.Text.Trim());
                        return;

                    case Stage.SellLandTitle:
                    case Stage.SellLandPrice:
                        Actions.SellLand(Bot, user, message.Text.Trim());
                        return;

                    case Stage.BuyBusinessTitle:
                    case Stage.BuyBusinessPrice:
                    case Stage.BuyBusinessFirstPayment:
                    case Stage.BuyBusinessCashFlow:
                        Actions.BuyBusiness(Bot, user, message.Text.Trim());
                        return;

                    case Stage.SellBusinessTitle:
                    case Stage.SellBusinessPrice:
                        Actions.SellBusiness(Bot, user, message.Text.Trim());
                        return;

                    case Stage.AdminAvailableAssets:
                        if (!user.IsAdmin) return;

                        Actions.ShowAvailableAssets(Bot, user, message.Text.Trim());
                        return;

                    case Stage.AdminAvailableAssetsClear:
                        if (!user.IsAdmin) return;

                        Actions.ClearAvailableAssets(Bot, user, message.Text.Trim());
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

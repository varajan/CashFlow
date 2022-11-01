using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using CashFlowBot.Actions;
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

            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnMessageEdited += BotOnMessageReceived;
            Bot.OnReceiveError += BotOnReceiveError;

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
                if (!user.Person.Exists && !new[] { Stage.SelectLanguage, Stage.GetProfession }.Contains(user.Stage))
                {
                    BaseActions.Start(Bot, user, message.Chat.Username);
                    return;
                }

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

                    BaseActions.Cancel(Bot, user);
                    return;
                }
                // Make user admin

                switch (message.Text.ToLower().Trim())
                {
                    case "en":
                    case "ua":
                    case "de":
                    case "/en":
                    case "/ua":
                    case "/de":
                        user.Language = message.Text.Replace("/", "").ToUpper().Trim().ParseEnum<Language>();

                        if (user.Person.Exists)
                        {
                            user.Person.Profession = Persons.Get(user.Id, user.Person.Profession).Profession;

                            BaseActions.Cancel(Bot, user);
                        }
                        else
                        {
                            BaseActions.Start(Bot, user);
                        }
                        return;

                    case "/start":
                        BaseActions.Start(Bot, user, message.Chat.Username);
                        return;

                    // Term 79: Pay Check
                    case "pay check":
                    case "грошовий потік":
                    case "gehalt":
                        var amount = user.Person.BigCircle
                            ? user.Person.CurrentCashFlow.AsCurrency()
                            : user.Person.CashFlow.AsCurrency();
                        SmallCircleActions.GetMoney(Bot, user, amount);
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

                        BaseActions.SmallCircleButtons(Bot, user,
                        Terms.Get(user.Person.Expenses.Children == 1 ? 20 : 25,
                        user, "{0}, you have {1} children expenses and {2} children.",
                        user.Person.Profession, user.Person.Expenses.ChildrenExpenses.AsCurrency(), user.Person.Expenses.Children.ToString()));
                        return;

                    // Term 80: Downsize
                    case "downsize":
                    case "звільнення":
                    case "entlassung":
                        SmallCircleActions.Downsize(Bot, user);
                        return;

                    #region My Data

                    // Term 31: Show my Data
                    case "show my data":
                    case "мої дані":
                    case "meine info":
                        SmallCircleActions.MyData(Bot, user);
                        return;

                    // Term 2: History
                    case "history":
                    case "історія":
                    case "transaktionen":
                        SmallCircleActions.History(Bot, user);
                        return;

                    // Term 34: Get Credit
                    case "get credit":
                    case "взяти кредит":
                    case "kredit bekomen":
                        switch (user.Stage)
                        {
                            case Stage.BuyRealEstateFirstPayment:
                                user.Stage = Stage.BuyRealEstateCredit;
                                BuyActions.BuyRealEstate(Bot, user, string.Empty);
                                return;

                            case Stage.BuyBusinessFirstPayment:
                                user.Stage = Stage.BuyBusinessCredit;
                                BuyActions.BuyBusiness(Bot, user, string.Empty);
                                return;

                            case Stage.StartCompanyPrice:
                                user.Stage = Stage.StartCompanyCredit;
                                BuyActions.StartCompany(Bot, user, string.Empty);
                                return;

                            case Stage.BuyLandPrice:
                                user.Stage = Stage.BuyLandCredit;
                                BuyActions.BuyLand(Bot, user, string.Empty);
                                return;

                            case Stage.BuyCoinsPrice:
                                user.Stage = Stage.BuyCoinsCredit;
                                BuyActions.BuyCoins(Bot, user, string.Empty);
                                return;

                            default:
                                CreditActions.GetCredit(Bot, user);
                                return;
                        }

                    // Term 32: Get Money
                    case "get money":
                    case "отримати гроші":
                    case "geld bekomen":
                        var buttons = user.Person.BigCircle
                            ? new[] { 50_000, 100_000, 200_000, user.Person.CurrentCashFlow }
                            : new[] { 1_000, 2_000, 5_000, user.Person.CashFlow };

                        BaseActions.Ask(Bot, user, Stage.GetMoney,
                            Terms.Get(0, user, "Your Cash Flow is *{0}*. How much should you get?",
                                user.Person.BigCircle ? user.Person.CurrentCashFlow.AsCurrency() : user.Person.CashFlow.AsCurrency()), buttons.Distinct().AsCurrency().ToArray());
                        return;

                    // Term 33: Give Money
                    case "give money":
                    case "заплатити гроші":
                    case "geld geben":
                    // Term 95: Pay with Cash
                    case "pay with cash":
                    case "оплатити готівкою":
                    case "mit bargeld zahlen":
                        var giveMoney = AvailableAssets.GetAsCurrency(user.Person.BigCircle ? AssetType.BigGiveMoney : AssetType.SmallGiveMoney);

                        BaseActions.Ask(Bot, user, Stage.GiveMoney, Terms.Get(21, user, "How much?"), giveMoney);
                        return;

                    #region Reduce Liabilities
                    // Term 40: Reduce Liabilities
                    case "reduce liabilities":
                    case "зменшити борги":
                    case "verbindlichkeiten reduzieren":
                        CreditActions.ReduceLiabilities(Bot, user);
                        return;

                    // Term 43: Mortgage
                    case "mortgage":
                    case "іпотека":
                    case "hypothek":
                        CreditActions.ReduceLiabilities(Bot, user, Stage.ReduceMortgage);
                        return;

                    // Term 44: School Loan
                    case "school loan":
                    case "кредит на освіту":
                    case "schuldarlehen":
                        CreditActions.ReduceLiabilities(Bot, user, Stage.ReduceSchoolLoan);
                        return;

                    // Term 45: Car Loan
                    case "car loan":
                    case "кредит на авто":
                    case "autokredit":
                        CreditActions.ReduceLiabilities(Bot, user, Stage.ReduceCarLoan);
                        return;

                    // Term 46: Credit Card
                    case "credit card":
                    case "кредитна картка":
                    case "kreditkarte":
                        CreditActions.ReduceLiabilities(Bot, user, Stage.ReduceCreditCard);
                        return;

                    // Term 92: Small Credit
                    case "small credit":
                    case "мікрокредит":
                    case "klein kredit":
                        CreditActions.ReduceLiabilities(Bot, user, Stage.ReduceSmallCredit);
                        return;

                    // Term 47: Bank Loan
                    case "bank loan":
                    case "банківська позика":
                    case "bankkredit":
                        CreditActions.ReduceLiabilities(Bot, user, Stage.ReduceBankLoan);
                        return;

                    // Term 114: Boat Loan
                    case "boat loan":
                    case "bootredit":
                    case "кредит за катер":
                        CreditActions.ReduceLiabilities(Bot, user, Stage.ReduceBoatLoan);
                        return;
                    #endregion

                    // Term 90: Charity - Pay 10%
                    case "charity - pay 10%":
                    case "благодійність - віддати 10%":
                    case "nächstenliebe - 10% zahlen":
                        SmallCircleActions.Charity(Bot, user);
                        return;

                    // Term 41: Stop Game
                    case "stop game":
                    case "закінчити гру":
                    case "spiel beenden":
                    case "/clear":
                        if (user.Person.Bankruptcy)
                        {
                            BaseActions.StopGame(Bot, user);
                            return;
                        }

                        BaseActions.Ask(Bot, user, Stage.StopGame,
                            Terms.Get(3, user, "Are you sure want to stop current game?"), Terms.Get(4, user, "Yes"));
                        return;

                    #endregion

                    #region Small opportunities

                    // Term 115: Start a company
                    case "start a company":
                    case "заснувати компанію":
                    case "gründe eine firma":
                        BuyActions.StartCompany(Bot, user);
                        return;

                    // Term 119: Buy coins
                    case "buy coins":
                    case "покупка монет":
                    case "münzen kaufen":
                        BuyActions.BuyCoins(Bot, user);
                        return;

                    // Term 81: Small Opportunity
                    case "small opportunity":
                    case "мала можливість":
                    case "kleine chance":
                        SmallCircleActions.SmallOpportunity(Bot, user);
                        return;

                    // Term 35: Buy Stocks
                    case "buy stocks":
                    case "купити акції":
                    case "aktien kaufen":
                        BuyActions.BuyStocks(Bot, user);
                        return;

                    // Term 36: Sell Stocks
                    case "sell stocks":
                    case "продати акції":
                    case "aktien verkaufen":
                        SellActions.SellStocks(Bot, user);
                        return;

                    // Term 37: Buy Real Estate
                    case "buy real estate":
                    case "купити нерухомість":
                    case "immobilien kaufen":
                        BuyActions.BuyRealEstate(Bot, user);
                        return;

                    // Term 82: 2 to 1
                    case "2 to 1":
                    case "2 до 1":
                    case "2 -> 1":
                        user.Stage = Stage.Stocks2to1;
                        SmallCircleActions.MultiplyStocks(Bot, user);
                        return;

                    // Term 83: 1 to 2
                    case "1 to 2":
                    case "1 до 2":
                    case "1 -> 2":
                        user.Stage = Stage.Stocks1to2;
                        SmallCircleActions.MultiplyStocks(Bot, user);
                        return;

                    #endregion

                    #region Big opportunities

                    // Term 84: Big Opportunity
                    case "big opportunity":
                    case "велика можливість":
                    case "große chance":
                        SmallCircleActions.BigOpportunity(Bot, user);
                        return;

                    // Term 74: Buy Business
                    case "buy business":
                    case "купити підприємство":
                    case "geschäft kaufen":
                        BuyActions.BuyBusiness(Bot, user);
                        return;

                    // Term 94: Buy Land
                    case "buy land":
                    case "купити землю":
                    case "land kaufen":
                        BuyActions.BuyLand(Bot, user);
                        return;
                    #endregion

                    #region Doodads
                    // Term 86: Doodads
                    case "doodads":
                    case "дрібнички":
                        SmallCircleActions.Doodads(Bot, user);
                        return;

                    // Term 96: Pay with Credit Card
                    case "pay with credit card":
                    case "оплатити кредиткою":
                    case "mit kreditkarte zahlen":
                        CreditActions.PayWithCreditCard(Bot, user);
                        return;

                    // Term 112: Buy a boat
                    case "buy a boat":
                    case "boot kaufen":
                    case "купити катер":
                        BuyActions.BuyBoat(Bot, user);
                        return;

                    #endregion

                    #region Market

                    // Term 85: Market
                    case "market":
                    case "ринок":
                    case "markt":
                        SmallCircleActions.Market(Bot, user);
                        return;

                    // Term 38: Sell Real Estate
                    case "sell real estate":
                    case "продати нерухомість":
                    case "immobilien verkaufen":
                        SellActions.SellRealEstate(Bot, user);
                        return;

                    // Term 75: Sell Business
                    case "sell business":
                    case "продати підприємство":
                    case "geschäft verkaufen":
                        SellActions.SellBusiness(Bot, user);
                        return;

                    // Term 118: Increase cash flow
                    case "increase cash flow":
                    case "збільшити грошовий потік":
                    case "cashflow erhöhen":
                        SmallCircleActions.IncreaseCashFlow(Bot, user);
                        return;

                    // Term 98 : Sell Land
                    case "sell land":
                    case "продати землю":
                    case "land verkaufen":
                        SellActions.SellLand(Bot, user);
                        return;

                    // Term 120 : Sell Coins
                    case "sell coins":
                    case "продаж монет":
                    case "münzen verkaufen":
                        SellActions.SellCoins(Bot, user);
                        return;

                    #endregion

                    // Term 69: Divorce
                    case "divorce":
                    case "розлучення":
                    case "die ehescheidung":
                        BigCircleActions.LostMoney(Bot, user, user.Person.Cash, ActionType.Divorce);
                        return;

                    // Term 1: Go to Big Circle
                    case "go to big circle":
                    case "перейти до великого кола":
                    case "eintreten den großen kreis":
                        BigCircleActions.GoToBigCircle(Bot, user);
                        return;

                    // Term 70: Tax Audit
                    case "tax audit":
                    case "die steuerprüfung":
                    case "податкова перевірка":
                        BigCircleActions.LostMoney(Bot, user, user.Person.Cash / 2, ActionType.TaxAudit);
                        return;

                    // Term 71: Lawsuit
                    case "lawsuit":
                    case "die klage":
                    case "судовий процес":
                        BigCircleActions.LostMoney(Bot, user, user.Person.Cash / 2, ActionType.Lawsuit);
                        return;

                    // Term 4 - YES
                    case "yes":
                    case "так":
                    case "ja":
                        SmallCircleActions.Confirm(Bot, user);
                        return;

                    // Term 109: Rollback last action
                    case "rollback last action":
                    case "скасувати останню операцію":
                    case "rollback der letzten transaktion":
                        BaseActions.Ask(Bot, user, Stage.Rollback,
                        Terms.Get(110, user, "Are you sure want to rollback last action?"), Terms.Get(4, user, "Yes"));
                        return;

                    // Term 6: Cancel
                    // Term 102: Main menu
                    case "main menu":
                    case "головне меню":
                    case "hauptmenü":
                    case "cancel":
                    case "скасувати":
                    case "absagen":
                    case "/cancel":
                        BaseActions.Cancel(Bot, user);
                        return;

                    #region Admin
                    case "admin":
                        if (user.IsAdmin)
                        {
                            AdminActions.AdminMenu(Bot, user);
                        }
                        else
                        {
                            AdminActions.NotifyAdmins(Bot, user);
                        }
                        return;

                    case "bring down":
                        if (!user.IsAdmin) break;

                        BaseActions.Ask(Bot, user, Stage.AdminBringDown, "Are you sure want to shut BOT down?", "Yes", "Back");
                        return;

                    case "logs":
                        if (!user.IsAdmin) break;

                        BaseActions.Ask(Bot, user, Stage.AdminLogs, "Which log would you like to get?", "Full", "Top", "Back");
                        return;

                    case "full":
                        if (!user.IsAdmin) break;

                        if (user.Stage == Stage.AdminLogs)
                        {
                            await using var stream = File.Open(Logger.LogFile, FileMode.Open);
                            var fts = new InputOnlineFile(stream, "logs.txt");
                            await Bot.SendDocumentAsync(user.Id, fts);
                            AdminActions.AdminMenu(Bot, user);
                        }
                        return;

                    case "top":
                        if (!user.IsAdmin) break;

                        if (user.Stage == Stage.AdminLogs)
                        {
                            Bot.SendMessage(user.Id, Logger.Top, ParseMode.Default);
                        }
                        AdminActions.AdminMenu(Bot, user);
                        return;

                    case "users":
                        if (!user.IsAdmin) break;

                        AdminActions.ShowUsers(Bot, user);
                        return;

                    case "back":
                        if (!user.IsAdmin) break;

                        AdminActions.AdminMenu(Bot, user);
                        return;

                    case "available assets":
                        if (!user.IsAdmin) break;

                        var assets = new List<string>();

                        foreach (var type in Enum.GetValues(typeof(AssetType)))
                        {
                            var assetType = type.ToString().ParseEnum<AssetType>();
                            var count = AvailableAssets.Get(assetType).Count();

                            if (count > 0) assets.Add($"{type} - {count}");
                        }

                        if (assets.Any())
                        {
                            BaseActions.Ask(Bot, user, Stage.AdminAvailableAssets, "What types to show?",
                            assets.Append("All").Append("Back").ToArray());
                            return;
                        }

                        Bot.SendMessage(user.Id, "There is no available assets.");
                        AdminActions.AdminMenu(Bot, user);
                        return;
                        #endregion
                }

                switch (user.Stage)
                {
                    case Stage.Stocks1to2:
                    case Stage.Stocks2to1:
                        SmallCircleActions.MultiplyStocks(Bot, user, message.Text.Trim());
                        return;

                    case Stage.GetProfession:
                        BaseActions.SetProfession(Bot, user, message.Text.Trim().ToLower());
                        return;

                    case Stage.GetCredit:
                        CreditActions.GetCredit(Bot, user, message.Text.Trim());
                        return;

                    case Stage.GetMoney:
                        SmallCircleActions.GetMoney(Bot, user, message.Text.Trim());
                        return;

                    case Stage.GiveMoney:
                        SmallCircleActions.GiveMoney(Bot, user, message.Text.Trim());
                        return;

                    case Stage.MicroCreditAmount:
                        CreditActions.PayWithCreditCard(Bot, user, message.Text.Trim());
                        return;

                    case Stage.ReduceBankLoan:
                        CreditActions.PayCredit(Bot, user, message.Text.Trim());
                        return;

                    case Stage.BuyStocksTitle:
                    case Stage.BuyStocksPrice:
                    case Stage.BuyStocksQtty:
                    case Stage.BuyStocksCashFlow:
                        BuyActions.BuyStocks(Bot, user, message.Text.Trim());
                        return;

                    case Stage.BuyLandPrice:
                    case Stage.BuyLandTitle:
                        BuyActions.BuyLand(Bot, user, message.Text.Trim());
                        return;

                    case Stage.SellStocksTitle:
                    case Stage.SellStocksPrice:
                        SellActions.SellStocks(Bot, user, message.Text.Trim());
                        return;

                    case Stage.SellCoinsTitle:
                    case Stage.SellCoinsPrice:
                        SellActions.SellCoins(Bot, user, message.Text.Trim());
                        return;

                    case Stage.StartCompanyTitle:
                    case Stage.StartCompanyCredit:
                    case Stage.StartCompanyPrice:
                        BuyActions.StartCompany(Bot, user, message.Text.Trim());
                        return;

                    case Stage.BuyCoinsTitle:
                    case Stage.BuyCoinsPrice:
                    case Stage.BuyCoinsCount:
                    case Stage.BuyCoinsCredit:
                        BuyActions.BuyCoins(Bot, user, message.Text.Trim());
                        return;

                    case Stage.BuyRealEstateTitle:
                    case Stage.BuyRealEstatePrice:
                    case Stage.BuyRealEstateFirstPayment:
                    case Stage.BuyRealEstateCashFlow:
                        BuyActions.BuyRealEstate(Bot, user, message.Text.Trim());
                        return;

                    case Stage.SellRealEstateTitle:
                    case Stage.SellRealEstatePrice:
                        SellActions.SellRealEstate(Bot, user, message.Text.Trim());
                        return;

                    case Stage.SellLandTitle:
                    case Stage.SellLandPrice:
                        SellActions.SellLand(Bot, user, message.Text.Trim());
                        return;

                    case Stage.BuyBusinessTitle:
                    case Stage.BuyBusinessPrice:
                    case Stage.BuyBusinessFirstPayment:
                    case Stage.BuyBusinessCashFlow:
                        BuyActions.BuyBusiness(Bot, user, message.Text.Trim());
                        return;

                    case Stage.SellBusinessTitle:
                    case Stage.SellBusinessPrice:
                        SellActions.SellBusiness(Bot, user, message.Text.Trim());
                        return;

                    case Stage.AdminAvailableAssets:
                        if (!user.IsAdmin) return;

                        AdminActions.ShowAvailableAssets(Bot, user, message.Text.Trim());
                        return;

                    case Stage.AdminAvailableAssetsClear:
                        if (!user.IsAdmin) return;

                        AdminActions.ClearAvailableAssets(Bot, user, message.Text.Trim());
                        return;

                    case Stage.Bankruptcy:
                        BankruptcyActions.SellAsset(Bot, user, message.Text.Trim().Replace("#", "").ToInt());
                        return;

                    case Stage.IncreaseCashFlow:
                        SmallCircleActions.IncreaseCashFlow(Bot, user, message.Text.Trim().AsCurrency());
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

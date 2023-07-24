using CashFlowBot.Actions;
using CashFlowBot.Data;
using CashFlowBot.DataBase;
using CashFlowBot.Extensions;
using CashFlowBot.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Terms = CashFlowBot.DataBase.Terms;

namespace CashFlowBot
{
    public class CashFlowBot
    {
        private static TelegramBotClient _bot;

        public static void Main()
        {
            _bot = InitBot();
            ServicePointManager.ServerCertificateValidationCallback += (_, _, _, _) => true;

            _bot.OnMessage += BotOnMessageReceived;
            _bot.OnMessageEdited += BotOnMessageReceived;
            _bot.OnReceiveError += BotOnReceiveError;

            _bot.StartReceiving(Array.Empty<UpdateType>());

            Console.WriteLine("Starting Bot.");
            Console.ReadLine();

            _bot.StopReceiving();
        }

        private static TelegramBotClient InitBot()
        {
            // debug:       1357607824:AAFbYG2hjms9b3mtlphXMiwRHEjIA13nJF8
            // production:  1991657067:AAGyDAK1xfqrfIEAFIKNsRjWOvy9owiKU40

            try
            {
                var pattern = @"^\d{10}:[a-zA-Z0-9]{35}$";
                var botIdFile = $"{AppDomain.CurrentDomain.BaseDirectory}/BotID.txt";
                var id = File.ReadAllLines(botIdFile).FirstOrDefault(x => !string.IsNullOrEmpty(x));

                if (string.IsNullOrEmpty(id)) throw new ArgumentException("id is null or empty");
                if (!Regex.IsMatch(id, pattern)) throw new InvalidDataException("Invalid bot ID");

                return new TelegramBotClient(id);
            }
            catch (Exception)
            {
                var howTo = $"{AppDomain.CurrentDomain.BaseDirectory}/index.html";
                Process.Start(new ProcessStartInfo("cmd", $"/c start {howTo}") { CreateNoWindow = true });
                throw;
            }
        }

        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            try
            {
                var message = messageEventArgs.Message;
                var user = new User(message.Chat.Id);

                Logger.Log($"{message.Chat.Id} - {message.Chat.Username} - {message.Text}");

                await _bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                if (message.Type != MessageType.Text) return;
                if (!user.Person.Exists && !new[] { Stage.SelectLanguage, Stage.GetProfession }.Contains(user.Stage))
                {
                    BaseActions.Start(_bot, user, message.From);
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

                    BaseActions.Cancel(_bot, user);
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

                            BaseActions.Cancel(_bot, user);
                        }
                        else
                        {
                            BaseActions.Start(_bot, user, message.From);
                        }
                        return;

                    case "/start":
                        BaseActions.Start(_bot, user, message.From);
                        return;

                    // Term 79: Pay Check
                    case "pay check":
                    case "грошовий потік":
                    case "gehalt":
                        var amount = user.Person.BigCircle
                            ? user.Person.CurrentCashFlow.AsCurrency()
                            : user.Person.CashFlow.AsCurrency();
                        SmallCircleActions.GetMoney(_bot, user, amount);
                        return;

                    // Term 39: Baby
                    case "baby":
                    case "дитина":
                    case "kind":
                        if (user.Person.Expenses.Children == 3)
                        {
                            _bot.SendMessage(user.Id, Terms.Get(57, user, "You're lucky parent of three children. You don't need one more."));
                            return;
                        }

                        user.Person.Expenses.Children++;
                        user.History.Add(ActionType.Child, user.Person.Expenses.Children);

                        BaseActions.SmallCircleButtons(_bot, user,
                            Terms.Get(user.Person.Expenses.Children == 1 ? 20 : 25,
                                user, "{0}, you have {1} children expenses and {2} children.",
                                user.Person.Profession, user.Person.Expenses.ChildrenExpenses.AsCurrency(), user.Person.Expenses.Children.ToString()));
                        return;

                    // Term 80: Downsize
                    case "downsize":
                    case "звільнення":
                    case "entlassung":
                        SmallCircleActions.Downsize(_bot, user);
                        return;

                    #region My Data

                    // Term 31: Show my Data
                    case "show my data":
                    case "мої дані":
                    case "meine info":
                        SmallCircleActions.MyData(_bot, user);
                        return;

                    // Term 140: Friends
                    case "friends":
                    case "друзі":
                    case "freunde":
                        BaseActions.ShowFriends(_bot, user);
                        return;

                    // Term 2: History
                    case "history":
                    case "історія":
                    case "transaktionen":
                        SmallCircleActions.History(_bot, user);
                        return;

                    // Term 34: Get Credit
                    case "get credit":
                    case "взяти кредит":
                    case "kredit bekomen":
                        switch (user.Stage)
                        {
                            case Stage.BuyRealEstateFirstPayment:
                                user.Stage = Stage.BuyRealEstateCredit;
                                BuyActions.BuyRealEstate(_bot, user, string.Empty);
                                return;

                            case Stage.BuyStocksQtty:
                                user.Stage = Stage.BuyStocksGetCredit;
                                BuyActions.BuyStocks(_bot, user, string.Empty);
                                return;

                            case Stage.BuyBusinessFirstPayment:
                                user.Stage = Stage.BuyBusinessCredit;
                                BuyActions.BuyBusiness(_bot, user, string.Empty);
                                return;

                            case Stage.StartCompanyPrice:
                                user.Stage = Stage.StartCompanyCredit;
                                BuyActions.StartCompany(_bot, user, string.Empty);
                                return;

                            case Stage.BuyLandPrice:
                                user.Stage = Stage.BuyLandCredit;
                                BuyActions.BuyLand(_bot, user, string.Empty);
                                return;

                            case Stage.BuyCoinsPrice:
                                user.Stage = Stage.BuyCoinsCredit;
                                BuyActions.BuyCoins(_bot, user, string.Empty);
                                return;

                            case Stage.TransferMoneyAmount:
                                user.Stage = Stage.TransferMoneyCredit;
                                SmallCircleActions.SendMoney(_bot, user, string.Empty);
                                return;

                            default:
                                CreditActions.GetCredit(_bot, user);
                                return;
                        }

                    // Term 32: Get Money
                    case "get money":
                    case "отримати гроші":
                    case "geld bekomen":
                        var buttons = user.Person.BigCircle
                            ? new[] { 50_000, 100_000, 200_000, user.Person.CurrentCashFlow }
                            : new[] { 1_000, 2_000, 5_000, user.Person.CashFlow };

                        BaseActions.Ask(_bot, user, Stage.GetMoney,
                            Terms.Get(0, user, "Your Cash Flow is *{0}*. How much should you get?",
                                user.Person.BigCircle ? user.Person.CurrentCashFlow.AsCurrency() : user.Person.CashFlow.AsCurrency()), buttons.Distinct().AsCurrency().ToArray());
                        return;

                    // Term 33: Give Money
                    case "give money":
                    case "заплатити гроші":
                    case "geld geben":
                        if (user.Person.BigCircle)
                        {
                            BaseActions.Ask(_bot, user, Stage.GiveMoney, Terms.Get(21, user, "How much?"),
                                AvailableAssets.GetAsCurrency(AssetType.BigGiveMoney));
                            return;
                        }

                        SmallCircleActions.SendMoney(_bot, user);
                        return;

                    // Term 95: Pay with Cash
                    case "pay with cash":
                    case "оплатити готівкою":
                    case "mit bargeld zahlen":
                        BaseActions.Ask(_bot, user, Stage.GiveMoney, Terms.Get(21, user, "How much?"),
                            AvailableAssets.GetAsCurrency(AssetType.SmallGiveMoney));
                        return;

                    #region Reduce Liabilities
                    // Term 40: Reduce Liabilities
                    case "reduce liabilities":
                    case "зменшити борги":
                    case "verbindlichkeiten reduzieren":
                        CreditActions.ReduceLiabilities(_bot, user);
                        return;

                    // Term 43: Mortgage
                    case "mortgage":
                    case "іпотека":
                    case "hypothek":
                        CreditActions.ReduceLiabilities(_bot, user, Stage.ReduceMortgage);
                        return;

                    // Term 44: School Loan
                    case "school loan":
                    case "кредит на освіту":
                    case "schuldarlehen":
                        CreditActions.ReduceLiabilities(_bot, user, Stage.ReduceSchoolLoan);
                        return;

                    // Term 45: Car Loan
                    case "car loan":
                    case "кредит на авто":
                    case "autokredit":
                        CreditActions.ReduceLiabilities(_bot, user, Stage.ReduceCarLoan);
                        return;

                    // Term 46: Credit Card
                    case "credit card":
                    case "кредитна картка":
                    case "kreditkarte":
                        CreditActions.ReduceLiabilities(_bot, user, Stage.ReduceCreditCard);
                        return;

                    // Term 92: Small Credit
                    case "small credit":
                    case "мікрокредит":
                    case "klein kredit":
                        CreditActions.ReduceLiabilities(_bot, user, Stage.ReduceSmallCredit);
                        return;

                    // Term 47: Bank Loan
                    case "bank loan":
                    case "банківська позика":
                    case "bankkredit":
                        CreditActions.ReduceLiabilities(_bot, user, Stage.ReduceBankLoan);
                        return;

                    // Term 114: Boat Loan
                    case "boat loan":
                    case "bootredit":
                    case "кредит за катер":
                        CreditActions.ReduceLiabilities(_bot, user, Stage.ReduceBoatLoan);
                        return;
                    #endregion

                    // Term 90: Charity - Pay 10%
                    case "charity - pay 10%":
                    case "благодійність - віддати 10%":
                    case "nächstenliebe - 10% zahlen":
                        SmallCircleActions.Charity(_bot, user);
                        return;

                    // Term 41: Stop Game
                    case "stop game":
                    case "закінчити гру":
                    case "spiel beenden":
                    case "/clear":
                        if (user.Person.Bankruptcy)
                        {
                            BaseActions.StopGame(_bot, user, message.From);
                            return;
                        }

                        BaseActions.Ask(_bot, user, Stage.StopGame,
                            Terms.Get(3, user, "Are you sure want to stop current game?"), Terms.Get(4, user, "Yes"));
                        return;

                    #endregion

                    #region Small opportunities

                    // Term 115: Start a company
                    case "start a company":
                    case "заснувати компанію":
                    case "gründe eine firma":
                        BuyActions.StartCompany(_bot, user);
                        return;

                    // Term 119: Buy coins
                    case "buy coins":
                    case "покупка монет":
                    case "münzen kaufen":
                        BuyActions.BuyCoins(_bot, user);
                        return;

                    // Term 81: Small Opportunity
                    case "small opportunity":
                    case "мала можливість":
                    case "kleine chance":
                        SmallCircleActions.SmallOpportunity(_bot, user);
                        return;

                    // Term 35: Buy Stocks
                    case "buy stocks":
                    case "купити акції":
                    case "aktien kaufen":
                        BuyActions.BuyStocks(_bot, user);
                        return;

                    // Term 36: Sell Stocks
                    case "sell stocks":
                    case "продати акції":
                    case "aktien verkaufen":
                        SellActions.SellStocks(_bot, user);
                        return;

                    // Term 37: Buy Real Estate
                    case "buy real estate":
                    case "купити нерухомість":
                    case "immobilien kaufen":
                        BuyActions.BuyRealEstate(_bot, user);
                        return;

                    // Term 82: Stocks x2
                    case "акції x2":
                    case "stocks x2":
                    case "aktien x2":
                        user.Stage = Stage.Stocks1to2;
                        SmallCircleActions.MultiplyStocks(_bot, user);
                        return;

                    // Term 83: Stocks ÷2
                    case "акції ÷2":
                    case "stocks ÷2":
                    case "aktien ÷2":
                    case "÷2":
                        user.Stage = Stage.Stocks2to1;
                        SmallCircleActions.MultiplyStocks(_bot, user);
                        return;

                    #endregion

                    #region Big opportunities

                    // Term 84: Big Opportunity
                    case "big opportunity":
                    case "велика можливість":
                    case "große chance":
                        SmallCircleActions.BigOpportunity(_bot, user);
                        return;

                    // Term 74: Buy Business
                    case "buy business":
                    case "купити підприємство":
                    case "geschäft kaufen":
                        BuyActions.BuyBusiness(_bot, user);
                        return;

                    // Term 94: Buy Land
                    case "buy land":
                    case "купити землю":
                    case "land kaufen":
                        BuyActions.BuyLand(_bot, user);
                        return;
                    #endregion

                    #region Doodads
                    // Term 86: Doodads
                    case "doodads":
                    case "дрібнички":
                        SmallCircleActions.Doodads(_bot, user);
                        return;

                    // Term 96: Pay with Credit Card
                    case "pay with credit card":
                    case "оплатити кредиткою":
                    case "mit kreditkarte zahlen":
                        CreditActions.PayWithCreditCard(_bot, user);
                        return;

                    // Term 112: Buy a boat
                    case "buy a boat":
                    case "boot kaufen":
                    case "купити катер":
                        BuyActions.BuyBoat(_bot, user);
                        return;

                    #endregion

                    #region Market

                    // Term 85: Market
                    case "market":
                    case "ринок":
                    case "markt":
                        SmallCircleActions.Market(_bot, user);
                        return;

                    // Term 38: Sell Real Estate
                    case "sell real estate":
                    case "продати нерухомість":
                    case "immobilien verkaufen":
                        SellActions.SellRealEstate(_bot, user);
                        return;

                    // Term 75: Sell Business
                    case "sell business":
                    case "продати підприємство":
                    case "geschäft verkaufen":
                        SellActions.SellBusiness(_bot, user);
                        return;

                    // Term 118: Increase cash flow
                    case "increase cash flow":
                    case "збільшити грошовий потік":
                    case "cashflow erhöhen":
                        SmallCircleActions.IncreaseCashFlow(_bot, user);
                        return;

                    // Term 98 : Sell Land
                    case "sell land":
                    case "продати землю":
                    case "land verkaufen":
                        SellActions.SellLand(_bot, user);
                        return;

                    // Term 120 : Sell Coins
                    case "sell coins":
                    case "продаж монет":
                    case "münzen verkaufen":
                        SellActions.SellCoins(_bot, user);
                        return;

                    #endregion

                    // Term 69: Divorce
                    case "divorce":
                    case "розлучення":
                    case "die ehescheidung":
                        BigCircleActions.LostMoney(_bot, user, user.Person.Cash, ActionType.Divorce);
                        return;

                    // Term 1: Go to Big Circle
                    case "go to big circle":
                    case "перейти до великого кола":
                    case "eintreten den großen kreis":
                        BigCircleActions.GoToBigCircle(_bot, user);
                        return;

                    // Term 70: Tax Audit
                    case "tax audit":
                    case "die steuerprüfung":
                    case "податкова перевірка":
                        BigCircleActions.LostMoney(_bot, user, user.Person.Cash / 2, ActionType.TaxAudit);
                        return;

                    // Term 71: Lawsuit
                    case "lawsuit":
                    case "die klage":
                    case "судовий процес":
                        BigCircleActions.LostMoney(_bot, user, user.Person.Cash / 2, ActionType.Lawsuit);
                        return;

                    // Term 4 - YES
                    case "yes":
                    case "так":
                    case "ja":
                        SmallCircleActions.Confirm(_bot, user, message.From);
                        return;

                    // Term 109: Rollback last action
                    case "rollback last action":
                    case "скасувати останню операцію":
                    case "rollback der letzten transaktion":
                        user.History.Rollback();
                        SmallCircleActions.History(_bot, user);
                        return;

                    // Term 6: Cancel
                    // Term 102: Main menu
                    // Term 138: No
                    case "main menu":
                    case "головне меню":
                    case "hauptmenü":
                    case "cancel":
                    case "скасувати":
                    case "absagen":
                    case "/cancel":
                    case "ні":
                    case "no":
                    case "nein":
                        BaseActions.Cancel(_bot, user);
                        return;

                    #region Admin
                    case "admin":
                        if (user.IsAdmin)
                        {
                            AdminActions.AdminMenu(_bot, user);
                        }
                        else
                        {
                            AdminActions.NotifyAdmins(_bot, user);
                        }
                        return;

                    case "bring down":
                        if (!user.IsAdmin) break;

                        BaseActions.Ask(_bot, user, Stage.AdminBringDown, "Are you sure want to shut BOT down?", "Yes", "Back");
                        return;

                    case "logs":
                        if (!user.IsAdmin) break;

                        BaseActions.Ask(_bot, user, Stage.AdminLogs, "Which log would you like to get?", "Full", "Top", "Back");
                        return;

                    case "full":
                        if (!user.IsAdmin) break;

                        if (user.Stage == Stage.AdminLogs)
                        {
                            await using var stream = File.Open(Logger.LogFile, FileMode.Open);
                            var fts = new InputOnlineFile(stream, "logs.txt");
                            await _bot.SendDocumentAsync(user.Id, fts);
                            AdminActions.AdminMenu(_bot, user);
                        }
                        return;

                    case "top":
                        if (!user.IsAdmin) break;

                        if (user.Stage == Stage.AdminLogs)
                        {
                            _bot.SendMessage(user.Id, Logger.Top, ParseMode.Default);
                        }
                        AdminActions.AdminMenu(_bot, user);
                        return;

                    case "users":
                        if (!user.IsAdmin) break;

                        AdminActions.ShowUsers(_bot, user);
                        return;

                    case "back":
                        if (!user.IsAdmin) break;

                        AdminActions.AdminMenu(_bot, user);
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
                            BaseActions.Ask(_bot, user, Stage.AdminAvailableAssets, "What types to show?",
                                assets.Append("All").Append("Back").ToArray());
                            return;
                        }

                        _bot.SendMessage(user.Id, "There is no available assets.");
                        AdminActions.AdminMenu(_bot, user);
                        return;
                        #endregion
                }

                switch (user.Stage)
                {
                    case Stage.ShowFriendData:
                        var friend = Users.AllUsers.OrderBy(x => x.LastActive).LastOrDefault(x => x.Name == message.Text);
                        if (friend != null)
                        {
                            _bot.SendMessage(user.Id, friend.Person.BigCircle ? friend.Person.Description : friend.Description);
                            _bot.SendMessage(user.Id, friend.History.TopFive);
                        }
                        BaseActions.ShowFriends(_bot, user);
                        return;

                    case Stage.Stocks1to2:
                    case Stage.Stocks2to1:
                        SmallCircleActions.MultiplyStocks(_bot, user, message.Text.Trim());
                        return;

                    case Stage.GetProfession:
                        BaseActions.SetProfession(_bot, user, message.Text.Trim().ToLower());
                        return;

                    case Stage.GetCredit:
                        CreditActions.GetCredit(_bot, user, message.Text.Trim());
                        return;

                    case Stage.GetMoney:
                        SmallCircleActions.GetMoney(_bot, user, message.Text.Trim());
                        return;

                    case Stage.GiveMoney:
                        SmallCircleActions.GiveMoney(_bot, user, message.Text.Trim());
                        return;

                    case Stage.TransferMoneyTo:
                    case Stage.TransferMoneyAmount:
                    case Stage.TransferMoneyCredit:
                        SmallCircleActions.SendMoney(_bot, user, message.Text.Trim());
                        return;

                    case Stage.MicroCreditAmount:
                        CreditActions.PayWithCreditCard(_bot, user, message.Text.Trim());
                        return;

                    case Stage.ReduceBankLoan:
                        CreditActions.PayCredit(_bot, user, message.Text.Trim());
                        return;

                    case Stage.BuyStocksTitle:
                    case Stage.BuyStocksPrice:
                    case Stage.BuyStocksQtty:
                    case Stage.BuyStocksCashFlow:
                        BuyActions.BuyStocks(_bot, user, message.Text.Trim());
                        return;

                    case Stage.BuyLandPrice:
                    case Stage.BuyLandTitle:
                        BuyActions.BuyLand(_bot, user, message.Text.Trim());
                        return;

                    case Stage.SellStocksTitle:
                    case Stage.SellStocksPrice:
                        SellActions.SellStocks(_bot, user, message.Text.Trim());
                        return;

                    case Stage.SellCoinsTitle:
                    case Stage.SellCoinsPrice:
                        SellActions.SellCoins(_bot, user, message.Text.Trim());
                        return;

                    case Stage.StartCompanyTitle:
                    case Stage.StartCompanyCredit:
                    case Stage.StartCompanyPrice:
                        BuyActions.StartCompany(_bot, user, message.Text.Trim());
                        return;

                    case Stage.BuyCoinsTitle:
                    case Stage.BuyCoinsPrice:
                    case Stage.BuyCoinsCount:
                    case Stage.BuyCoinsCredit:
                        BuyActions.BuyCoins(_bot, user, message.Text.Trim());
                        return;

                    case Stage.BuyRealEstateTitle:
                    case Stage.BuyRealEstatePrice:
                    case Stage.BuyRealEstateFirstPayment:
                    case Stage.BuyRealEstateCashFlow:
                        BuyActions.BuyRealEstate(_bot, user, message.Text.Trim());
                        return;

                    case Stage.SellRealEstateTitle:
                    case Stage.SellRealEstatePrice:
                        SellActions.SellRealEstate(_bot, user, message.Text.Trim());
                        return;

                    case Stage.SellLandTitle:
                    case Stage.SellLandPrice:
                        SellActions.SellLand(_bot, user, message.Text.Trim());
                        return;

                    case Stage.BuyBusinessTitle:
                    case Stage.BuyBusinessPrice:
                    case Stage.BuyBusinessFirstPayment:
                    case Stage.BuyBusinessCashFlow:
                        BuyActions.BuyBusiness(_bot, user, message.Text.Trim());
                        return;

                    case Stage.SellBusinessTitle:
                    case Stage.SellBusinessPrice:
                        SellActions.SellBusiness(_bot, user, message.Text.Trim());
                        return;

                    case Stage.AdminAvailableAssets:
                        if (!user.IsAdmin) return;

                        AdminActions.ShowAvailableAssets(_bot, user, message.Text.Trim());
                        return;

                    case Stage.AdminAvailableAssetsClear:
                        if (!user.IsAdmin) return;

                        AdminActions.ClearAvailableAssets(_bot, user, message.Text.Trim());
                        return;

                    case Stage.Bankruptcy:
                        BankruptcyActions.SellAsset(_bot, user, message.Text.Trim().Replace("#", "").ToInt());
                        return;

                    case Stage.IncreaseCashFlow:
                        SmallCircleActions.IncreaseCashFlow(_bot, user, message.Text.Trim().AsCurrency());
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

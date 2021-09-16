using System;
using System.Collections.Generic;
using System.Linq;
using CashFlowBot.Data;
using CashFlowBot.DataBase;
using CashFlowBot.Extensions;
using CashFlowBot.Models;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Terms = CashFlowBot.DataBase.Terms;

namespace CashFlowBot
{
    public static class Actions
    {
        public static void AdminMenu(TelegramBotClient bot, User user)
        {
            user.Stage = Stage.Admin;
            bot.SetButtons(user.Id, "Hi, Admin.", "Logs", "Bring Down", "Users", Terms.Get(6, user, "Cancel"));
        }

        public static void NotifyAdmins(TelegramBotClient bot, User user)
        {
            var rkm = new ReplyKeyboardMarkup
            {
                Keyboard = new[] { $"Make {user.Id} admin", Terms.Get(6, user, "Cancel") }.Select(button => new KeyboardButton[] { button })
            };

            foreach (var usr in Users.AllUsers)
            {
                if (!usr.IsAdmin) continue;

                bot.SendTextMessageAsync(usr.Id, $"{user.Name} wants to become Admin.", replyMarkup: rkm, parseMode: ParseMode.Default);
            }
        }

        public static void ShowUsers(TelegramBotClient bot, User user)
        {
            var users = Users.AllUsers.Select(x => $"{(x.IsAdmin ? "A" : "")}[{x.Id}] {x.Name}").ToList();
            bot.SendMessage(user.Id, $"There are {users.Count} users.");
            bot.SendMessage(user.Id, string.Join(Environment.NewLine, users), ParseMode.Default);
        }

        public static void BuyProperty(TelegramBotClient bot, User user)
        {
            var properties = AvailableAssets.Get(AssetType.PropertyType).ToArray();

            if (user.Person.Cash == 0)
            {
                SmallCircleButtons(bot, user, Terms.Get(5, user, "You don't have enough money"));
                return;
            }

            user.Stage = Stage.BuyPropertyTitle;
            bot.SetButtons(user.Id, Terms.Get(7, user, "Title:"), properties.Append(Terms.Get(6, user, "Cancel")));
        }

        public static void BuyProperty(TelegramBotClient bot, User user, string value)
        {
            var title = value.Trim().ToUpper();
            var number = value.AsCurrency();
            var prices = AvailableAssets.Get(AssetType.PropertyBuyPrice).AsCurrency().Append(Terms.Get(6, user, "Cancel"));
            var firstPayments = AvailableAssets.Get(AssetType.PropertyFirstPayment).AsCurrency().Append(Terms.Get(6, user, "Cancel"));
            var cashFlows = AvailableAssets.Get(AssetType.PropertyCashFlow).AsCurrency().Append(Terms.Get(6, user, "Cancel"));

            switch (user.Stage)
            {
                case Stage.BuyPropertyTitle:
                    user.Stage = Stage.BuyPropertyPrice;
                    user.Person.Assets.Add(title, AssetType.Property);
                    bot.SetButtons(user.Id, Terms.Get(8, user, "What is the price?"), prices);
                    return;

                case Stage.BuyPropertyPrice:
                    if (number <= 0)
                    {
                        bot.SetButtons(user.Id, Terms.Get(9, user, "Invalid price value. Try again."), prices);
                        return;
                    }

                    user.Person.Assets.Properties.First(a => a.Price == 0).Price = number;
                    user.Stage = Stage.BuyPropertyFirstPayment;

                    bot.SetButtons(user.Id, Terms.Get(10, user, "What is the first payment?"), firstPayments);
                    return;

                case Stage.BuyPropertyFirstPayment:
                    if (number <= 0)
                    {
                        bot.SendMessage(user.Id, Terms.Get(11, user, "Invalid first payment amount."));
                        return;
                    }

                    var asset = user.Person.Assets.Properties.First(a => a.Mortgage == 0);
                    asset.Mortgage = asset.Price - number;
                    user.Stage = Stage.BuyPropertyCashFlow;

                    bot.SetButtons(user.Id, Terms.Get(12, user, "What is the cash flow?"), cashFlows);
                    return;

                case Stage.BuyPropertyCashFlow:
                    var property = user.Person.Assets.Properties.First(a => a.CashFlow == 0);
                    property.CashFlow = number;
                    user.Stage = Stage.BuyPropertyCashFlow;

                    AvailableAssets.Add(property.Title, AssetType.PropertyType);
                    AvailableAssets.Add(property.Price, AssetType.PropertyBuyPrice);
                    AvailableAssets.Add(property.Price-property.Mortgage, AssetType.PropertyFirstPayment);
                    AvailableAssets.Add(property.CashFlow, AssetType.PropertyCashFlow);

                    SmallCircleButtons(bot, user, Terms.Get(13, user, "Done."));
                    return;
            }
        }

        public static void SellProperty(TelegramBotClient bot, User user)
        {
            var properties = user.Person.Assets.Properties;

            if (properties.Any())
            {
                var propertyIds = new List<string>();
                var propertyList = string.Empty;

                for (int i = 0; i < properties.Count; i++)
                {
                    propertyIds.Add($"#{i+1}");
                    propertyList += $"{Environment.NewLine}*#{i+1}* - {properties[i].Description}";
                }

                propertyIds.Add(Terms.Get(6, user, "Cancel"));
                user.Stage = Stage.SellPropertyTitle;

                bot.SetButtons(user.Id, Terms.Get(14, user, "What property do you want to sell?{0}{1}", Environment.NewLine, propertyList), propertyIds);
                return;
            }

            SmallCircleButtons(bot, user, Terms.Get(15, user, "You have no properties."));
        }

        public static void SellProperty(TelegramBotClient bot, User user, string value)
        {
            var properties = user.Person.Assets.Properties;
            var prices = AvailableAssets.Get(AssetType.PropertySellPrice).AsCurrency().Append(Terms.Get(6, user, "Cancel"));

            switch (user.Stage)
            {
                case Stage.SellPropertyTitle:
                    var index = value.Replace("#", "").ToInt();

                    if (index < 1 || index > properties.Count)
                    {
                        bot.SendMessage(user.Id, Terms.Get(16, user, "Invalid property number."));
                        return;
                    }

                    properties[index - 1].Title += "*";
                    user.Stage = Stage.SellPropertyPrice;
                    bot.SetButtons(user.Id, Terms.Get(8, user, "What is the price?"), prices);
                    return;

                case Stage.SellPropertyPrice:
                    var price = value.AsCurrency();
                    var property = properties.First(x => x.Title.EndsWith("*"));

                    user.Person.Cash += price - property.Mortgage;
                    property.Delete();

                    AvailableAssets.Add(price, AssetType.PropertySellPrice);

                    SmallCircleButtons(bot, user, Terms.Get(13, user, "Done."));
                    return;
            }
        }

        public static void BuyStocks(TelegramBotClient bot, User user)
        {
            var stocks = AvailableAssets.Get(AssetType.Stock).Append(Terms.Get(6, user, "Cancel"));

            if (user.Person.Cash == 0)
            {
                SmallCircleButtons(bot, user, Terms.Get(5, user, "You don't have enough money"));
                return;
            }

            user.Stage = Stage.BuyStocksTitle;
            bot.SetButtons(user.Id, Terms.Get(7, user, "Title:"), stocks);
        }

        public static void BuyStocks(TelegramBotClient bot, User user, string value)
        {
            var title = value.Trim().ToUpper();
            var number = value.AsCurrency();
            var prices = AvailableAssets.Get(AssetType.StockPrice).AsCurrency().ToList();
            prices.Add(Terms.Get(6, user, "Cancel"));

            switch (user.Stage)
            {
                case Stage.BuyStocksTitle:

                    user.Stage = Stage.BuyStocksPrice;
                    user.Person.Assets.Add(title, AssetType.Stock);
                    bot.SetButtons(user.Id, Terms.Get(8, user, "What is the price?"), prices);
                    return;

                case Stage.BuyStocksPrice:
                    if (number <= 0)
                    {
                        bot.SetButtons(user.Id, Terms.Get(9, user, "Invalid price value. Try again."), prices);
                        return;
                    }

                    user.Person.Assets.Stocks.First(a => a.Price == 0).Price = number;
                    user.Stage = Stage.BuyStocksQtty;

                    int upToQtty = user.Person.Cash / number;
                    bot.SetButtons(user.Id,
                    Terms.Get(17, user,"You can buy up to {0} stocks. How much stocks would you like to buy?", upToQtty),
                    upToQtty.ToString(),
                    Terms.Get(6, user, "Cancel"));
                    return;

                case Stage.BuyStocksQtty:
                    if (number <= 0)
                    {
                        bot.SendMessage(user.Id, Terms.Get(18, user, "Invalid quantity value. Try again."));
                        return;
                    }

                    var asset = user.Person.Assets.Stocks.First(a => a.Qtty == 0);

                    var totalPrice = asset.Price * number;
                    var availableCash = user.Person.Cash;
                    int availableQtty = availableCash / asset.Price;

                    var defaultMsg = "{0} x {1} = {2}. You have only {3}. You can buy {4} stocks. So, what quantity of stocks do you want to buy?";
                    var message = Terms.Get(19, user, defaultMsg, number, asset.Price.AsCurrency(), totalPrice.AsCurrency(), availableCash.AsCurrency(), availableQtty);

                    if (totalPrice > availableCash)
                    {
                        bot.SendMessage(user.Id, message);
                        return;
                    }

                    asset.Qtty = number;
                    user.Person.Cash -= totalPrice;

                    AvailableAssets.Add(asset.Title, AssetType.Stock);
                    AvailableAssets.Add(asset.Price, AssetType.StockPrice);

                    SmallCircleButtons(bot, user, Terms.Get(13, user, "Done."));
                    return;
            }
        }

        public static void SellStocks(TelegramBotClient bot, User user)
        {
            var stocks = user.Person.Assets.Stocks
                .Select(x => x.Title)
                .Distinct()
                .Append(Terms.Get(6, user, "Cancel"))
                .ToList();

            if (stocks.Count > 1)
            {
                user.Stage = Stage.SellStocksTitle;
                bot.SetButtons(user.Id, Terms.Get(27, user, "What stocks do you want to sell?"), stocks);
            }
            else
            {
                SmallCircleButtons(bot, user, Terms.Get(49, user, "You have no stocks."));
            }
        }

        public static void SellStocks(TelegramBotClient bot, User user, string value)
        {
            var title = value.Trim().ToUpper();
            var number = value.AsCurrency();
            var stocks = user.Person.Assets.Stocks;
            var prices = AvailableAssets.Get(AssetType.StockPrice).AsCurrency().ToList();
            prices.Add(Terms.Get(6, user, "Cancel"));

            switch (user.Stage)
            {
                case Stage.SellStocksTitle:
                    var assets = stocks.Where(x => x.Title == title).ToList();

                    if (!assets.Any())
                    {
                        SmallCircleButtons(bot, user, "Invalid stocks name.");
                        return;
                    }

                    user.Stage = Stage.SellStocksPrice;
                    assets.ForEach(x => x.Title += "*");
                    bot.SetButtons(user.Id, Terms.Get(8, user, "What is the price?"), prices);
                    return;

                case Stage.SellStocksPrice:
                    var stocksToSell = stocks.Where(x => x.Title.EndsWith("*")).ToList();
                    var qtty = stocksToSell.Sum(x => x.Qtty);

                    if (number <= 0)
                    {
                        bot.SetButtons(user.Id, Terms.Get(9, user, "Invalid price value. Try again."), prices);
                        return;
                    }

                    user.Person.Cash += qtty * number;
                    stocksToSell.ForEach(x => x.Delete());

                    AvailableAssets.Add(number, AssetType.StockPrice);

                    SmallCircleButtons(bot, user, Terms.Get(13, user, "Done."));
                    return;
            }
        }

        public static void ReduceLiabilities(TelegramBotClient bot, User user, Stage stage)
        {
            if (user.Person.Cash < 1000)
            {
                Cancel(bot, user);
                return;
            }

            user.Stage = stage;
            int cost = 0;

            switch (stage)
            {
                case Stage.ReduceMortgage:
                    cost = user.Person.Liabilities.Mortgage;
                    break;

                case Stage.ReduceSchoolLoan:
                    cost = user.Person.Liabilities.SchoolLoan;
                    break;

                case Stage.ReduceCarLoan:
                    cost = user.Person.Liabilities.CarLoan;
                    break;

                case Stage.ReduceCreditCard:
                    cost = user.Person.Liabilities.CreditCard;
                    break;

                case Stage.ReduceBankLoan:
                    cost = user.Person.Liabilities.BankLoan;
                    break;
            }

            var buttons = new[] { 1000, 5000, 10000, cost, user.Person.Cash/1000*1000 }
                .Where(x => x <= user.Person.Cash && x <= cost)
                .OrderBy(x => x)
                .Distinct()
                .Select(x => x.AsCurrency())
                .ToList();
            buttons.Add(Terms.Get(6, user, "Cancel"));

            bot.SetButtons(user.Id, Terms.Get(20, user, "How much would you pay?"), buttons);
        }

        public static void ReduceLiabilities(TelegramBotClient bot, User user)
        {
            var l = user.Person.Liabilities;
            var x = user.Person.Expenses;
            var buttons = new List<string>();
            var liabilities = string.Empty;
            var monthly = Terms.Get(42, user, "monthly");
            var mortgage = Terms.Get(43, user, "Mortgage");
            var schoolLoan = Terms.Get(44, user, "School Loan");
            var carLoan = Terms.Get(45, user, "Car Loan");
            var creditCard = Terms.Get(46, user, "Credit Card");
            var bankLoan = Terms.Get(47, user, "Bank Loan");

            if (l.Mortgage > 0)
            {
                buttons.Add(mortgage);
                liabilities += $"*{mortgage}:* {l.Mortgage.AsCurrency()} - {x.Mortgage.AsCurrency()} {monthly}{Environment.NewLine}";
            }

            if (l.SchoolLoan > 0)
            {
                buttons.Add(schoolLoan);
                liabilities += $"*{schoolLoan}:* {l.SchoolLoan.AsCurrency()} - {x.SchoolLoan.AsCurrency()} {monthly}{Environment.NewLine}";
            }

            if (l.CarLoan > 0)
            {
                buttons.Add(carLoan);
                liabilities += $"*{carLoan}:* {l.CarLoan.AsCurrency()} - {x.CarLoan.AsCurrency()} {monthly}{Environment.NewLine}";
            }

            if (l.CreditCard > 0)
            {
                buttons.Add(creditCard);
                liabilities += $"*{creditCard}:* {l.CreditCard.AsCurrency()} - {x.CreditCard.AsCurrency()} {monthly}{Environment.NewLine}";
            }

            if (l.BankLoan > 0)
            {
                buttons.Add(bankLoan);
                liabilities += $"*{bankLoan}:* {l.BankLoan.AsCurrency()} - {x.BankLoan.AsCurrency()} {monthly}{Environment.NewLine}";
            }

            if (user.Person.Cash < 1000)
            {
                bot.SendMessage(user.Id, liabilities);
                SmallCircleButtons(bot, user, Terms.Get(48, user, "You don't have money to reduce liabilities, your balance is {0}", user.Person.Cash.AsCurrency()));
                return;
            }

            if (buttons.Any())
            {
                buttons.Add(Terms.Get(6, user, "Cancel"));
                bot.SetButtons(user.Id, liabilities, buttons);
                return;
            }

            Cancel(bot, user);
        }

        public static void ShowData(TelegramBotClient bot, User user) => SmallCircleButtons(bot, user, user.Description);

        public static void GetCredit(TelegramBotClient bot, User user)
        {
            user.Stage = Stage.GetCredit;
            bot.SetButtons(user.Id, Terms.Get(21, user, "How much?"), "1000", "2000", "5000", "10 000", "20 000", Terms.Get(6, user, "Cancel"));
        }

        public static void GetMoney(TelegramBotClient bot, User user, string value)
        {
            var amount = value.AsCurrency();
            user.Person.Cash += amount;

            SmallCircleButtons(bot, user, Terms.Get(22, user, "Ok, you've got {0}", amount.AsCurrency()));
        }

        public static void GiveMoney(TelegramBotClient bot, User user, string value)
        {
            var amount = value.AsCurrency();

            if (user.Person.Cash < amount)
            {
                SmallCircleButtons(bot, user, Terms.Get(23, user, "You don't have {0}, but only {1}", amount.AsCurrency(), user.Person.Cash.AsCurrency()));
                return;
            }

            user.Person.Cash -= amount;

            AvailableAssets.Add(amount, AssetType.GiveMoney);
            SmallCircleButtons(bot, user, user.Description);
        }

        public static void GetCredit(TelegramBotClient bot, User user, string value)
        {
            var amount = value.AsCurrency();

            if (amount % 1000 > 0 || amount < 1000)
            {
                bot.SendMessage(user.Id, Terms.Get(24, user, "Invalid amount. The amount must be a multiple of 1000"));
                return;
            }

            user.Person.Cash += amount;
            user.Person.Expenses.BankLoan += amount / 10;
            user.Person.Liabilities.BankLoan += amount;

            SmallCircleButtons(bot, user, Terms.Get(22, user, "Ok, you've got {0}", amount.AsCurrency()));
        }

        public static void PayCredit(TelegramBotClient bot, User user, string value, Stage stage)
        {
            var amount = value.AsCurrency();
            int expenses;

            if (amount % 1000 > 0 || amount < 1000)
            {
                bot.SendMessage(user.Id, Terms.Get(24, user, "Invalid amount. The amount must be a multiple of 1000"));
                return;
            }

            if (amount > user.Person.Cash)
            {
                bot.SendMessage(user.Id, Terms.Get(23, user, "You don't have {0}, but only {1}", amount.AsCurrency(), user.Person.Cash.AsCurrency()));
                return;
            }

            switch (stage)
            {
                case Stage.ReduceMortgage:
                    amount = Math.Min(amount, user.Person.Liabilities.Mortgage);
                    expenses = user.Person.Expenses.Mortgage * amount / user.Person.Liabilities.Mortgage;

                    user.Person.Cash -= amount;
                    user.Person.Expenses.Mortgage -= expenses;
                    user.Person.Liabilities.Mortgage -= amount;
                    break;

                case Stage.ReduceSchoolLoan:
                    amount = Math.Min(amount, user.Person.Liabilities.SchoolLoan);
                    expenses = user.Person.Expenses.SchoolLoan * amount / user.Person.Liabilities.SchoolLoan;

                    user.Person.Cash -= amount;
                    user.Person.Expenses.SchoolLoan -= expenses;
                    user.Person.Liabilities.SchoolLoan -= amount;
                    break;

                case Stage.ReduceCarLoan:
                    amount = Math.Min(amount, user.Person.Liabilities.CarLoan);
                    expenses = user.Person.Expenses.CarLoan * amount / user.Person.Liabilities.CarLoan;

                    user.Person.Cash -= amount;
                    user.Person.Expenses.CarLoan -= expenses;
                    user.Person.Liabilities.CarLoan -= amount;
                    break;

                case Stage.ReduceCreditCard:
                    amount = Math.Min(amount, user.Person.Liabilities.CreditCard);
                    expenses = user.Person.Expenses.CreditCard * amount / user.Person.Liabilities.CreditCard;

                    user.Person.Cash -= amount;
                    user.Person.Expenses.CreditCard -= expenses;
                    user.Person.Liabilities.CreditCard -= amount;
                    break;

                case Stage.ReduceBankLoan:
                    amount = Math.Min(amount, user.Person.Liabilities.BankLoan);
                    expenses = amount / 10;

                    user.Person.Cash -= amount;
                    user.Person.Expenses.BankLoan -= expenses;
                    user.Person.Liabilities.BankLoan -= amount;
                    break;
            }

            Cancel(bot, user);
        }

        public static void Cancel(TelegramBotClient bot, User user)
        {
            if (user.Person.BigCircle)
            {
                BigCircleButtons(bot, user);
                return;
            }

            SmallCircleButtons(bot, user, user.Person.Description);
        }

        public static void Confirm(TelegramBotClient bot, User user)
        {
            switch (user.Stage)
            {
                case Stage.GetChild:
                    user.Person.Expenses.Children++;
                    SmallCircleButtons(bot, user, Terms.Get(25, user, "{0}, you have {1} children expenses.",
                    user.Person.Profession, user.Person.Expenses.ChildrenExpenses.AsCurrency()));
                    return;

                case Stage.StopGame:
                    user.Person.Clear();
                    user.Person.Expenses.Clear();
                    user.Stage = Stage.Nothing;

                    Start(bot, user);
                    return;

                case Stage.AdminBringDown:
                    Environment.Exit(0);
                    return;
            }

            Cancel(bot, user);
        }

        public static void Ask(TelegramBotClient bot, User user, Stage stage, string question, params string[] buttons)
        {
            user.Stage = stage;

            bot.SetButtons(user.Id, question, buttons.Append(Terms.Get(6, user, "Cancel")));
        }

        public static void ChangeLanguage(TelegramBotClient bot, User user) =>
            Ask(bot, user, Stage.Nothing, "Language/Мова", "EN", "UA");

        public static void Start(TelegramBotClient bot, User user, string name = null)
        {
            var professions = Persons.Get(user.Id).Select(x => x.Profession).ToArray();

            if (!user.Exists)
            {
                user.Create();
                user.Name = name ?? "N/A";
                user.IsAdmin = !Users.AllUsers.Any();
                ChangeLanguage(bot, user);
                return;
            }

            if (user.Person.Exists)
            {
                bot.SetButtons(user.Id,
                Terms.Get(26, user, "Please stop current game before starting a new one."),
                Terms.Get(41, user, "Stop Game"),
                Terms.Get(6, user, "Cancel"));
                return;
            }

            user.Stage = Stage.GetProfession;
            bot.SetButtons(user.Id, Terms.Get(28, user, "Choose your *profession*"), professions);
        }

        public static void SetProfession(TelegramBotClient bot, User user, string profession)
        {
            var professions = Persons.Get(user.Id).Select(x => x.Profession.ToLower());

            if (!professions.Contains(profession))
            {
                bot.SendMessage(user.Id, Terms.Get(29, user, "Profession not found. Try again."));
                return;
            }

            user.Person.Create(profession);

            SmallCircleButtons(bot, user, Terms.Get(30, user, "Welcome, {0}!", user.Person.Profession));
        }

        private static async void BigCircleButtons(TelegramBotClient bot, User user)
        {
            var rkm = new ReplyKeyboardMarkup
            {
                Keyboard = new List<IEnumerable<KeyboardButton>>
                {
                    new List<KeyboardButton>{Terms.Get(32, user, "Get Money")},
                    new List<KeyboardButton>{Terms.Get(33, user, "Give Money")},
                    new List<KeyboardButton>{Terms.Get(-1, user, "Divorce")}, // todo. Term. all cash
                    new List<KeyboardButton>{Terms.Get(-1, user, "Tax Audit")}, // todo. Term. half cash
                    new List<KeyboardButton>{Terms.Get(-1, user, "Lawsuit")}, // todo. Term. half cash
                    new List<KeyboardButton>{Terms.Get(41, user, "Stop Game")}
                }
            };

            user.Stage = Stage.Nothing;

            await bot.SendTextMessageAsync(user.Id, user.Person.Description, replyMarkup: rkm, parseMode: ParseMode.Markdown);
        }

        private static async void SmallCircleButtons(TelegramBotClient bot, User user, string message)
        {
            if (user.Person.Assets.Income > user.Person.Expenses.Total)
            {
                bot.SendMessage(user.Id, Terms.Get(68, user, "Your income is greater, then expenses. You are ready for Big Circle."));
                user.Person.InitialCashFlow = user.Person.Assets.Income / 10 * 1000;
                user.Person.Cash += user.Person.InitialCashFlow;
                user.Person.BigCircle = true;
                Cancel(bot, user);
                return;
            }

            var rkm = new ReplyKeyboardMarkup
            {
                Keyboard = new List<IEnumerable<KeyboardButton>>
                {
                    new List<KeyboardButton>{Terms.Get(31, user, "Show my Data")},
                    new List<KeyboardButton>{Terms.Get(32, user, "Get Money"), Terms.Get(33, user, "Give Money"), Terms.Get(34, user, "Get Credit")},
                    new List<KeyboardButton>{Terms.Get(35, user, "Buy Stocks"), Terms.Get(36, user, "Sell Stocks")},
                    new List<KeyboardButton>{Terms.Get(37, user, "Buy Property"), Terms.Get(38, user, "Sell Property")},
                    new List<KeyboardButton>{Terms.Get(39, user, "Add Child"), Terms.Get(40, user, "Reduce Liabilities")},
                    new List<KeyboardButton>{Terms.Get(41, user, "Stop Game")}
                }
            };

            user.Person.Assets.CleanUp();
            user.Stage = Stage.Nothing;

            await bot.SendTextMessageAsync(user.Id, message, replyMarkup: rkm, parseMode: ParseMode.Markdown);
        }
    }
}

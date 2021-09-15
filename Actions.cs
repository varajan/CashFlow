using System;
using System.Collections.Generic;
using System.Linq;
using CashFlowBot.Data;
using CashFlowBot.Extensions;
using CashFlowBot.Models;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CashFlowBot
{
    public static class Actions
    {
        public static void BuyProperty(TelegramBotClient bot, long userId)
        {
            var user = new User(userId);
            var properties = AvailableAssets.Get(AssetType.PropertyType).ToArray();

            if (user.Person.Cash == 0)
            {
                SetDefaultButtons(bot, user, "You have no money to buy property.");
                return;
            }

            user.Stage = Stage.BuyPropertyTitle;
            bot.SetButtons(user.Id, "Title:", properties.Append("Cancel"));
        }

        public static void BuyProperty(TelegramBotClient bot, long userId, string value)
        {
            var user = new User(userId);
            var title = value.Trim().ToUpper();
            var number = value.AsCurrency();
            var prices = AvailableAssets.Get(AssetType.PropertyBuyPrice).Append("Cancel");
            var firstPayments = AvailableAssets.Get(AssetType.PropertyFirstPayment).Append("Cancel");
            var cashFlows = AvailableAssets.Get(AssetType.PropertyCashFlow).Append("Cancel");

            switch (user.Stage)
            {
                case Stage.BuyPropertyTitle:
                    user.Stage = Stage.BuyPropertyPrice;
                    user.Person.Assets.Add(title, AssetType.Property);
                    bot.SetButtons(user.Id, "Price?", prices);
                    return;

                case Stage.BuyPropertyPrice:
                    if (number <= 0)
                    {
                        bot.SetButtons(user.Id, "Invalid price value. Try again.", prices);
                        return;
                    }

                    user.Person.Assets.Properties.First(a => a.Price == 0).Price = number;
                    user.Stage = Stage.BuyPropertyFirstPayment;

                    bot.SetButtons(user.Id, "What is the first payment?", firstPayments);
                    return;

                case Stage.BuyPropertyFirstPayment:
                    if (number <= 0)
                    {
                        bot.SendMessage(user.Id, "Invalid first payment amount.");
                        return;
                    }

                    var asset = user.Person.Assets.Properties.First(a => a.Mortgage == 0);
                    asset.Mortgage = asset.Price - number;
                    user.Stage = Stage.BuyPropertyCashFlow;

                    bot.SetButtons(user.Id, "What is the cash flow?", cashFlows);
                    return;

                case Stage.BuyPropertyCashFlow:
                    var property = user.Person.Assets.Properties.First(a => a.CashFlow == 0);
                    property.CashFlow = number;
                    user.Stage = Stage.BuyPropertyCashFlow;

                    AvailableAssets.Add(property.Title, AssetType.PropertyType);
                    AvailableAssets.Add(property.Price, AssetType.PropertyBuyPrice);
                    AvailableAssets.Add(property.Price-property.Mortgage, AssetType.PropertyFirstPayment);
                    AvailableAssets.Add(property.CashFlow, AssetType.PropertyCashFlow);

                    SetDefaultButtons(bot, user, "Done");
                    return;
            }
        }

        public static void SellProperty(TelegramBotClient bot, long userId)
        {
            var user = new User(userId);
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

                propertyIds.Add("Cancel");
                user.Stage = Stage.SellPropertyTitle;

                bot.SetButtons(user.Id, $"What property do you want to sell?{Environment.NewLine}{propertyList}", propertyIds);
                return;
            }

            SetDefaultButtons(bot, user, "You have no properties.");
        }

        public static void SellProperty(TelegramBotClient bot, long userId, string value)
        {
            var user = new User(userId);
            var properties = user.Person.Assets.Properties;
            var prices = AvailableAssets.Get(AssetType.PropertySellPrice).AsCurrency().Append("Cancel");

            switch (user.Stage)
            {
                case Stage.SellPropertyTitle:
                    var index = value.Replace("#", "").ToInt();

                    if (index < 1 || index > properties.Count)
                    {
                        bot.SendMessage(user.Id, "Invalid property number.");
                        return;
                    }

                    properties[index - 1].Title += "*";
                    user.Stage = Stage.SellPropertyPrice;
                    bot.SetButtons(user.Id, "What is the price?", prices);
                    return;

                case Stage.SellPropertyPrice:
                    var price = value.AsCurrency();
                    var property = properties.First(x => x.Title.EndsWith("*"));

                    user.Person.Cash += price - property.Mortgage;
                    property.Delete();

                    AvailableAssets.Add(price, AssetType.PropertySellPrice);

                    SetDefaultButtons(bot, user, "Done.");
                    return;
            }
        }

        public static void BuyStocks(TelegramBotClient bot, long userId)
        {
            var user = new User(userId);
            var stocks = AvailableAssets.Get(AssetType.Stock);

            if (user.Person.Cash == 0)
            {
                SetDefaultButtons(bot, user, "You have no money to buy stocks.");
                return;
            }

            stocks.Add("Cancel");
            user.Stage = Stage.BuyStocksTitle;
            bot.SetButtons(user.Id, "Title:", stocks);
        }

        public static void BuyStocks(TelegramBotClient bot, long userId, string value)
        {
            var user = new User(userId);
            var title = value.Trim().ToUpper();
            var number = value.AsCurrency();
            var prices = AvailableAssets.Get(AssetType.StockPrice).AsCurrency().ToList();
            prices.Add("Cancel");

            switch (user.Stage)
            {
                case Stage.BuyStocksTitle:

                    user.Stage = Stage.BuyStocksPrice;
                    user.Person.Assets.Add(title, AssetType.Stock);
                    bot.SetButtons(user.Id, "Price?", prices);
                    return;

                case Stage.BuyStocksPrice:
                    if (number <= 0)
                    {
                        bot.SetButtons(user.Id, "Invalid price value. Try again.", prices);
                        return;
                    }

                    user.Person.Assets.Stocks.First(a => a.Price == 0).Price = number;
                    user.Stage = Stage.BuyStocksQtty;

                    int upToQtty = user.Person.Cash / number;
                    bot.SetButtons(user.Id,
                    $"You can buy up to {upToQtty} stocks. How much stocks would you like to buy?", upToQtty.ToString(),
                    "Cancel");
                    return;

                case Stage.BuyStocksQtty:
                    if (number <= 0)
                    {
                        bot.SendMessage(user.Id, "Invalid quantity value. Try again.");
                        return;
                    }

                    var asset = user.Person.Assets.Stocks.First(a => a.Qtty == 0);

                    var totalPrice = asset.Price * number;
                    var availableCash = user.Person.Cash;
                    int availableQtty = availableCash / asset.Price;

                    if (totalPrice > availableCash)
                    {
                        bot.SendMessage(user.Id,
                        $"{number} x {asset.Price.AsCurrency()} = {totalPrice}. You have only {availableCash.AsCurrency()}." +
                        $"You can buy {availableQtty} stocks. So, what quantity of stocks do you want to buy?");
                        return;
                    }

                    asset.Qtty = number;
                    user.Person.Cash -= totalPrice;

                    AvailableAssets.Add(asset.Title, AssetType.Stock);
                    AvailableAssets.Add(asset.Price, AssetType.StockPrice);

                    SetDefaultButtons(bot, user, "Done.");
                    return;
            }
        }

        public static void SellStocks(TelegramBotClient bot, long userId)
        {
            var user = new User(userId);
            var stocks = user.Person.Assets.Stocks
                .Select(x => x.Title)
                .Distinct()
                .Append("Cancel")
                .ToList();

            if (stocks.Count > 1)
            {
                user.Stage = Stage.SellStocksTitle;
                bot.SetButtons(user.Id, "What stocks do you want to sell?", stocks);
            }
            else
            {
                SetDefaultButtons(bot, user, "You have no stocks.");
            }
        }

        public static void SellStocks(TelegramBotClient bot, long userId, string value)
        {
            var user = new User(userId);
            var title = value.Trim().ToUpper();
            var number = value.AsCurrency();
            var stocks = user.Person.Assets.Stocks;
            var prices = AvailableAssets.Get(AssetType.StockPrice).AsCurrency().ToList();
            prices.Add("Cancel");

            switch (user.Stage)
            {
                case Stage.SellStocksTitle:
                    var assets = stocks.Where(x => x.Title == title).ToList();

                    if (!assets.Any())
                    {
                        SetDefaultButtons(bot, user, "Invalid stocks name.");
                        return;
                    }

                    user.Stage = Stage.SellStocksPrice;
                    assets.ForEach(x => x.Title += "*");
                    bot.SetButtons(user.Id, "Price:", prices);
                    return;

                case Stage.SellStocksPrice:
                    var stocksToSell = stocks.Where(x => x.Title.EndsWith("*")).ToList();
                    var qtty = stocksToSell.Sum(x => x.Qtty);

                    if (number <= 0)
                    {
                        bot.SetButtons(user.Id, "Invalid price value. Try again.", prices);
                        return;
                    }

                    user.Person.Cash += qtty * number;
                    stocksToSell.ForEach(x => x.Delete());

                    AvailableAssets.Add(number, AssetType.StockPrice);

                    SetDefaultButtons(bot, user, "Done.");
                    return;
            }
        }

        public static void ReduceLiabilities(TelegramBotClient bot, long userId, Stage stage)
        {
            var user = new User(userId);

            if (user.Person.Cash < 1000)
            {
                Cancel(bot, user.Id);
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
                .Select(x => x.ToString())
                .ToList();
            buttons.Add("Cancel");

            bot.SetButtons(user.Id, "How much would you pay?", buttons);
        }

        public static void ReduceLiabilities(TelegramBotClient bot, long userId)
        {
            var user = new User(userId);
            var l = user.Person.Liabilities;
            var x = user.Person.Expenses;
            var buttons = new List<string>();
            var liabilities = string.Empty;

            if (l.Mortgage > 0)
            {
                buttons.Add("Mortgage");
                liabilities += $"*Mortgage:* {l.Mortgage.AsCurrency()} - {x.Mortgage.AsCurrency()} monthly{Environment.NewLine}";
            }

            if (l.SchoolLoan > 0)
            {
                buttons.Add("School Loan");
                liabilities += $"*School Loan:* {l.SchoolLoan.AsCurrency()} - {x.SchoolLoan.AsCurrency()} monthly{Environment.NewLine}";
            }

            if (l.CarLoan > 0)
            {
                buttons.Add("Car Loan");
                liabilities += $"*Car Loan:* {l.CarLoan.AsCurrency()} - {x.CarLoan.AsCurrency()} monthly{Environment.NewLine}";
            }

            if (l.CreditCard > 0)
            {
                buttons.Add("Credit Card");
                liabilities += $"*Credit Card:* {l.CreditCard.AsCurrency()} - {x.CreditCard.AsCurrency()} monthly{Environment.NewLine}";
            }

            if (l.BankLoan > 0)
            {
                buttons.Add("Bank Loan");
                liabilities += $"*Bank Loan:* {l.BankLoan.AsCurrency()} - {x.BankLoan.AsCurrency()} monthly{Environment.NewLine}";
            }

            if (user.Person.Cash < 1000)
            {
                bot.SendMessage(user.Id, liabilities);
                SetDefaultButtons(bot, user, $"You don't have money to reduce liabilities, your balance is {user.Person.Cash.AsCurrency()}");
                return;
            }

            if (buttons.Any())
            {
                buttons.Add("Cancel");
                bot.SetButtons(user.Id, liabilities, buttons);
                return;
            }

            Cancel(bot, user.Id);
        }

        public static void ShowData(TelegramBotClient bot, long userId)
        {
            var user = new User(userId);

            SetDefaultButtons(bot, user, user.Description);
        }

        public static void GetCredit(TelegramBotClient bot, long userId)
        {
            var user = new User(userId);

            user.Stage = Stage.GetCredit;
            bot.SetButtons(user.Id, "How much?", "1000", "2000", "5000", "10 000", "20 000", "Cancel");
        }


        public static void GetMoney(TelegramBotClient bot, long userId, string value)
        {
            var amount = value.AsCurrency();
            var user = new User(userId);
            user.Person.Cash += amount;

            SetDefaultButtons(bot, user, $"Ok, you've got {amount.AsCurrency()}");
        }

        public static void GiveMoney(TelegramBotClient bot, long userId, string value)
        {
            var amount = value.AsCurrency();
            var user = new User(userId);

            if (user.Person.Cash < amount)
            {
                SetDefaultButtons(bot, user, $"You don't have {amount.AsCurrency()}, but only {user.Person.Cash.AsCurrency()}");
                return;
            }

            user.Person.Cash -= amount;

            AvailableAssets.Add(amount, AssetType.GiveMoney);
            SetDefaultButtons(bot, user, user.Description);
        }

        public static void GetCredit(TelegramBotClient bot, long userId, string value)
        {
            var user = new User(userId);
            var amount = value.AsCurrency();

            if (amount % 1000 > 0 || amount < 1000)
            {
                bot.SendMessage(user.Id, "Invalid amount. The amount must be a multiple of 1000");
                return;
            }

            user.Person.Cash += amount;
            user.Person.Expenses.BankLoan += amount / 10;
            user.Person.Liabilities.BankLoan += amount;

            SetDefaultButtons(bot, user, $"Ok, you've got {amount.AsCurrency()}");
        }

        public static void PayCredit(TelegramBotClient bot, long userId, string value, Stage stage)
        {
            var user = new User(userId);
            var amount = value.ToInt();
            int expenses;

            if (amount % 1000 > 0 || amount < 1000)
            {
                bot.SendMessage(user.Id, "Invalid amount. The amount must be a multiple of 1000");
                return;
            }

            if (amount > user.Person.Cash)
            {
                bot.SendMessage(user.Id, $"Cannot pay {amount.AsCurrency()}, you have {user.Person.Cash.AsCurrency()} only.");
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

            Cancel(bot, user.Id);
        }

        public static void Cancel(TelegramBotClient bot, long userId)
        {
            var user = new User(userId);

            SetDefaultButtons(bot, user, user.Person.Description);
        }

        public static void Confirm(TelegramBotClient bot, long userId)
        {
            var user = new User(userId);

            switch (user.Stage)
            {
                case Stage.GetChild:
                    user.Person.Expenses.Children++;
                    SetDefaultButtons(bot, user, $"{user.Person.Profession}, you have {user.Person.Expenses.ChildrenExpenses.AsCurrency()} children expenses.");
                    return;

                case Stage.StopGame:
                    user.Person.Clear();
                    user.Person.Expenses.Clear();
                    user.Stage = Stage.Nothing;

                    Start(bot, user.Id);
                    return;

                case Stage.BringDown:
                    Environment.Exit(0);
                    return;
            }

            Cancel(bot, user.Id);
        }

        public static void Ask(TelegramBotClient bot, long userId, Stage stage, string question, params string[] buttons)
        {
            var user = new User(userId);
            user.Stage = stage;

            bot.SetButtons(user.Id, question, buttons.Append("Cancel"));
        }

        public static void Start(TelegramBotClient bot, long userId)
        {
            var user = new User(userId);
            var professions = Persons.Items.Select(x => x.Profession).ToArray();

            if (!user.Exists) { user.Create(); }

            if (user.Person.Exists)
            {
                bot.SetButtons(user.Id, "Please stop current game before starting a new one.", "Stop game", "Cancel");
                return;
            }

            user.Stage = Stage.GetProfession;
            bot.SetButtons(user.Id, "Choose your *profession*", professions);
        }

        public static void SetProfession(TelegramBotClient bot, long userId, string profession)
        {
            var professions = Persons.Items.Select(x => x.Profession.ToLower());
            var user = new User(userId);

            if (!professions.Contains(profession))
            {
                bot.SendMessage(user.Id, "Profession not found. Try again.");
                return;
            }

            user.Person.Create(profession);

            SetDefaultButtons(bot, user, $"Welcome, {user.Person.Profession}!");
        }

        private static async void SetDefaultButtons(TelegramBotClient bot, User user, string message)
        {
            var rkm = new ReplyKeyboardMarkup
            {
                Keyboard = new List<IEnumerable<KeyboardButton>>
                {
                    new List<KeyboardButton>{"Show my Data"},
                    new List<KeyboardButton>{"Get Money", "Give Money", "Get Credit"},
                    new List<KeyboardButton>{"Buy Stocks", "Sell Stocks"},
                    new List<KeyboardButton>{"Buy Property", "Sell Property"},
                    new List<KeyboardButton>{"Add Child", "Reduce Liabilities"},
                    new List<KeyboardButton>{"Stop Game"}
                }
            };

            user.Person.Assets.CleanUp();
            user.Stage = Stage.Nothing;

            await bot.SendTextMessageAsync(user.Id, message, replyMarkup: rkm, parseMode: ParseMode.Markdown);
        }
    }
}

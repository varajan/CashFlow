using System;
using System.Linq;
using CashFlowBot.Data;
using CashFlowBot.Extensions;
using CashFlowBot.Models;
using Telegram.Bot;

namespace CashFlowBot
{
    public static class Actions
    {
        public static void Buy(TelegramBotClient bot, long userId)
        {
            var user = new User(userId);
            user.Stage = Stage.Buy;

            bot.SetButtons(user.Id, "What to buy?", "Stocks", "Business", "Apartment", "Other");
        }

        public static void Sell(TelegramBotClient bot, long userId)
        {
            var user = new User(userId);
            user.Stage = Stage.Sell;

            bot.SetButtons(user.Id, "What to sell?", "Stocks", "Business", "Apartment", "Other");
        }

        public static void BuyStocks(TelegramBotClient bot, long userId)
        {
            var user = new User(userId);
            var stocks = AvailableAssets.Get(AssetType.Stock);

            stocks.Add("Cancel");
            user.Stage = Stage.BuyStocksTitle;
            bot.SetButtons(user.Id, "Title:", stocks);
        }

        public static void BuyStocks(TelegramBotClient bot, long userId, string value)
        {
            var user = new User(userId);
            var title = value.Trim().ToUpper();
            var number = value.ToInt();
            var prices = AvailableAssets.Get(AssetType.StockPrice).ToList();
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

                    user.Person.Assets.Items.First(a => a.Price == 0).Price = number;
                    user.Stage = Stage.BuyStocksQtty;

                    int upToQtty = user.Person.Cash / number;
                    bot.SetButtons(user.Id, $"You can buy up to {upToQtty} stocks. How much stocks would you like to buy?", upToQtty.ToString(), "Cancel");
                    return;

                case Stage.BuyStocksQtty:
                    if (number <= 0)
                    {
                        bot.SendMessage(user.Id, "Invalid quantity value. Try again.");
                        return;
                    }

                    var asset = user.Person.Assets.Items.First(a => a.Qtty == 0);

                    var totalPrice = asset.Price * number;
                    var availableCash = user.Person.Cash;
                    int availableQtty = availableCash / asset.Price;

                    if (totalPrice > availableCash)
                    {
                        bot.SendMessage(user.Id, $"{number} x ${asset.Price} = {totalPrice}. You have only ${availableCash}." +
                                                 $"You can buy {availableQtty} stocks. So, what quantity of stocks do you want to buy?");
                        return;
                    }

                    asset.Qtty = number;
                    user.Person.Cash -= totalPrice;
                    user.Stage = Stage.Nothing;

                    AvailableAssets.Add(asset.Title, AssetType.Stock);
                    AvailableAssets.Add(asset.Price, AssetType.StockPrice);

                    SetDefaultButtons(bot, user, "Done.");
                    return;
            }
        }

        public static void SellStocks(TelegramBotClient bot, long userId)
        {
            var user = new User(userId);
            var stocks = user.Person.Assets.Items
                .Where(x => x.Type == AssetType.Stock)
                .Select(x => x.Title)
                .Distinct()
                .ToList();

            stocks.Add("Cancel");

            if (stocks.Count > 1)
            {
                user.Stage = Stage.SellStocksTitle;
                bot.SetButtons(user.Id, "What stocks do you want to sell?", stocks);
            }
            else
            {
                user.Stage = Stage.Nothing;
                SetDefaultButtons(bot, user, "You have no stocks.");
            }
        }

        public static void SellStocks(TelegramBotClient bot, long userId, string value)
        {
            var user = new User(userId);
            var title = value.Trim().ToUpper();
            var number = value.ToInt();
            var stocks = user.Person.Assets.Items.Where(x => x.Type == AssetType.Stock).ToList();
            var prices = AvailableAssets.Get(AssetType.StockPrice).ToList();
            prices.Add("Cancel");

            switch (user.Stage)
            {
                case Stage.SellStocksTitle:
                    var assets = stocks.Where(x => x.Title == title).ToList();

                    if (!assets.Any())
                    {
                        user.Stage = Stage.Nothing;
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
                    user.Stage = Stage.Nothing;
                    stocksToSell.ForEach(x => x.Delete());

                    AvailableAssets.Add(number, AssetType.StockPrice);

                    SetDefaultButtons(bot, user, "Done.");
                    return;
            }
        }

        public static void GetMoney(TelegramBotClient bot, long userId)
        {
            var user = new User(userId);

            user.Person.Cash += user.Person.CashFlow;
            bot.SendMessage(user.Id, $"{user.Person.Profession}, you have ${user.Person.Cash}");
        }

        public static void ShowData(TelegramBotClient bot, long userId)
        {
            var user = new User(userId);

            bot.SendMessage(user.Id, user.Description);
        }

        public static void GetCredit(TelegramBotClient bot, long userId)
        {
            var user = new User(userId);

            user.Stage = Stage.GetCredit;
            bot.SendMessage(user.Id, "How much?");
        }

        public static void GetCredit(TelegramBotClient bot, long userId, string value)
        {
            var user = new User(userId);
            var amount = value.ToInt();

            if (amount % 1000 > 0 || amount < 1000)
            {
                bot.SendMessage(user.Id, "Invalid amount. The amount must be a multiple of 1000");
                return;
            }

            user.Person.Cash += amount;
            user.Person.Expenses.BankLoan += amount / 10;
            user.Person.Liabilities.BankLoan += amount;
            user.Stage = Stage.Nothing;
            bot.SendMessage(user.Id, $"Ok, you've got ${amount}");
        }

        public static void PayCredit(TelegramBotClient bot, long userId)
        {
            var user = new User(userId);

            if (user.Person.Expenses.BankLoan == 0)
            {
                bot.SendMessage(user.Id, "You have no credit.");
                return;
            }

            user.Stage = Stage.PayCredit;
            bot.SendMessage(user.Id, "How much?");
        }

        public static void PayCredit(TelegramBotClient bot, long userId, string value)
        {
            var user = new User(userId);
            var amount = value.ToInt();

            if (amount % 1000 > 0 || amount < 1000)
            {
                bot.SendMessage(user.Id, "Invalid amount. The amount must be a multiple of 1000");
                return;
            }

            amount = Math.Min(amount, user.Person.Liabilities.BankLoan);

            if (amount > user.Person.Cash)
            {
                bot.SendMessage(user.Id, $"Cannot pay ${amount}, you have ${user.Person.Cash} only.");
                return;
            }

            user.Person.Cash -= amount;
            user.Person.Expenses.BankLoan -= amount / 10;
            user.Person.Liabilities.BankLoan -= amount;
            user.Stage = Stage.Nothing;
            bot.SendMessage(user.Id, $"Ok, you've payed ${amount}");
        }

        public static void Cancel(TelegramBotClient bot, long userId)
        {
            var user = new User(userId);
            user.Stage = Stage.Nothing;

            SetDefaultButtons(bot, user, user.Person.Description);
        }

        public static void Clear(TelegramBotClient bot, long userId)
        {
            var user = new User(userId);
            user.Person.Clear();
            user.Person.Expenses.Clear();
            user.Stage = Stage.Nothing;

            Start(bot, user.Id);
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

            user.Stage = Stage.Nothing;
            user.Person.Create(profession);

            SetDefaultButtons(bot, user, $"Welcome, {user.Person.Profession}!");
        }

        private static void SetDefaultButtons(TelegramBotClient bot, User user, string message)
        {
            // TODO: reset asset titles, remove empty, etc.

            bot.SetButtons(user.Id, message, "Show my Data", "Buy", "Sell", "Get money", "Pay Credit", "Get Credit");
        }
    }
}

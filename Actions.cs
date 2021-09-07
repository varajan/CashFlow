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
            user.Stage = Stages.Buy;

            bot.SetButtons(user.Id, "What to buy?", "Stocks", "Business", "Apartment", "Other");
        }

        public static void BuyStocks(TelegramBotClient bot, long userId)
        {
            var user = new User(userId);
            user.Stage = Stages.BuyStocksTitle;

            bot.SendMessage(user.Id, "Title:");
        }

        public static void BuyStocks(TelegramBotClient bot, long userId, string value)
        {
            var user = new User(userId);

            var number = value.ToInt();

            switch (user.Stage)
            {
                case Stages.BuyStocksTitle:
                    user.Stage = Stages.BuyStocksPrice;
                    user.Person.Assets.Add(value.Trim().ToUpper());
                    bot.SendMessage(user.Id, "Price:");
                    return;

                case Stages.BuyStocksPrice:
                    if (number <= 0)
                    {
                        bot.SendMessage(user.Id, "Invalid value. Try again.");
                        return;
                    }

                    user.Person.Assets.Items.First(a => a.Price == 0).Price = number;

                    user.Stage = Stages.BuyStocksQtty;
                    bot.SendMessage(user.Id, "Quantity:");
                    return;

                case Stages.BuyStocksQtty:
                    if (number <= 0)
                    {
                        bot.SendMessage(user.Id, "Invalid value. Try again.");
                        return;
                    }

                    var asset = user.Person.Assets.Items.First(a => a.Qtty == 0);

                    var totalPrice = asset.Price * number;
                    var availableCash = user.Person.Cash;
                    int availableQtty = user.Person.Cash / asset.Price;

                    if (totalPrice > availableCash)
                    {
                        bot.SendMessage(user.Id, $"{number} x ${asset.Price} = {totalPrice}. You have only ${availableCash}." +
                                                 $"You can buy {availableQtty} stocks. So, what quantity of stocks do you want to buy?");
                        return;
                    }

                    asset.Qtty = number;
                    user.Person.Cash -= totalPrice;
                    user.Stage = Stages.Nothing;
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

            user.Stage = Stages.GetCredit;
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
            user.Stage = Stages.Nothing;
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

            user.Stage = Stages.PayCredit;
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
            user.Stage = Stages.Nothing;
            bot.SendMessage(user.Id, $"Ok, you've payed ${amount}");
        }

        public static void Cancel(TelegramBotClient bot, long userId)
        {
            var user = new User(userId);
            user.Stage = Stages.Nothing;

            SetDefaultButtons(bot, user, "Canceled");
        }

        public static void Clear(TelegramBotClient bot, long userId)
        {
            var user = new User(userId);
            user.Person.Clear();
            user.Person.Expenses.Clear();
            user.Stage = Stages.Nothing;

            bot.SendMessage(userId, "Done");
        }

        public static void Start(TelegramBotClient bot, long userId)
        {
            var user = new User(userId);
            var professions = string.Join(Environment.NewLine, Persons.Items.Select(x => x.Profession));

            if (!user.Exists) { user.Create(); }

            if (user.Person.Exists)
            {
                bot.SendMessage(user.Id, "Please stop current game before starting a new one.");
                return;
            }

            user.Stage = Stages.GetProfession;
            bot.SendMessage(user.Id, $"Choose your *profession*:{Environment.NewLine}{professions}");
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

            user.Stage = Stages.Nothing;
            user.Person.Create(profession);

            SetDefaultButtons(bot, user, $"Welcome, {user.Person.Profession}!");
        }

        private static void SetDefaultButtons(TelegramBotClient bot, User user, string message)
        {
            bot.SetButtons(user.Id, message, "Show my Data", "Buy", "Sell", "Get money", "Pay Credit", "Get Credit");
        }
    }
}

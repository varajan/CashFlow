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
            bot.SendMessage(user.Id, "Ok");
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

            bot.SetButtons(user.Id, $"Welcome, {user.Person.Profession}!","Show my Data", "Buy", "Sell", "Get money");
        }
    }
}

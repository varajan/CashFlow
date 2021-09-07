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

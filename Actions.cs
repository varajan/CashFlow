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
        public static void Clear(TelegramBotClient bot, long userId)
        {
            var user = new User(userId);
            user.DeletePersons();
            user.DeleteExpenses();
            user.Stage = Stages.Nothing;

            bot.SendMessage(userId, "Done");
        }

        public static void Start(TelegramBotClient bot, long userId)
        {
            var user = new User(userId);
            var professions = string.Join(Environment.NewLine, Data.Persons.Items.Select(x => x.Profession));

            if (user.HasPerson)
            {
                bot.SendMessage(user.Id, "Please stop current game before starting a new one.");
                return;
            }

            user.Stage = Stages.GetProfession;
            bot.SendMessage(user.Id, $"Choose your *profession*:{Environment.NewLine}{professions}");
        }
    }
}

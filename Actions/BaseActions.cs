﻿using System.Collections.Generic;
using System.Linq;
using CashFlowBot.Data;
using CashFlowBot.DataBase;
using CashFlowBot.Extensions;
using CashFlowBot.Models;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CashFlowBot.Actions
{
    public class BaseActions
    {
        public static void Ask(TelegramBotClient bot, User user, Stage stage, string question, params string[] buttons)
        {
            user.Stage = stage;
            bot.SetButtons(user.Id, question, buttons.Append(Terms.Get(6, user, "Cancel")));
        }

        public static void ChangeLanguage(TelegramBotClient bot, User user) =>
            Ask(bot, user, Stage.Nothing, "Language/Мова", "UA", "EN", "DE"); // todo -- get from enum

        public static async void Start(TelegramBotClient bot, User user, string name = null)
        {
            if (!user.Exists)
            {
                user.Create();
                user.Name = name ?? "N/A";
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

            var professions = Persons.Get(user.Id).Select(x => x.Profession).OrderBy(x => x).ToList();
            var rkm = new ReplyKeyboardMarkup { Keyboard = new List<IEnumerable<KeyboardButton>>() };

            while (professions.Any())
            {
                var x = professions.Take(3).ToList();
                professions = professions.Skip(3).ToList();

                if (x.Count == 3) { rkm.Keyboard = rkm.Keyboard.Append(new KeyboardButton[] { x[0], x[1], x[2] }); continue; }
                if (x.Count == 2) { rkm.Keyboard = rkm.Keyboard.Append(new KeyboardButton[] { x[0], x[1] }); continue; }
                if (x.Count == 1) { rkm.Keyboard = rkm.Keyboard.Append(new KeyboardButton[] { x[0] }); }
            }

            user.Stage = Stage.GetProfession;
            await bot.SendTextMessageAsync(user.Id, Terms.Get(28, user, "Choose your *profession*"),
                replyMarkup: rkm, parseMode: ParseMode.Markdown);
        }

        public static void SetProfession(TelegramBotClient bot, User user, string profession)
        {
            var professions = Persons.Get(user.Id).Select(x => x.Profession.ToLower());

            if (!professions.Contains(profession))
            {
                bot.SendMessage(user.Id, Terms.Get(29, user, "Profession not found. Try again."));
                return;
            }

            user.History.Clear();
            user.Person.Create(profession);

            SmallCircleButtons(bot, user, Terms.Get(30, user, "Welcome, {0}!", user.Person.Profession));
        }

        public static void Cancel(TelegramBotClient bot, User user)
        {
            if (user.Person.BigCircle)
            {
                BigCircleButtons(bot, user);
                return;
            }

            user.Person.Assets.CleanUp();
            SmallCircleButtons(bot, user, user.Person.Description);
        }

        private static async void BigCircleButtons(TelegramBotClient bot, User user)
        {
            if (user.Person.CurrentCashFlow >= user.Person.TargetCashFlow)
            {
                bot.SendMessage(user.Id, user.Person.Description);
                bot.SetButtons(user.Id, Terms.Get(73, user, "You are the winner!"), Terms.Get(41, user, "Stop Game"));
                return;
            }

            var rkm = new ReplyKeyboardMarkup
            {
                Keyboard = new List<IEnumerable<KeyboardButton>>
                {
                    new List<KeyboardButton>{Terms.Get(32, user, "Get Money"), Terms.Get(33, user, "Give Money")},
                    new List<KeyboardButton>{Terms.Get(69, user, "Divorce"), Terms.Get(70, user, "Tax Audit"), Terms.Get(71, user, "Lawsuit")},
                    new List<KeyboardButton>{Terms.Get(74, user, "Buy Business")},
                    new List<KeyboardButton>{Terms.Get(41, user, "Stop Game")}
                }
            };

            user.Person.Assets.CleanUp();
            user.Stage = Stage.Nothing;

            await bot.SendTextMessageAsync(user.Id, user.Person.Description, replyMarkup: rkm, parseMode: ParseMode.Markdown);
        }

        public static async void SmallCircleButtons(TelegramBotClient bot, User user, string message)
        {
            user.Person.ReadyForBigCircle = user.Person.Assets.Income > user.Person.Expenses.Total;

            if (user.Person.ReadyForBigCircle)
            {
                bot.SendMessage(user.Id, Terms.Get(68, user, "Your income is greater, then expenses. You are ready for Big Circle."));
            }

            var rkm = new ReplyKeyboardMarkup
            {
                Keyboard = new List<IEnumerable<KeyboardButton>>
                {
                    user.History.IsEmpty
                    ? new List<KeyboardButton>{Terms.Get(31, user, "Show my Data")}
                    : new List<KeyboardButton>{Terms.Get(31, user, "Show my Data"), Terms.Get(2, user, "History")},
                    new List<KeyboardButton>{Terms.Get(81, user, "Small Opportunity"), Terms.Get(84, user, "Big Opportunity") },
                    new List<KeyboardButton>{Terms.Get(86, user, "Doodads"), Terms.Get(85, user, "Market")},
                    new List<KeyboardButton>{ Terms.Get(80, user, "Downsize"), Terms.Get(39, user, "Baby")},
                    new List<KeyboardButton>{Terms.Get(79, user, "Pay Check")}
                }
            };

            user.Person.Assets.CleanUp();
            user.Stage = Stage.Nothing;

            if (user.Person.ReadyForBigCircle)
            {
                rkm.Keyboard = rkm.Keyboard.Append(new List<KeyboardButton> { Terms.Get(1, user, "Go to Big Circle") });
            }

            await bot.SendTextMessageAsync(user.Id, message, replyMarkup: rkm, parseMode: ParseMode.Markdown);
        }
    }
}
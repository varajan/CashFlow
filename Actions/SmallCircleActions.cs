﻿using CashFlowBot.Data;
using CashFlowBot.DataBase;
using CashFlowBot.Extensions;
using CashFlowBot.Models;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramUser = Telegram.Bot.Types.User;
using Terms = CashFlowBot.DataBase.Terms;

namespace CashFlowBot.Actions;

public class SmallCircleActions : BaseActions
{
    public static void Downsize(TelegramBotClient bot, User user)
    {
        var expenses = user.Person.Expenses.Total;

        bot.SendMessage(user.Id, Terms.Get(87, user, "You were fired. You've payed total amount of your expenses: {0} and lose 2 turns.", expenses.AsCurrency()));

        if (user.Person.Cash < expenses)
        {
            var delta = expenses - user.Person.Cash;
            var credit = (int)Math.Ceiling(delta / 1_000d) * 1_000;

            user.GetCredit(credit);
            bot.SendMessage(user.Id, Terms.Get(88, user, "You've taken {0} from bank.", credit.AsCurrency()));
        }

        user.Person.Cash -= expenses;
        user.History.Add(ActionType.Downsize, expenses);
        Cancel(bot, user);
    }

    public static void IncreaseCashFlow(TelegramBotClient bot, User user)
    {
        if (!user.Person.Assets.SmallBusinesses.Any())
        {
            bot.SendMessage(user.Id, Terms.Get(136, user, "You have no small Business."));
            Cancel(bot, user);
            return;
        }

        var question = Terms.Get(12, user, "What is the cash flow?");
        var amounts = AvailableAssets.GetAsCurrency(AssetType.IncreaseCashFlow);

        Ask(bot, user, Stage.IncreaseCashFlow, question, amounts);
    }

    public static void IncreaseCashFlow(TelegramBotClient bot, User user, int amount)
    {
        foreach (var business in user.Person.Assets.SmallBusinesses)
        {
            business.CashFlow += amount;
            user.History.Add(ActionType.IncreaseCashFlow, amount);
        }

        bot.SendMessage(user.Id, Terms.Get(13, user, "Done."));
        Cancel(bot, user);
    }

    public static async void SmallOpportunity(TelegramBotClient bot, User user)
    {
        var rkm = new ReplyKeyboardMarkup
        {
            Keyboard = new List<IEnumerable<KeyboardButton>>
            {
                new List<KeyboardButton>
                {
                    Terms.Get(35, user, "Buy Stocks"),
                    Terms.Get(36, user, "Sell Stocks"),
                    Terms.Get(82, user, "Stocks x2"),
                    Terms.Get(83, user, "Stocks ÷2")
                },
                new List<KeyboardButton>{ Terms.Get(37, user, "Buy Real Estate"), Terms.Get(94, user, "Buy Land") },
                new List<KeyboardButton>{ Terms.Get(119, user, "Buy coins"), Terms.Get(115, user, "Start a company") },
                new List<KeyboardButton>{ Terms.Get(6, user, "Cancel") }
            }
        };

        user.Person.Assets.CleanUp();
        user.Person.SmallRealEstate = true;
        user.Stage = Stage.Nothing;

        await bot.SendTextMessageAsync(user.Id, Terms.Get(89, user, "What do you want?"), replyMarkup: rkm, parseMode: ParseMode.Markdown);
    }

    public static async void BigOpportunity(TelegramBotClient bot, User user)
    {
        var rkm = new ReplyKeyboardMarkup
        {
            Keyboard = new List<IEnumerable<KeyboardButton>>
            {
                new List<KeyboardButton>{Terms.Get(37, user, "Buy Real Estate")},
                new List<KeyboardButton>{Terms.Get(74, user, "Buy Business")},
                new List<KeyboardButton>{Terms.Get(94, user, "Buy Land")},
                new List<KeyboardButton>{ Terms.Get(6, user, "Cancel") }
            }
        };

        user.Person.Assets.CleanUp();
        user.Person.SmallRealEstate = false;
        user.Stage = Stage.Nothing;

        await bot.SendTextMessageAsync(user.Id, Terms.Get(89, user, "What do you want?"), replyMarkup: rkm, parseMode: ParseMode.Markdown);
    }

    public static async void Doodads(TelegramBotClient bot, User user)
    {
        var rkm = new ReplyKeyboardMarkup
        {
            Keyboard = new List<IEnumerable<KeyboardButton>>
            {
                new List<KeyboardButton>{Terms.Get(95, user, "Pay with Cash")},
                new List<KeyboardButton>{Terms.Get(96, user, "Pay with Credit Card")},
                new List<KeyboardButton>{Terms.Get(112, user, "Buy a boat")},
                new List<KeyboardButton>{ Terms.Get(6, user, "Cancel") }
            }
        };

        user.Person.Assets.CleanUp();
        user.Stage = Stage.Nothing;

        await bot.SendTextMessageAsync(user.Id, Terms.Get(89, user, "What do you want?"), replyMarkup: rkm, parseMode: ParseMode.Markdown);
    }

    public static async void Market(TelegramBotClient bot, User user)
    {
        var rkm = new ReplyKeyboardMarkup
        {
            Keyboard = new List<IEnumerable<KeyboardButton>>
                {
                    new List<KeyboardButton> { Terms.Get(38, user, "Sell Real Estate"), Terms.Get(98, user, "Sell Land"), Terms.Get(75, user, "Sell Business") },
                    new List<KeyboardButton> { Terms.Get(120, user, "Sell Coins"), Terms.Get(118, user, "Increase cash flow") },
                    new List<KeyboardButton> { Terms.Get(6, user, "Cancel") }
                }
        };

        user.Person.Assets.CleanUp();
        user.Stage = Stage.Nothing;

        await bot.SendTextMessageAsync(user.Id, Terms.Get(89, user, "What do you want?"), replyMarkup: rkm, parseMode: ParseMode.Markdown);
    }

    public static void MultiplyStocks(TelegramBotClient bot, User user)
    {
        var stocks = user.Person.Assets.Stocks
            .Select(x => x.Title)
            .Distinct()
            .Append(Terms.Get(6, user, "Cancel"))
            .ToList();

        if (stocks.Count > 1)
        {
            bot.SetButtons(user.Id, Terms.Get(7, user, "Title:"), stocks);
        }
        else
        {
            SmallCircleButtons(bot, user, Terms.Get(49, user, "You have no stocks."));
        }
    }

    public static void MultiplyStocks(TelegramBotClient bot, User user, string title)
    {
        title = title.Trim().ToUpper();

        var k = user.Stage == Stage.Stocks1to2 ? 2 : 0.5;
        var action = user.Stage == Stage.Stocks1to2 ? ActionType.Stocks1To2 : ActionType.Stocks2To1;
        var stocks = user.Person.Assets.Stocks;
        var assets = stocks.Where(x => x.Title == title).ToList();

        if (!assets.Any())
        {
            SmallCircleButtons(bot, user, "Invalid stocks name.");
            return;
        }

        assets.Where(x => x.Title == title).ToList()
            .ForEach(x =>
            {
                x.Qtty = (int) (x.Qtty * k);
                user.History.Add(action, x.Id);
            });

        Cancel(bot, user);
    }

    public static void History(TelegramBotClient bot, User user)
    {
        var cancel = Terms.Get(102, user, "Main menu");
        var rollBack = Terms.Get(109, user, "Rollback last action");

        if (user.History.IsEmpty)
        {
            bot.SetButtons(user.Id, user.History.Description, cancel);
        }
        else
        {
            bot.SetButtons(user.Id, user.History.Description, rollBack, cancel);
        }
    }

    public static async void MyData(TelegramBotClient bot, User user)
    {
        var rkm = new ReplyKeyboardMarkup
        {
            Keyboard = new List<IEnumerable<KeyboardButton>>
            {
                new List<KeyboardButton> { Terms.Get(32, user, "Get Money"), Terms.Get(34, user, "Get Credit") },
                new List<KeyboardButton> { Terms.Get(90, user, "Charity - Pay 10%"), Terms.Get(40, user, "Reduce Liabilities") },
                new List<KeyboardButton> { Terms.Get(41, user, "Stop Game") },
                new List<KeyboardButton> { Terms.Get(102, user, "Main menu") }
            }
        };

        user.Person.Assets.CleanUp();
        user.Stage = Stage.Nothing;

        await bot.SendTextMessageAsync(user.Id, user.Description, replyMarkup: rkm, parseMode: ParseMode.Markdown);
    }

    public static void Charity(TelegramBotClient bot, User user)
    {
        var amount = (user.Person.Assets.Income + user.Person.Salary) / 10;

        if (user.Person.Cash <= amount)
        {
            SmallCircleButtons(bot, user, Terms.Get(23, user, "You don't have {0}, but only {1}",
                amount.AsCurrency(), user.Person.Cash.AsCurrency()));
            return;
        }

        user.Person.Cash -= amount;
        user.History.Add(ActionType.Charity, amount);

        SmallCircleButtons(bot, user, Terms.Get(91, user, "You've payed {0}, now you can use two dice in next 3 turns.",
            amount.AsCurrency()));
    }

    public static void GetMoney(TelegramBotClient bot, User user, string value)
    {
        var amount = value.AsCurrency();
        user.Person.Bankruptcy = user.Person.Cash + amount < 0;

        if (user.Person.Bankruptcy)
        {
            user.History.Add(ActionType.Bankruptcy);
            BankruptcyActions.ShowMenu(bot, user);
            return;
        }

        user.Person.Cash += amount;
        user.History.Add(ActionType.GetMoney, amount);

        bot.SendMessage(user.Id, Terms.Get(22, user, "Ok, you've got *{0}*", amount.AsCurrency()));
        Cancel(bot, user);
    }

    public static void GiveMoney(TelegramBotClient bot, User user, string value)
    {
        var amount = value.AsCurrency();

        if (!user.Person.BigCircle && user.Person.Cash < amount)
        {
            var delta = amount - user.Person.Cash;
            var credit = (int)Math.Ceiling(delta / 1_000d) * 1_000;

            user.GetCredit(credit);
            bot.SendMessage(user.Id, Terms.Get(88, user, "You've taken {0} from bank.", credit.AsCurrency()));
        }

        if (user.Person.Cash < amount)
        {
            bot.SendMessage(user.Id, Terms.Get(23, user, "You don''t have {0}, but only {1}", amount.AsCurrency(), user.Person.Cash.AsCurrency()));
            Cancel(bot, user);
            return;
        }

        user.Person.Cash -= amount;
        user.History.Add(ActionType.PayMoney, amount);

        if (user.Person.BigCircle) AvailableAssets.Add(amount, AssetType.BigGiveMoney);

        Cancel(bot, user);
    }

    public static void SendMoney(TelegramBotClient bot, User user)
    {
        var cancel = Terms.Get(6, user, "Cancel");
        var bank = Terms.Get(149, user, "Bank");
        var users = Users.ActiveUsersNames(user, Circle.Small).ToList();

        user.Person.Assets.Transfer?.Delete();
        user.Stage = Stage.TransferMoneyTo;
        bot.SetButtons(user.Id, Terms.Get(147, user, "Whom?"), users.Append(bank).Append(cancel));
    }

    public static void SendMoney(TelegramBotClient bot, User user, string value)
    {
        var bank = Terms.Get(149, user, "Bank");

        switch (user.Stage)
        {
            case Stage.TransferMoneyTo:
                if (value != bank && !Users.ActiveUsersNames(user, Circle.Small).Contains(value))
                {
                    bot.SendMessage(user.Id, Terms.Get(145, user, "Not found."));
                    Cancel(bot, user);
                    return;
                }

                var cancel = Terms.Get(6, user, "Cancel");
                var buttons = Enumerable.Range(1, 8)
                    .Select(x => (500 * x).AsCurrency())
                    .Append(cancel);

                user.Person.Assets.Add(value, AssetType.Transfer);
                user.Stage = Stage.TransferMoneyAmount;
                bot.SetButtons(user.Id, Terms.Get(21, user, "How much?"), buttons);
                return;

            case Stage.TransferMoneyAmount:
                user.Person.Assets.Transfer.Qtty = value.AsCurrency();
                if (user.Person.Cash <= user.Person.Assets.Transfer.Qtty)
                {
                    var message = Terms.Get(23, user, "You don''t have {0}, but only {1}", user.Person.Assets.Transfer.Qtty.AsCurrency(), user.Person.Cash.AsCurrency());
                    bot.SetButtons(user.Id, message, Terms.Get(34, user, "Get Credit"), Terms.Get(6, user, "Cancel"));
                    return;
                }

                Transfer();
                return;

            case Stage.TransferMoneyCredit:
                var delta = user.Person.Assets.Transfer.Qtty - user.Person.Cash;
                var credit = (int)Math.Ceiling(delta / 1_000d) * 1_000;

                user.GetCredit(credit);
                Transfer();
                return;
        }

        void Transfer()
        {
            var to = user.Person.Assets.Transfer.Title;
            var amount = user.Person.Assets.Transfer.Qtty;
            var friend = Users.ActiveUsers(user).FirstOrDefault(x => x.Name == to);
            var message = Terms.Get(146, user, "{0} transferred {2} to {1}.", user.Name, friend?.Name ?? bank, amount.AsCurrency(), Environment.NewLine);

            user.Person.Cash -= amount;
            user.History.Add(ActionType.PayMoney, amount);

            if (friend is not null)
            {
                friend.Person.Cash += amount;
                friend.History.Add(ActionType.GetMoney, amount);
            }

            Users.ActiveUsers(user).Append(user).ForEach(u => bot.SendMessage(u.Id, message));

            user.Person.Assets.Transfer.Delete();
            Cancel(bot, user);
        }
    }

    public static void Confirm(TelegramBotClient bot, User user, TelegramUser from)
    {
        var person = Persons.Get(user.Id, user.Person.Profession);

        switch (user.Stage)
        {
            case Stage.StopGame:
                StopGame(bot, user, from);
                return;

            case Stage.AdminBringDown:
                Environment.Exit(0);
                return;

            case Stage.ReduceMortgage:
                user.Person.Cash -= user.Person.Liabilities.Mortgage;
                user.Person.Expenses.Mortgage = 0;
                user.Person.Liabilities.Mortgage = 0;
                user.History.Add(ActionType.Mortgage, person.Liabilities.Mortgage);
                CreditActions.ReduceLiabilities(bot, user);
                return;

            case Stage.ReduceSchoolLoan:
                user.Person.Cash -= user.Person.Liabilities.SchoolLoan;
                user.Person.Expenses.SchoolLoan = 0;
                user.Person.Liabilities.SchoolLoan = 0;
                user.History.Add(ActionType.SchoolLoan, person.Liabilities.SchoolLoan);
                CreditActions.ReduceLiabilities(bot, user);
                return;

            case Stage.ReduceCarLoan:
                user.Person.Cash -= user.Person.Liabilities.CarLoan;
                user.Person.Expenses.CarLoan = 0;
                user.Person.Liabilities.CarLoan = 0;
                user.History.Add(ActionType.CarLoan, person.Liabilities.CarLoan);
                CreditActions.ReduceLiabilities(bot, user);
                return;

            case Stage.ReduceCreditCard:
                user.Person.Cash -= user.Person.Liabilities.CreditCard;
                user.Person.Expenses.CreditCard = 0;
                user.Person.Liabilities.CreditCard = 0;
                user.History.Add(ActionType.CreditCard, person.Liabilities.CreditCard);
                CreditActions.ReduceLiabilities(bot, user);
                return;

            case Stage.ReduceSmallCredit:
                user.Person.Cash -= user.Person.Liabilities.SmallCredits;
                user.Person.Expenses.SmallCredits = 0;
                user.Person.Liabilities.SmallCredits = 0;
                user.History.Add(ActionType.SmallCredit, person.Liabilities.SmallCredits);
                CreditActions.ReduceLiabilities(bot, user);
                return;

            case Stage.ReduceBoatLoan:
                var boat = user.Person.Assets.Boat;

                user.Person.Cash -= boat.Mortgage;
                user.History.Add(ActionType.PayOffBoat, boat.Mortgage);
                boat.CashFlow = 0;
                CreditActions.ReduceLiabilities(bot, user);
                return;
        }

        Cancel(bot, user);
    }
}
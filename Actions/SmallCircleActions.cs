using System;
using System.Collections.Generic;
using System.Linq;
using CashFlowBot.Data;
using CashFlowBot.Extensions;
using CashFlowBot.Models;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Terms = CashFlowBot.DataBase.Terms;

namespace CashFlowBot.Actions
{
    public class SmallCircleActions : BaseActions
    {
        public static void Downsize(TelegramBotClient bot, User user)
        {
            var expenses = user.Person.Expenses.Total;

            bot.SendMessage(user.Id,
            Terms.Get(87, user, "You were fired. You've payed total amount of your expenses: {0} and lose 2 turns.", expenses.AsCurrency()));

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
                bot.SendMessage(user.Id, Terms.Get(77, user, "You have no Business."));
                Cancel(bot, user);
                return;
            }

            foreach (var business in user.Person.Assets.SmallBusinesses)
            {
                business.CashFlow += 400;
                user.History.Add(ActionType.IncreaseCashFlow, business.Id);
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
                        Terms.Get(82, user, "Stocks 2 to 1"),
                        Terms.Get(83, user, "Stocks 1 to 2")
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
                    x.Qtty = (int)(x.Qtty * k);
                    user.History.Add(action, x.Id);
                });

            Cancel(bot, user);
        }

        public static void History(TelegramBotClient bot, User user)
        {
            var cancel = Terms.Get(6, user, "Cancel");
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
                    new List<KeyboardButton>{Terms.Get(32, user, "Get Money"), Terms.Get(34, user, "Get Credit")},
                    new List<KeyboardButton>{Terms.Get(90, user, "Charity - Pay 10%"), Terms.Get(40, user, "Reduce Liabilities")},
                    new List<KeyboardButton>{Terms.Get(41, user, "Stop Game")},
                    new List<KeyboardButton>{Terms.Get(102, user, "Main menu") }
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
                user.History.Add(ActionType.Bankruptcy, 0);
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

            AvailableAssets.Add(amount, user.Person.BigCircle ? AssetType.BigGiveMoney : AssetType.SmallGiveMoney);
            Cancel(bot, user);
        }

        public static void Confirm(TelegramBotClient bot, User user)
        {
            var person = Persons.Get(user.Id, user.Person.Profession);

            switch (user.Stage)
            {
                case Stage.StopGame:
                    StopGame(bot, user);
                    return;

                case Stage.AdminBringDown:
                    Environment.Exit(0);
                    return;

                case Stage.Rollback:
                    user.History.Rollback();
                    History(bot, user);
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
}
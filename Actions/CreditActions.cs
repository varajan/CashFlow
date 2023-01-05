using System;
using System.Collections.Generic;
using System.Linq;
using CashFlowBot.Data;
using CashFlowBot.Extensions;
using CashFlowBot.Models;
using Telegram.Bot;
using Terms = CashFlowBot.DataBase.Terms;

namespace CashFlowBot.Actions
{
    public class CreditActions : BaseActions
    {
        public static void PayWithCreditCard(TelegramBotClient bot, User user)
        {
            var cancel = Terms.Get(6, user, "Cancel");
            user.Stage = Stage.MicroCreditAmount;
            var monthly = AvailableAssets.GetAsCurrency(AssetType.MicroCreditAmount).Append(cancel);

            bot.SetButtons(user.Id, Terms.Get(21, user, "How much?"), monthly);
        }

        public static void PayWithCreditCard(TelegramBotClient bot, User user, string value)
        {
            var amount = value.AsCurrency();
            AvailableAssets.Add(amount, AssetType.MicroCreditAmount);

            user.Person.Liabilities.CreditCard += amount;
            user.Person.Expenses.CreditCard += (int) (amount * 0.03);
            user.History.Add(ActionType.MicroCredit, amount);

            SmallCircleButtons(bot, user, Terms.Get(13, user, "Done."));
        }

        public static void GetCredit(TelegramBotClient bot, User user)
        {
            var cancel = Terms.Get(6, user, "Cancel");
            user.Stage = Stage.GetCredit;
            bot.SetButtons(user.Id, Terms.Get(21, user, "How much?"), "1000", "2000", "5000", "10 000", "20 000", cancel);
        }

        public static void GetCredit(TelegramBotClient bot, User user, string value)
        {
            var amount = value.AsCurrency();

            if (amount % 1000 > 0 || amount < 1000)
            {
                bot.SendMessage(user.Id, Terms.Get(24, user, "Invalid amount. The amount must be a multiple of 1000"));
                return;
            }

            user.GetCredit(amount);

            SmallCircleButtons(bot, user, Terms.Get(22, user, "Ok, you've got {0}", amount.AsCurrency()));
        }

        public static void PayCredit(TelegramBotClient bot, User user, string value)
        {
            var amount = value.AsCurrency();

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

            user.PayCredit(amount, regular: true);
            ReduceLiabilities(bot, user);
        }

        public static void ReduceLiabilities(TelegramBotClient bot, User user, Stage stage)
        {
            int cost = 1_000;
            var cancel = Terms.Get(6, user, "Cancel");

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

                case Stage.ReduceSmallCredit:
                    cost = user.Person.Liabilities.SmallCredits;
                    break;

                case Stage.ReduceBankLoan:
                    cost = user.Person.Liabilities.BankLoan;
                    break;

                case Stage.ReduceBoatLoan:
                    cost = user.Person.Assets.Boat.Mortgage;
                    break;
            }

            if (user.Person.Cash < cost)
            {
                bot.SendMessage(user.Id, Terms.Get(23, user, "You don't have {0}, but only {1}", cost.AsCurrency(), user.Person.Cash.AsCurrency()));
                ReduceLiabilities(bot, user);
                return;
            }

            if (stage == Stage.ReduceBankLoan)
            {
                user.Stage = stage;
                var buttons = new[] { 1000, 5000, 10000, cost, user.Person.Cash / 1000 * 1000 }
                    .Where(x => x <= user.Person.Cash && x <= cost)
                    .OrderBy(x => x)
                    .Distinct()
                    .Select(x => x.AsCurrency())
                    .Append(cancel);

                bot.SetButtons(user.Id, Terms.Get(21, user, "How much?"), buttons);
                return;
            }

            var reduceLiabilities = Terms.Get(40, user, "Reduce Liabilities");
            var type = Terms.Get((int)stage, user, "Liability");
            var yes = Terms.Get(4, user, "Yes");

            Ask(bot, user, stage, $"{reduceLiabilities} - {type}. {yes}?", Terms.Get(4, user, "Yes"));
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
            var smallCredit = Terms.Get(92, user, "Small Credit");
            var bankLoan = Terms.Get(47, user, "Bank Loan");
            var boatLoan = Terms.Get(114, user, "Boat Loan");
            var cancel = Terms.Get(6, user, "Cancel");

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

            if (l.SmallCredits > 0)
            {
                buttons.Add(smallCredit);
                liabilities += $"*{smallCredit}:* {l.SmallCredits.AsCurrency()} - {x.SmallCredits.AsCurrency()} {monthly}{Environment.NewLine}";
            }

            if (l.BankLoan > 0)
            {
                buttons.Add(bankLoan);
                liabilities += $"*{bankLoan}:* {l.BankLoan.AsCurrency()} - {x.BankLoan.AsCurrency()} {monthly}{Environment.NewLine}";
            }

            var boat = user.Person.Assets.Boat;
            if (boat != null && boat.CashFlow != 0)
            {
                buttons.Add(boatLoan);
                liabilities += $"*{boatLoan}:* {boat.Mortgage.AsCurrency()} - {boat.CashFlow.AsCurrency()} {monthly}{Environment.NewLine}";
            }

            if (buttons.Any())
            {
                var cashTerm = Terms.Get(51, user, "Cash");
                user.Stage = Stage.Nothing;
                bot.SetButtons(user.Id, $"*{cashTerm}:* {user.Person.Cash.AsCurrency()}{Environment.NewLine}{Environment.NewLine}{liabilities}", buttons.Append(cancel));
                return;
            }

            SmallCircleButtons(bot, user, Terms.Get(93, user, "You have no liabilities."));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using CashFlowBot.Data;
using CashFlowBot.DataBase;
using CashFlowBot.Extensions;
using CashFlowBot.Models;
using Telegram.Bot;

namespace CashFlowBot.Actions
{
    public class BankruptcyActions : BaseActions
    {
        public static void SellAsset(TelegramBotClient bot, User user, int item)
        {
            var assets = user.Person.Assets.Items.OrderBy(x => x.Type).ToList();

            if (item < assets.Count)
            {
                var asset = assets[item];
                asset.Sell(ActionType.BankruptcySellAsset, asset.BancrupcySellPrice);

                user.Person.Cash += asset.BancrupcySellPrice;
                user.PayCredit(user.Person.Cash);
            }

            ShowMenu(bot, user);
        }

        public static void ShowMenu(TelegramBotClient bot, User user)
        {
            var stopGame = Terms.Get(41, user, "Stop Game");
            var history = Terms.Get(2, user, "History");

            user.Person.Bankruptcy = user.Person.CashFlow < 0;
            if (!user.Person.Bankruptcy)
            {
                ReturnToGame(bot, user);
                return;
            }

            if (user.Person.Assets.Items.Any())
            {
                ShowSellAssetsMenu(bot, user);
                return;
            }

            user.Person.ReduceCredits();

            if (user.Person.Bankruptcy)
            {
                bot.SetButtons(user.Id, Terms.Get(129, user, "You are bankrupt. Game is over."), history, stopGame);
                return;
            }

            ReturnToGame(bot, user);
        }

        private static void ReturnToGame(TelegramBotClient bot, User user)
        {
            user.Person.CreditsReduced = false;
            bot.SendMessage(user.Id, Terms.Get(130, user, "You have paid off your debts, you can continue."));
            Cancel(bot, user);
        }

        private static void ShowSellAssetsMenu(TelegramBotClient bot, User user)
        {
            var history = Terms.Get(2, user, "History");
            var price = Terms.Get(64, user, "Price");
            var cashFlow = Terms.Get(55, user, "Cash Flow");
            var bankLoan = Terms.Get(47, user, "Bank Loan");
            var buttons = new List<string>();
            var assets = new List<string>();

            int i = 0;
            foreach (var asset in user.Person.Assets.Items.OrderBy(x => x.Type))
            {
                buttons.Add($"#{i++}");
                assets.Add(asset.CashFlow == 0
                ? $"#{i} - {asset.Title} - {price}: {asset.BancrupcySellPrice}"
                : $"#{i} - {asset.Title} - {price}: {asset.BancrupcySellPrice}, {cashFlow}: {asset.TotalCashFlow.AsCurrency()}");
            }

            var message = $"*{Terms.Get(126, user, "You're out of money.")}*" +
                Environment.NewLine + $"{bankLoan}: {user.Person.Liabilities.BankLoan.AsCurrency()}" +
                Environment.NewLine + $"{cashFlow}: {user.Person.CashFlow.AsCurrency()}" +
                Environment.NewLine + Terms.Get(127, user, "You have to sell your assets till you cash flow is positive.") +
                Environment.NewLine + Terms.Get(128, user, "What asset do you want to sell?") +
                Environment.NewLine + string.Join(Environment.NewLine, assets);

            user.Stage = Stage.Bankruptcy;
            bot.SetButtons(user.Id, message, buttons.Append(history));
        }
    }
}

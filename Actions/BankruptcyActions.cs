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
        public static void ShowMenu(TelegramBotClient bot, User user)
        {

            // check if still bankrupt?

            var stopGame = Terms.Get(41, user, "Stop Game");
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

            if (assets.Any())
            {
                var message = $"*{Terms.Get(126, user, "You're out of money.")}*" +
                    Environment.NewLine + $"{bankLoan}: {user.Person.Expenses.BankLoan.AsCurrency()}" +
                    Environment.NewLine + $"{cashFlow}: {user.Person.CashFlow.AsCurrency()}" +
                    Environment.NewLine + Terms.Get(127, user, "You have to sell your assets till you cash flow is positive.") +
                    Environment.NewLine + Terms.Get(128, user, "What asset do you want to sell?") +
                    Environment.NewLine + string.Join(Environment.NewLine, assets);

                bot.SetButtons(user.Id, message, buttons.Append(history));
                return;
            }
            else
            {
                // do it only once!
                user.Person.Expenses.CarLoan /= 2;
                user.Person.Expenses.CreditCard /= 2;
                user.Person.Expenses.SmallCredits /= 2;
                user.Person.Liabilities.CarLoan /= 2;
                user.Person.Liabilities.CreditCard /= 2;
                user.Person.Liabilities.SmallCredits /= 2;

                user.History.Add(ActionType.CreditsReduce, 0); // todo
                user.Person.Bankruptcy = user.Person.CashFlow < 0;

                if (user.Person.Bankruptcy)
                {
                    bot.SetButtons(user.Id, Terms.Get(129, user, "You are bankrupt. Game is over."), history, stopGame);
                }
                else
                {
                    bot.SendMessage(user.Id, Terms.Get(130, user, "You have paid off your debts, you can continue."));
                    Cancel(bot, user);
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using CashFlowBot.Data;
using CashFlowBot.DataBase;
using CashFlowBot.Extensions;
using CashFlowBot.Models;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Terms = CashFlowBot.DataBase.Terms;

namespace CashFlowBot
{
    public static class Actions
    {
        public static void Downsize(TelegramBotClient bot, User user)
        {
            var expenses = user.Person.Expenses.Total;

            bot.SendMessage(user.Id,
            Terms.Get(87, user, "You were fired. You've payed total amount of your expenses: {0} and lose 2 turns.", expenses.AsCurrency()));

            if (user.Person.Cash < expenses)
            {
                var delta = expenses - user.Person.Cash;
                var credit = delta / 1_000 * 1_000;

                if (delta % 1_000 > 0) credit += 1_000;

                user.GetCredit(credit);
                bot.SendMessage(user.Id, Terms.Get(88, user, "You've taken {0} from bank.", credit.AsCurrency()));
            }

            user.Person.Cash -= expenses;
            Cancel(bot, user);
        }

        public static void Divorce(TelegramBotClient bot, User user)
        {
            bot.SendMessage(user.Id, Terms.Get(72, user, "You've lost {0}.", user.Person.Cash));
            user.Person.Cash = 0;
            Cancel(bot, user);
        }

        public static void TaxAudit(TelegramBotClient bot, User user)
        {
            bot.SendMessage(user.Id, Terms.Get(72, user, "You've lost {0}.", user.Person.Cash/2));
            user.Person.Cash /= 2;
            Cancel(bot, user);
        }

        public static void AdminMenu(TelegramBotClient bot, User user)
        {
            user.Stage = Stage.Admin;
            bot.SetButtons(user.Id, "Hi, Admin.",
            "Logs", "Bring Down", "Users", "Available Assets", "Cancel");
        }

        public static void ShowAvailableAssets(TelegramBotClient bot, User user, string value)
        {
            if (value == "All")
            {
                var assets = new List<string>();

                foreach (var type in Enum.GetValues(typeof(AssetType)))
                {
                    var assetType = type.ToString().ParseEnum<AssetType>();
                    var count = AvailableAssets.Get(assetType).Count;

                    if (count > 0)
                    {
                        assets.AddRange(AvailableAssets.Get(assetType).Select(x => $"*{assetType}*: '{x}' - '{x.ToInt().AsCurrency()}'"));
                    }
                }

                bot.SendMessage(user.Id, string.Join(Environment.NewLine, assets));
                user.Stage = Stage.Admin;
                AdminMenu(bot, user);
            }
            else
            {
                var assetType = value.SubStringTo("-").Trim().ParseEnum<AssetType>();
                var assets = AvailableAssets.Get(assetType).Select(x => $"'{x}' - '{x.ToInt().AsCurrency()}'");

                user.Stage = Stage.AdminAvailableAssetsClear;
                bot.SetButtons(user.Id, string.Join(Environment.NewLine, assets), $"Clear {assetType}", "Back");
            }
        }

        public static void ClearAvailableAssets(TelegramBotClient bot, User user, string value)
        {
            var assetType = value.Split(" ").Last().ParseEnum<AssetType>();
            AvailableAssets.Clear(assetType);
            AdminMenu(bot, user);
        }

        public static void NotifyAdmins(TelegramBotClient bot, User user)
        {
            if (Users.AllUsers.All(x => !x.IsAdmin))
            {
                user.IsAdmin = true;
                return;
            }

            var rkm = new ReplyKeyboardMarkup
            {
                Keyboard = new[] { $"Make {user.Id} admin", Terms.Get(6, user, "Cancel") }.Select(button => new KeyboardButton[] { button })
            };

            foreach (var usr in Users.AllUsers)
            {
                if (!usr.IsAdmin) continue;

                bot.SendTextMessageAsync(usr.Id, $"{user.Name} wants to become Admin.", replyMarkup: rkm, parseMode: ParseMode.Default);
            }
        }

        public static void ShowUsers(TelegramBotClient bot, User user)
        {
            var users = Users.AllUsers.Select(x => $"{(x.IsAdmin ? "A" : "")}[{x.Id}] {x.Name}").ToList();
            bot.SendMessage(user.Id, $"There are {users.Count} users.");
            bot.SendMessage(user.Id, string.Join(Environment.NewLine, users), ParseMode.Default);
        }

        public static void BuyLand(TelegramBotClient bot, User user)
        {
            var landTypes = AvailableAssets.Get(AssetType.LandType).ToArray();

            if (user.Person.Cash == 0)
            {
                SmallCircleButtons(bot, user, Terms.Get(5, user, "You don't have enough money"));
                return;
            }

            user.Stage = Stage.BuyLandTitle;
            bot.SetButtons(user.Id, Terms.Get(7, user, "Title:"), landTypes.Append(Terms.Get(6, user, "Cancel")));
        }

        public static void BuyLand(TelegramBotClient bot, User user, string value)
        {
            var title = value.Trim().ToUpper();
            var number = value.AsCurrency();
            var prices = AvailableAssets.Get(AssetType.LandPrice).AsCurrency().Append(Terms.Get(6, user, "Cancel"));

            switch (user.Stage)
            {
                case Stage.BuyLandTitle:
                    user.Stage = Stage.BuyLandPrice;
                    user.Person.Assets.Add(title, AssetType.Land);
                    bot.SetButtons(user.Id, Terms.Get(8, user, "What is the price?"), prices);
                    return;

                case Stage.BuyLandPrice:
                    var land = user.Person.Assets.Lands.First(a => a.Price == 0);

                    if (number <= 0)
                    {
                        bot.SetButtons(user.Id, Terms.Get(9, user, "Invalid price value. Try again."), prices);
                        return;
                    }

                    if (user.Person.Cash < number)
                    {
                        SmallCircleButtons(bot, user, Terms.Get(23, user, "You don''t have {0}, but only {1}", number.AsCurrency(), user.Person.Cash.AsCurrency()));
                        return;
                    }

                    land.Price = number;
                    user.Person.Cash -= number;

                    AvailableAssets.Add(land.Title, AssetType.LandType);
                    AvailableAssets.Add(land.Price, AssetType.LandPrice);

                    SmallCircleButtons(bot, user, Terms.Get(13, user, "Done."));
                    return;
            }
        }

        public static void BuyBusiness(TelegramBotClient bot, User user)
        {
            var businesses = AvailableAssets.Get(user.Person.BigCircle ? AssetType.BigBusinessType : AssetType.SmallBusinessType).ToArray();

            if (user.Person.Cash == 0)
            {
                SmallCircleButtons(bot, user, Terms.Get(5, user, "You don't have enough money"));
                return;
            }

            user.Stage = Stage.BuyBusinessTitle;
            bot.SetButtons(user.Id, Terms.Get(7, user, "Title:"), businesses.Append(Terms.Get(6, user, "Cancel")));
        }

        public static void BuyBusiness(TelegramBotClient bot, User user, string value)
        {
            var title = value.Trim().ToUpper();
            var number = value.AsCurrency();
            var prices = AvailableAssets.Get(user.Person.BigCircle ? AssetType.BigBusinessBuyPrice : AssetType.SmallBusinessBuyPrice)
                .AsCurrency().Append(Terms.Get(6, user, "Cancel"));
            var firstPayments = AvailableAssets.Get(AssetType.SmallBusinessFirstPayment)
                .AsCurrency().Append(Terms.Get(6, user, "Cancel"));
            var cashFlows = AvailableAssets.Get(user.Person.BigCircle ? AssetType.BigBusinessCashFlow : AssetType.SmallBusinessCashFlow)
                .AsCurrency().Append(Terms.Get(6, user, "Cancel"));

            switch (user.Stage)
            {
                case Stage.BuyBusinessTitle:
                    user.Stage = Stage.BuyBusinessPrice;
                    user.Person.Assets.Add(title, AssetType.Business, user.Person.BigCircle);
                    bot.SetButtons(user.Id, Terms.Get(8, user, "What is the price?"), prices);
                    return;

                case Stage.BuyBusinessPrice:
                    if (number <= 0)
                    {
                        bot.SetButtons(user.Id, Terms.Get(9, user, "Invalid price value. Try again."), prices);
                        return;
                    }

                    if (user.Person.BigCircle && user.Person.Cash < number)
                    {
                        bot.SendMessage(user.Id, Terms.Get(23, user, "You don''t have {0}, but only {1}", number.AsCurrency(), user.Person.Cash.AsCurrency()));
                        return;
                    }

                    user.Person.Assets.Businesses.First(a => a.Price == 0).Price = number;

                    if (user.Person.BigCircle)
                    {
                        user.Stage = Stage.BuyBusinessCashFlow;
                        user.Person.Cash -= number;

                        bot.SetButtons(user.Id, Terms.Get(12, user, "What is the cash flow?"), cashFlows);
                        return;
                    }

                    user.Stage = Stage.BuyBusinessFirstPayment;
                    bot.SetButtons(user.Id, Terms.Get(10, user, "What is the first payment?"), firstPayments);
                    return;

                case Stage.BuyBusinessFirstPayment:
                    if (number <= 0)
                    {
                        bot.SendMessage(user.Id, Terms.Get(11, user, "Invalid first payment amount."));
                        return;
                    }

                    if (user.Person.Cash < number)
                    {
                        SmallCircleButtons(bot, user, Terms.Get(23, user, "You don''t have {0}, but only {1}", number.AsCurrency(), user.Person.Cash.AsCurrency()));
                        return;
                    }

                    var asset = user.Person.Assets.Businesses.First(a => a.Mortgage == 0);
                    asset.Mortgage = asset.Price - number;
                    user.Stage = Stage.BuyBusinessCashFlow;
                    user.Person.Cash -= number;

                    bot.SetButtons(user.Id, Terms.Get(12, user, "What is the cash flow?"), cashFlows);
                    return;

                case Stage.BuyBusinessCashFlow:
                    var business = user.Person.Assets.Businesses.First(a => a.CashFlow == 0);
                    business.CashFlow = number;

                    AvailableAssets.Add(business.Title, user.Person.BigCircle ? AssetType.BigBusinessType : AssetType.SmallBusinessType);
                    AvailableAssets.Add(business.Price, user.Person.BigCircle ? AssetType.BigBusinessBuyPrice : AssetType.SmallBusinessBuyPrice);
                    AvailableAssets.Add(business.Price-business.Mortgage, AssetType.SmallBusinessFirstPayment);
                    AvailableAssets.Add(business.CashFlow, user.Person.BigCircle ? AssetType.BigBusinessCashFlow : AssetType.SmallBusinessCashFlow);

                    bot.SendMessage(user.Id, Terms.Get(13, user, "Done."));
                    Cancel(bot, user);
                    return;
            }
        }

        public static void SellLand(TelegramBotClient bot, User user)
        {
            var lands = user.Person.Assets.Lands;

            if (lands.Any())
            {
                var landsIds = new List<string>();
                var landsList = string.Empty;

                for (int i = 0; i < lands.Count; i++)
                {
                    landsIds.Add($"#{i+1}");
                    landsList += $"{Environment.NewLine}*#{i+1}* - {lands[i].Description}";
                }

                landsIds.Add(Terms.Get(6, user, "Cancel"));
                user.Stage = Stage.SellLandTitle;

                bot.SetButtons(user.Id, Terms.Get(99, user, "What Land do you want to sell?{0}{1}", Environment.NewLine, landsList), landsIds);
                return;
            }

            SmallCircleButtons(bot, user, Terms.Get(100, user, "You have no Land."));
        }

        public static void SellLand(TelegramBotClient bot, User user, string value)
        {
            var lands = user.Person.Assets.Lands;
            var prices = AvailableAssets.Get(AssetType.LandPrice)
                .AsCurrency().Append(Terms.Get(6, user, "Cancel"));

            switch (user.Stage)
            {
                case Stage.SellLandTitle:
                    var index = value.Replace("#", "").ToInt();

                    if (index < 1 || index > lands.Count)
                    {
                        bot.SendMessage(user.Id, Terms.Get(101, user, "Invalid land number."));
                        return;
                    }

                    lands[index - 1].Title += "*";
                    user.Stage = Stage.SellLandPrice;
                    bot.SetButtons(user.Id, Terms.Get(8, user, "What is the price?"), prices);
                    return;

                case Stage.SellLandPrice:
                    var price = value.AsCurrency();
                    var land = lands.First(x => x.Title.EndsWith("*"));

                    user.Person.Cash += price;
                    land.Delete();

                    AvailableAssets.Add(price, AssetType.LandPrice);

                    SmallCircleButtons(bot, user, Terms.Get(13, user, "Done."));
                    return;
            }
        }

        public static void SellBusiness(TelegramBotClient bot, User user)
        {
            var businesses = user.Person.Assets.Businesses;

            if (businesses.Any())
            {
                var businessesIds = new List<string>();
                var businessesList = string.Empty;

                for (int i = 0; i < businesses.Count; i++)
                {
                    businessesIds.Add($"#{i+1}");
                    businessesList += $"{Environment.NewLine}*#{i+1}* - {businesses[i].Description}";
                }

                businessesIds.Add(Terms.Get(6, user, "Cancel"));
                user.Stage = Stage.SellBusinessTitle;

                bot.SetButtons(user.Id, Terms.Get(78, user, "What Business do you want to sell?{0}{1}", Environment.NewLine, businessesList), businessesIds);
                return;
            }

            SmallCircleButtons(bot, user, Terms.Get(77, user, "You have no Business."));
        }

        public static void SellBusiness(TelegramBotClient bot, User user, string value)
        {
            var businesses = user.Person.Assets.Businesses;
            var prices = AvailableAssets.Get(AssetType.SmallBusinessBuyPrice)
                .AsCurrency().Append(Terms.Get(6, user, "Cancel"));

            switch (user.Stage)
            {
                case Stage.SellBusinessTitle:
                    var index = value.Replace("#", "").ToInt();

                    if (index < 1 || index > businesses.Count)
                    {
                        bot.SendMessage(user.Id, Terms.Get(76, user, "Invalid business number."));
                        return;
                    }

                    businesses[index - 1].Title += "*";
                    user.Stage = Stage.SellBusinessPrice;
                    bot.SetButtons(user.Id, Terms.Get(8, user, "What is the price?"), prices);
                    return;

                case Stage.SellBusinessPrice:
                    var price = value.AsCurrency();
                    var business = businesses.First(x => x.Title.EndsWith("*"));

                    user.Person.Cash += price - business.Mortgage;
                    business.Delete();

                    AvailableAssets.Add(price, AssetType.BigBusinessSellPrice);

                    SmallCircleButtons(bot, user, Terms.Get(13, user, "Done."));
                    return;
            }
        }

        public static void BuyRealEstate(TelegramBotClient bot, User user)
        {
            var properties = AvailableAssets.Get(AssetType.RealEstateType).ToArray();

            if (user.Person.Cash == 0)
            {
                SmallCircleButtons(bot, user, Terms.Get(5, user, "You don't have enough money"));
                return;
            }

            user.Stage = Stage.BuyRealEstateTitle;
            bot.SetButtons(user.Id, Terms.Get(7, user, "Title:"), properties.Append(Terms.Get(6, user, "Cancel")));
        }

        public static void BuyRealEstate(TelegramBotClient bot, User user, string value)
        {
            var title = value.Trim().ToUpper();
            var number = value.AsCurrency();
            var prices = AvailableAssets.Get(AssetType.RealEstateBuyPrice).AsCurrency().Append(Terms.Get(6, user, "Cancel"));
            var firstPayments = AvailableAssets.Get(AssetType.RealEstateFirstPayment).AsCurrency().Append(Terms.Get(6, user, "Cancel"));
            var cashFlows = AvailableAssets.Get(AssetType.RealEstateCashFlow).AsCurrency().Append(Terms.Get(6, user, "Cancel"));

            switch (user.Stage)
            {
                case Stage.BuyRealEstateTitle:
                    user.Stage = Stage.BuyRealEstatePrice;
                    user.Person.Assets.Add(title, AssetType.RealEstate);
                    bot.SetButtons(user.Id, Terms.Get(8, user, "What is the price?"), prices);
                    return;

                case Stage.BuyRealEstatePrice:
                    if (number <= 0)
                    {
                        bot.SetButtons(user.Id, Terms.Get(9, user, "Invalid price value. Try again."), prices);
                        return;
                    }

                    user.Person.Assets.RealEstates.First(a => a.Price == 0).Price = number;
                    user.Stage = Stage.BuyRealEstateFirstPayment;

                    bot.SetButtons(user.Id, Terms.Get(10, user, "What is the first payment?"), firstPayments);
                    return;

                case Stage.BuyRealEstateFirstPayment:
                    if (number <= 0)
                    {
                        bot.SendMessage(user.Id, Terms.Get(11, user, "Invalid first payment amount."));
                        return;
                    }

                    if (user.Person.Cash < number)
                    {
                        bot.SendMessage(user.Id, Terms.Get(23, user, "You don''t have {0}, but only {1}", number.AsCurrency(), user.Person.Cash.AsCurrency()));
                        return;
                    }

                    var asset = user.Person.Assets.RealEstates.First(a => a.Mortgage == 0);
                    asset.Mortgage = asset.Price - number;
                    user.Stage = Stage.BuyRealEstateCashFlow;
                    user.Person.Cash -= number;

                    bot.SetButtons(user.Id, Terms.Get(12, user, "What is the cash flow?"), cashFlows);
                    return;

                case Stage.BuyRealEstateCashFlow:
                    var realEstate = user.Person.Assets.RealEstates.First(a => a.CashFlow == 0);
                    realEstate.CashFlow = number;

                    AvailableAssets.Add(realEstate.Title, AssetType.RealEstateType);
                    AvailableAssets.Add(realEstate.Price, AssetType.RealEstateBuyPrice);
                    AvailableAssets.Add(realEstate.Price-realEstate.Mortgage, AssetType.RealEstateFirstPayment);
                    AvailableAssets.Add(realEstate.CashFlow, AssetType.RealEstateCashFlow);

                    SmallCircleButtons(bot, user, Terms.Get(13, user, "Done."));
                    return;
            }
        }

        public static void SellRealEstate(TelegramBotClient bot, User user)
        {
            var properties = user.Person.Assets.RealEstates;

            if (properties.Any())
            {
                var realEstateIds = new List<string>();
                var realEstateList = string.Empty;

                for (int i = 0; i < properties.Count; i++)
                {
                    realEstateIds.Add($"#{i+1}");
                    realEstateList += $"{Environment.NewLine}*#{i+1}* - {properties[i].Description}";
                }

                realEstateIds.Add(Terms.Get(6, user, "Cancel"));
                user.Stage = Stage.SellRealEstateTitle;

                bot.SetButtons(user.Id, Terms.Get(14, user, "What RealEstate do you want to sell?{0}{1}", Environment.NewLine, realEstateList), realEstateIds);
                return;
            }

            SmallCircleButtons(bot, user, Terms.Get(15, user, "You have no properties."));
        }

        public static void SellRealEstate(TelegramBotClient bot, User user, string value)
        {
            var properties = user.Person.Assets.RealEstates;
            var prices = AvailableAssets.Get(AssetType.RealEstateSellPrice).AsCurrency().Append(Terms.Get(6, user, "Cancel"));

            switch (user.Stage)
            {
                case Stage.SellRealEstateTitle:
                    var index = value.Replace("#", "").ToInt();

                    if (index < 1 || index > properties.Count)
                    {
                        bot.SendMessage(user.Id, Terms.Get(16, user, "Invalid Real Estate number."));
                        return;
                    }

                    properties[index - 1].Title += "*";
                    user.Stage = Stage.SellRealEstatePrice;
                    bot.SetButtons(user.Id, Terms.Get(8, user, "What is the price?"), prices);
                    return;

                case Stage.SellRealEstatePrice:
                    var price = value.AsCurrency();
                    var realEstate = properties.First(x => x.Title.EndsWith("*"));

                    user.Person.Cash += price - realEstate.Mortgage;
                    realEstate.Delete();

                    AvailableAssets.Add(price, AssetType.RealEstateSellPrice);

                    SmallCircleButtons(bot, user, Terms.Get(13, user, "Done."));
                    return;
            }
        }

        public static void BuyStocks(TelegramBotClient bot, User user)
        {
            var stocks = AvailableAssets.Get(AssetType.Stock).Append(Terms.Get(6, user, "Cancel"));

            if (user.Person.Cash == 0)
            {
                SmallCircleButtons(bot, user, Terms.Get(5, user, "You don't have enough money"));
                return;
            }

            user.Stage = Stage.BuyStocksTitle;
            bot.SetButtons(user.Id, Terms.Get(7, user, "Title:"), stocks);
        }

        public static void BuyStocks(TelegramBotClient bot, User user, string value)
        {
            var title = value.Trim().ToUpper();
            var number = value.AsCurrency();
            var prices = AvailableAssets.Get(AssetType.StockPrice).AsCurrency().ToList();
            prices.Add(Terms.Get(6, user, "Cancel"));

            switch (user.Stage)
            {
                case Stage.BuyStocksTitle:

                    user.Stage = Stage.BuyStocksPrice;
                    user.Person.Assets.Add(title, AssetType.Stock);
                    bot.SetButtons(user.Id, Terms.Get(8, user, "What is the price?"), prices);
                    return;

                case Stage.BuyStocksPrice:
                    if (number <= 0)
                    {
                        bot.SetButtons(user.Id, Terms.Get(9, user, "Invalid price value. Try again."), prices);
                        return;
                    }

                    user.Person.Assets.Stocks.First(a => a.Price == 0).Price = number;
                    user.Stage = Stage.BuyStocksQtty;

                    int upToQtty = user.Person.Cash / number;
                    bot.SetButtons(user.Id,
                    Terms.Get(17, user,"You can buy up to {0} stocks. How much stocks would you like to buy?", upToQtty),
                    upToQtty.ToString(),
                    Terms.Get(6, user, "Cancel"));
                    return;

                case Stage.BuyStocksQtty:
                    if (number <= 0)
                    {
                        bot.SendMessage(user.Id, Terms.Get(18, user, "Invalid quantity value. Try again."));
                        return;
                    }

                    var asset = user.Person.Assets.Stocks.First(a => a.Qtty == 0);

                    var totalPrice = asset.Price * number;
                    var availableCash = user.Person.Cash;
                    int availableQtty = availableCash / asset.Price;

                    var defaultMsg = "{0} x {1} = {2}. You have only {3}. You can buy {4} stocks. So, what quantity of stocks do you want to buy?";
                    var message = Terms.Get(19, user, defaultMsg, number, asset.Price.AsCurrency(), totalPrice.AsCurrency(), availableCash.AsCurrency(), availableQtty);

                    if (totalPrice > availableCash)
                    {
                        bot.SendMessage(user.Id, message);
                        return;
                    }

                    asset.Qtty = number;
                    user.Person.Cash -= totalPrice;

                    AvailableAssets.Add(asset.Title, AssetType.Stock);
                    AvailableAssets.Add(asset.Price, AssetType.StockPrice);

                    SmallCircleButtons(bot, user, Terms.Get(13, user, "Done."));
                    return;
            }
        }

        public static void SellStocks(TelegramBotClient bot, User user)
        {
            var stocks = user.Person.Assets.Stocks
                .Select(x => x.Title)
                .Distinct()
                .Append(Terms.Get(6, user, "Cancel"))
                .ToList();

            if (stocks.Count > 1)
            {
                user.Stage = Stage.SellStocksTitle;
                bot.SetButtons(user.Id, Terms.Get(27, user, "What stocks do you want to sell?"), stocks);
            }
            else
            {
                SmallCircleButtons(bot, user, Terms.Get(49, user, "You have no stocks."));
            }
        }

        public static void SellStocks(TelegramBotClient bot, User user, string value)
        {
            var title = value.Trim().ToUpper();
            var number = value.AsCurrency();
            var stocks = user.Person.Assets.Stocks;
            var prices = AvailableAssets.Get(AssetType.StockPrice).AsCurrency().ToList();
            prices.Add(Terms.Get(6, user, "Cancel"));

            switch (user.Stage)
            {
                case Stage.SellStocksTitle:
                    var assets = stocks.Where(x => x.Title == title).ToList();

                    if (!assets.Any())
                    {
                        SmallCircleButtons(bot, user, "Invalid stocks name.");
                        return;
                    }

                    user.Stage = Stage.SellStocksPrice;
                    assets.ForEach(x => x.Title += "*");
                    bot.SetButtons(user.Id, Terms.Get(8, user, "What is the price?"), prices);
                    return;

                case Stage.SellStocksPrice:
                    var stocksToSell = stocks.Where(x => x.Title.EndsWith("*")).ToList();
                    var qtty = stocksToSell.Sum(x => x.Qtty);

                    if (number <= 0)
                    {
                        bot.SetButtons(user.Id, Terms.Get(9, user, "Invalid price value. Try again."), prices);
                        return;
                    }

                    user.Person.Cash += qtty * number;
                    stocksToSell.ForEach(x => x.Delete());

                    AvailableAssets.Add(number, AssetType.StockPrice);

                    SmallCircleButtons(bot, user, Terms.Get(13, user, "Done."));
                    return;
            }
        }

        public static void ReduceLiabilities(TelegramBotClient bot, User user, Stage stage)
        {
            if (user.Person.Cash < 1000)
            {
                Cancel(bot, user);
                return;
            }

            user.Stage = stage;
            int cost = 0;

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
            }

            var buttons = new[] { 1000, 5000, 10000, cost, user.Person.Cash/1000*1000 }
                .Where(x => x <= user.Person.Cash && x <= cost)
                .OrderBy(x => x)
                .Distinct()
                .Select(x => x.AsCurrency())
                .ToList();
            buttons.Add(Terms.Get(6, user, "Cancel"));

            bot.SetButtons(user.Id, Terms.Get(21, user, "How much?"), buttons);
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

            if (user.Person.Cash < 1000)
            {
                bot.SendMessage(user.Id, liabilities);
                SmallCircleButtons(bot, user, Terms.Get(48, user, "You don't have money to reduce liabilities, your balance is {0}", user.Person.Cash.AsCurrency()));
                return;
            }

            if (buttons.Any())
            {
                buttons.Add(Terms.Get(6, user, "Cancel"));
                bot.SetButtons(user.Id, liabilities, buttons);
                return;
            }

            SmallCircleButtons(bot, user, Terms.Get(93, user, "You have no liabilities."));
        }

        public static async void SmallOpportunity(TelegramBotClient bot, User user)
        {
            var rkm = new ReplyKeyboardMarkup
            {
                Keyboard = new List<IEnumerable<KeyboardButton>>
                {
                    new List<KeyboardButton> {Terms.Get(35, user, "Buy Stocks"), Terms.Get(36, user, "Sell Stocks")},
                    new List<KeyboardButton> {Terms.Get(82, user, "Stocks 2 to 1"), Terms.Get(83, user, "Stocks 1 to 2")},
                    new List<KeyboardButton>{Terms.Get(37, user, "Buy Real Estate")},
                    new List<KeyboardButton>{ Terms.Get(6, user, "Cancel") }
                }
            };

            user.Person.Assets.CleanUp();
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
                    new List<KeyboardButton>{Terms.Get(34, user, "Get Credit") },
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
                    new List<KeyboardButton>{Terms.Get(38, user, "Sell Real Estate")},
                    new List<KeyboardButton>{Terms.Get(75, user, "Sell Business")},
                    new List<KeyboardButton>{Terms.Get(98, user, "Sell Land") },
                    new List<KeyboardButton>{ Terms.Get(6, user, "Cancel") }
                }
            };

            user.Person.Assets.CleanUp();
            user.Stage = Stage.Nothing;

            await bot.SendTextMessageAsync(user.Id, Terms.Get(89, user, "What do you want?"), replyMarkup: rkm, parseMode: ParseMode.Markdown);
        }

        public static void PayWithCreditCard(TelegramBotClient bot, User user)
        {
            user.Stage = Stage.MicroCreditAmount;
            var monthly = AvailableAssets.Get(AssetType.MicroCreditAmount).AsCurrency().Append(Terms.Get(6, user, "Cancel"));

            bot.SetButtons(user.Id, Terms.Get(21, user, "How much?"), monthly);
        }

        public static void PayWithCreditCard(TelegramBotClient bot, User user, string value)
        {
            var amount = value.AsCurrency();

            switch (user.Stage)
            {
                case Stage.MicroCreditAmount:
                    var monthly = AvailableAssets.Get(AssetType.MicroCreditMonthly).AsCurrency();
                    AvailableAssets.Add(amount, AssetType.MicroCreditAmount);

                    user.Person.Liabilities.SmallCredits += amount;
                    user.Stage = Stage.MicroCreditMonthly;
                    bot.SetButtons(user.Id, Terms.Get(97, user, "What is the monthly payment?"), monthly);
                    return;

                case Stage.MicroCreditMonthly:
                    AvailableAssets.Add(amount, AssetType.MicroCreditMonthly);

                    user.Person.Expenses.SmallCredits += amount;
                    SmallCircleButtons(bot, user, Terms.Get(13, user, "Done."));
                    return;

            }

            user.Stage = Stage.MicroCreditAmount;
            bot.SetButtons(user.Id, Terms.Get(21, user, "How much?"), "1000", "2000", "5000", "10 000", "20 000", Terms.Get(6, user, "Cancel"));

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
            var stocks = user.Person.Assets.Stocks;
            var assets = stocks.Where(x => x.Title == title).ToList();

            if (!assets.Any())
            {
                SmallCircleButtons(bot, user, "Invalid stocks name.");
                return;
            }

            assets.Where(x => x.Title == title).ToList().ForEach(x => x.Qtty = (int)(x.Qtty * k));
            Cancel(bot, user);
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
                    new List<KeyboardButton>{Terms.Get(6, user, "Cancel")}
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
            SmallCircleButtons(bot, user, Terms.Get(91, user, "You've payed {0}, now you can use two dice in next 3 turns.",
            amount.AsCurrency()));
        }

        public static void GetCredit(TelegramBotClient bot, User user)
        {
            user.Stage = Stage.GetCredit;
            bot.SetButtons(user.Id, Terms.Get(21, user, "How much?"), "1000", "2000", "5000", "10 000", "20 000", Terms.Get(6, user, "Cancel"));
        }

        public static void GetMoney(TelegramBotClient bot, User user, string value)
        {
            var amount = value.AsCurrency();
            user.Person.Cash += amount;

            bot.SendMessage(user.Id, Terms.Get(22, user, "Ok, you've got *{0}*", amount.AsCurrency()));
            Cancel(bot, user);
        }

        public static void GiveMoney(TelegramBotClient bot, User user, string value)
        {
            var amount = value.AsCurrency();

            if (user.Person.Cash < amount)
            {
                SmallCircleButtons(bot, user, Terms.Get(23, user, "You don't have {0}, but only {1}", amount.AsCurrency(), user.Person.Cash.AsCurrency()));
                return;
            }

            user.Person.Cash -= amount;

            AvailableAssets.Add(amount, user.Person.BigCircle ? AssetType.BigGiveMoney : AssetType.SmallGiveMoney);
            Cancel(bot, user);
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

        public static void PayCredit(TelegramBotClient bot, User user, string value, Stage stage)
        {
            var amount = value.AsCurrency();
            int expenses;

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

            switch (stage)
            {
                case Stage.ReduceMortgage:
                    amount = Math.Min(amount, user.Person.Liabilities.Mortgage);
                    expenses = user.Person.Expenses.Mortgage * amount / user.Person.Liabilities.Mortgage;

                    user.Person.Cash -= amount;
                    user.Person.Expenses.Mortgage -= expenses;
                    user.Person.Liabilities.Mortgage -= amount;
                    break;

                case Stage.ReduceSchoolLoan:
                    amount = Math.Min(amount, user.Person.Liabilities.SchoolLoan);
                    expenses = user.Person.Expenses.SchoolLoan * amount / user.Person.Liabilities.SchoolLoan;

                    user.Person.Cash -= amount;
                    user.Person.Expenses.SchoolLoan -= expenses;
                    user.Person.Liabilities.SchoolLoan -= amount;
                    break;

                case Stage.ReduceCarLoan:
                    amount = Math.Min(amount, user.Person.Liabilities.CarLoan);
                    expenses = user.Person.Expenses.CarLoan * amount / user.Person.Liabilities.CarLoan;

                    user.Person.Cash -= amount;
                    user.Person.Expenses.CarLoan -= expenses;
                    user.Person.Liabilities.CarLoan -= amount;
                    break;

                case Stage.ReduceCreditCard:
                    amount = Math.Min(amount, user.Person.Liabilities.CreditCard);
                    expenses = user.Person.Expenses.CreditCard * amount / user.Person.Liabilities.CreditCard;

                    user.Person.Cash -= amount;
                    user.Person.Expenses.CreditCard -= expenses;
                    user.Person.Liabilities.CreditCard -= amount;
                    break;

                case Stage.ReduceSmallCredit:
                    amount = Math.Min(amount, user.Person.Liabilities.SmallCredits);
                    expenses = user.Person.Expenses.SmallCredits * amount / user.Person.Liabilities.SmallCredits;

                    user.Person.Cash -= amount;
                    user.Person.Expenses.SmallCredits -= expenses;
                    user.Person.Liabilities.SmallCredits -= amount;
                    break;

                case Stage.ReduceBankLoan:
                    amount = Math.Min(amount, user.Person.Liabilities.BankLoan);
                    expenses = amount / 10;

                    user.Person.Cash -= amount;
                    user.Person.Expenses.BankLoan -= expenses;
                    user.Person.Liabilities.BankLoan -= amount;
                    break;
            }

            Cancel(bot, user);
        }

        public static void Cancel(TelegramBotClient bot, User user)
        {
            if (user.Person.BigCircle)
            {
                BigCircleButtons(bot, user);
                return;
            }

            SmallCircleButtons(bot, user, user.Person.Description);
        }

        public static void Confirm(TelegramBotClient bot, User user)
        {
            switch (user.Stage)
            {
                //case Stage.GetChild:
                //    user.Person.Expenses.Children++;
                //    SmallCircleButtons(bot, user, Terms.Get(25, user, "{0}, you have {1} children expenses.",
                //    user.Person.Profession, user.Person.Expenses.ChildrenExpenses.AsCurrency()));
                //    return;

                case Stage.StopGame:
                    user.Person.Clear();
                    user.Person.Expenses.Clear();
                    user.Stage = Stage.Nothing;

                    Start(bot, user);
                    return;

                case Stage.AdminBringDown:
                    Environment.Exit(0);
                    return;
            }

            Cancel(bot, user);
        }

        public static void Ask(TelegramBotClient bot, User user, Stage stage, string question, params string[] buttons)
        {
            user.Stage = stage;
            bot.SetButtons(user.Id, question, buttons.Append(Terms.Get(6, user, "Cancel")));
        }

        public static void ChangeLanguage(TelegramBotClient bot, User user) =>
            Ask(bot, user, Stage.Nothing, "Language/Мова", "UA", "EN", "DE");

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
            var rkm = new ReplyKeyboardMarkup {Keyboard = new List<IEnumerable<KeyboardButton>>()};

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

            user.Person.Create(profession);

            SmallCircleButtons(bot, user, Terms.Get(30, user, "Welcome, {0}!", user.Person.Profession));
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
            if (user.Person.Assets.Income > user.Person.Expenses.Total)
            {
                bot.SendMessage(user.Id, Terms.Get(68, user, "Your income is greater, then expenses. You are ready for Big Circle."));
                user.Person.InitialCashFlow = user.Person.Assets.Income / 10 * 1000;
                user.Person.Cash += user.Person.InitialCashFlow;
                user.Person.BigCircle = true;
                user.Person.Assets.Clear();

                Cancel(bot, user);
                return;
            }

            var rkm = new ReplyKeyboardMarkup
            {
                Keyboard = new List<IEnumerable<KeyboardButton>>
                {
                    new List<KeyboardButton>{Terms.Get(31, user, "Show my Data")},
                    new List<KeyboardButton>{Terms.Get(81, user, "Small Opportunity"), Terms.Get(84, user, "Big Opportunity") },
                    new List<KeyboardButton>{Terms.Get(86, user, "Doodads"), Terms.Get(85, user, "Market")},
                    new List<KeyboardButton>{ Terms.Get(80, user, "Downsize"), Terms.Get(39, user, "Baby")}, // todo 39
                    new List<KeyboardButton>{Terms.Get(79, user, "Pay Check")}
                }
            };

            user.Person.Assets.CleanUp();
            user.Stage = Stage.Nothing;

            await bot.SendTextMessageAsync(user.Id, message, replyMarkup: rkm, parseMode: ParseMode.Markdown);
        }
    }
}

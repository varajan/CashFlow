﻿using System;
using System.Linq;
using CashFlowBot.Data;
using CashFlowBot.Extensions;
using CashFlowBot.Models;
using Telegram.Bot;
using Terms = CashFlowBot.DataBase.Terms;

namespace CashFlowBot.Actions
{
    public class BuyActions : BaseActions
    {
        public static void BuyLand(TelegramBotClient bot, User user)
        {
            var landTypes = AvailableAssets.Get(AssetType.LandTitle).ToArray();

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
            var asset = user.Person.Assets.Lands.FirstOrDefault(a => a.IsDraft) ?? user.Person.Assets.Add(title, AssetType.LandTitle);
            var prices = AvailableAssets.Get(AssetType.LandBuyPrice).AsCurrency().Append(Terms.Get(6, user, "Cancel"));

            switch (user.Stage)
            {
                case Stage.BuyLandTitle:
                    user.Stage = Stage.BuyLandPrice;
                    bot.SetButtons(user.Id, Terms.Get(8, user, "What is the price?"), prices);
                    return;

                case Stage.BuyLandPrice:
                    if (number <= 0)
                    {
                        bot.SetButtons(user.Id, Terms.Get(9, user, "Invalid price value. Try again."), prices);
                        return;
                    }

                    if (user.Person.Cash < number)
                    {
                        asset.Price = number;
                        var message = Terms.Get(23, user, "You don''t have {0}, but only {1}", number.AsCurrency(), user.Person.Cash.AsCurrency());

                        bot.SetButtons(user.Id, message, Terms.Get(34, user, "Get Credit"), Terms.Get(6, user, "Cancel"));
                        return;
                    }

                    asset.Price = number;
                    CompleteTransaction();
                    return;

                case Stage.BuyLandCredit:
                    var delta = asset.Price - user.Person.Cash;
                    var credit = (int)Math.Ceiling(delta / 1_000d) * 1_000;

                    user.GetCredit(credit);
                    CompleteTransaction();
                    return;
            }

            void CompleteTransaction()
            {
                user.Person.Cash -= asset.Price;
                asset.IsDraft = false;
                user.History.Add(ActionType.BuyLand, asset.Id);

                AvailableAssets.Add(asset.Title, AssetType.LandTitle);
                AvailableAssets.Add(asset.Price, AssetType.LandBuyPrice);

                SmallCircleButtons(bot, user, Terms.Get(13, user, "Done."));
            }
        }

        public static void BuyBusiness(TelegramBotClient bot, User user)
        {
            user.Stage = Stage.BuyBusinessTitle;
            var businesses = AvailableAssets.Get(user.Person.BigCircle ? AssetType.BigBusinessType : AssetType.BusinessType).ToArray();
            bot.SetButtons(user.Id, Terms.Get(7, user, "Title:"), businesses.Append(Terms.Get(6, user, "Cancel")));
        }

        public static void BuyBusiness(TelegramBotClient bot, User user, string value)
        {
            var title = value.Trim().ToUpper();
            var number = value.AsCurrency();
            var asset = user.Person.Assets.Businesses.FirstOrDefault(a => a.IsDraft) ??
                        user.Person.Assets.Add(title, AssetType.Business, user.Person.BigCircle);
            var prices = AvailableAssets.Get(user.Person.BigCircle ? AssetType.BigBusinessBuyPrice : AssetType.BusinessBuyPrice)
                .AsCurrency().Append(Terms.Get(6, user, "Cancel"));
            var firstPayments = AvailableAssets.Get(AssetType.BusinessFirstPayment)
                .AsCurrency().Append(Terms.Get(6, user, "Cancel"));
            var cashFlows = AvailableAssets.Get(user.Person.BigCircle ? AssetType.BigBusinessCashFlow : AssetType.BusinessCashFlow)
                .AsCurrency().Append(Terms.Get(6, user, "Cancel"));

            switch (user.Stage)
            {
                case Stage.BuyBusinessTitle:
                    user.Stage = Stage.BuyBusinessPrice;
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

                    asset.Price = number;

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
                    asset.Mortgage = asset.Price - number;

                    if (number <= 0)
                    {
                        bot.SendMessage(user.Id, Terms.Get(11, user, "Invalid first payment amount."));
                        return;
                    }

                    if (user.Person.Cash < number)
                    {
                        var message = Terms.Get(23, user, "You don''t have {0}, but only {1}", number.AsCurrency(), user.Person.Cash.AsCurrency());

                        bot.SetButtons(user.Id, message, Terms.Get(34, user, "Get Credit"), Terms.Get(6, user, "Cancel"));
                        return;
                    }

                    user.Stage = Stage.BuyBusinessCashFlow;
                    user.Person.Cash -= number;

                    bot.SetButtons(user.Id, Terms.Get(12, user, "What is the cash flow?"), cashFlows);
                    return;

                case Stage.BuyBusinessCredit:
                    number = asset.Price - asset.Mortgage;
                    var delta = number - user.Person.Cash;
                    var credit = (int)Math.Ceiling(delta / 1_000d) * 1_000;

                    user.GetCredit(credit);
                    user.Stage = Stage.BuyBusinessCashFlow;
                    user.Person.Cash -= number;

                    bot.SendMessage(user.Id, Terms.Get(88, user, "You've taken {0} from bank.", credit.AsCurrency()));
                    bot.SetButtons(user.Id, Terms.Get(12, user, "What is the cash flow?"), cashFlows);
                    return;

                case Stage.BuyBusinessCashFlow:
                    asset.CashFlow = number;
                    asset.IsDraft = false;
                    user.History.Add(ActionType.BuyBusiness, asset.Id);

                    AvailableAssets.Add(asset.Title, user.Person.BigCircle ? AssetType.BigBusinessType : AssetType.BusinessType);
                    AvailableAssets.Add(asset.Price, user.Person.BigCircle ? AssetType.BigBusinessBuyPrice : AssetType.BusinessBuyPrice);
                    AvailableAssets.Add(asset.Price - asset.Mortgage, AssetType.BusinessFirstPayment);
                    AvailableAssets.Add(asset.CashFlow, user.Person.BigCircle ? AssetType.BigBusinessCashFlow : AssetType.BusinessCashFlow);

                    bot.SendMessage(user.Id, Terms.Get(13, user, "Done."));
                    Cancel(bot, user);
                    return;
            }
        }

        public static void BuyRealEstate(TelegramBotClient bot, User user)
        {
            var properties = AvailableAssets.Get(user.Person.SmallRealEstate ? AssetType.RealEstateSmallType : AssetType.RealEstateBigType)
                .OrderBy(x => x.Length)
                .ThenBy(x => x)
                .ToArray();

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
            var asset = user.Person.Assets.RealEstates.FirstOrDefault(a => a.IsDraft) ?? user.Person.Assets.Add(title, AssetType.RealEstate);
            var prices = AvailableAssets.Get(user.Person.SmallRealEstate ? AssetType.RealEstateSmallBuyPrice : AssetType.RealEstateBigBuyPrice)
                .AsCurrency().Append(Terms.Get(6, user, "Cancel"));
            var firstPayments = AvailableAssets.Get(user.Person.SmallRealEstate ? AssetType.RealEstateSmallFirstPayment : AssetType.RealEstateBigFirstPayment)
                .AsCurrency().Append(Terms.Get(6, user, "Cancel"));
            var cashFlows = AvailableAssets.Get(user.Person.SmallRealEstate ? AssetType.RealEstateSmallCashFlow : AssetType.RealEstateBigCashFlow)
                .AsCurrency().Append(Terms.Get(6, user, "Cancel"));

            switch (user.Stage)
            {
                case Stage.BuyRealEstateTitle:
                    user.Stage = Stage.BuyRealEstatePrice;
                    bot.SetButtons(user.Id, Terms.Get(8, user, "What is the price?"), prices);
                    return;

                case Stage.BuyRealEstatePrice:
                    if (number <= 0)
                    {
                        bot.SetButtons(user.Id, Terms.Get(9, user, "Invalid price value. Try again."), prices);
                        return;
                    }

                    asset.Price = number;
                    user.Stage = Stage.BuyRealEstateFirstPayment;

                    bot.SetButtons(user.Id, Terms.Get(10, user, "What is the first payment?"), firstPayments);
                    return;

                case Stage.BuyRealEstateFirstPayment:
                    asset.Mortgage = asset.Price - number;

                    if (number < 0)
                    {
                        bot.SendMessage(user.Id, Terms.Get(11, user, "Invalid first payment amount."));
                        return;
                    }

                    if (user.Person.Cash < number)
                    {
                        var message = Terms.Get(23, user, "You don''t have {0}, but only {1}", number.AsCurrency(), user.Person.Cash.AsCurrency());

                        bot.SetButtons(user.Id, message, Terms.Get(34, user, "Get Credit"), Terms.Get(6, user, "Cancel"));
                        return;
                    }

                    user.Stage = Stage.BuyRealEstateCashFlow;
                    bot.SetButtons(user.Id, Terms.Get(12, user, "What is the cash flow?"), cashFlows);
                    return;

                case Stage.BuyRealEstateCredit:
                    number = asset.Price - asset.Mortgage;
                    var delta = number - user.Person.Cash;
                    var credit = (int)Math.Ceiling(delta / 1_000d) * 1_000;

                    user.GetCredit(credit);
                    user.Stage = Stage.BuyRealEstateCashFlow;

                    bot.SendMessage(user.Id, Terms.Get(88, user, "You've taken {0} from bank.", credit.AsCurrency()));
                    bot.SetButtons(user.Id, Terms.Get(12, user, "What is the cash flow?"), cashFlows);
                    return;

                case Stage.BuyRealEstateCashFlow:
                    asset.CashFlow = number;
                    user.Person.Cash -= asset.Price - asset.Mortgage;
                    asset.IsDraft = false;

                    user.History.Add(ActionType.BuyRealEstate, asset.Id);

                    AvailableAssets.Add(asset.Title, user.Person.SmallRealEstate ? AssetType.RealEstateSmallType : AssetType.RealEstateBigType);
                    AvailableAssets.Add(asset.Price, user.Person.SmallRealEstate ? AssetType.RealEstateSmallBuyPrice : AssetType.RealEstateBigBuyPrice);
                    AvailableAssets.Add(asset.Price - asset.Mortgage, user.Person.SmallRealEstate ? AssetType.RealEstateSmallFirstPayment : AssetType.RealEstateBigFirstPayment);
                    AvailableAssets.Add(asset.CashFlow, user.Person.SmallRealEstate ? AssetType.RealEstateSmallCashFlow : AssetType.RealEstateBigCashFlow);

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
            var asset = user.Person.Assets.Stocks.FirstOrDefault(a => a.IsDraft) ?? user.Person.Assets.Add(title, AssetType.Stock);
            var prices = AvailableAssets.Get(AssetType.StockPrice).AsCurrency().Append(Terms.Get(6, user, "Cancel"));
            var cashFlows = AvailableAssets.Get(AssetType.StockCashFlow).AsCurrency().Append(Terms.Get(6, user, "Cancel"));

            switch (user.Stage)
            {
                case Stage.BuyStocksTitle:

                    user.Stage = Stage.BuyStocksPrice;
                    bot.SetButtons(user.Id, Terms.Get(8, user, "What is the price?"), prices);
                    return;

                case Stage.BuyStocksPrice:
                    if (number <= 0)
                    {
                        bot.SetButtons(user.Id, Terms.Get(9, user, "Invalid price value. Try again."), prices);
                        return;
                    }

                    asset.Price = number;
                    user.Stage = Stage.BuyStocksQtty;

                    int upToQtty = user.Person.Cash / number;
                    int upTo50 = upToQtty / 50 * 50;
                    bool isSimple = number < 1000;
                    var buttons = (isSimple
                        ? new[] { upToQtty, upTo50, upTo50 - 50, upTo50 - 100 }
                        : new[] { upToQtty, upToQtty - 1, upToQtty - 2, upToQtty - 3 })
                        .Where(x => x > 0)
                        .Distinct()
                        .OrderBy(x => x)
                        .Select(x => x.ToString())
                        .Append(Terms.Get(6, user, "Cancel"));

                    bot.SetButtons(user.Id, Terms.Get(17, user, "You can buy up to {0} stocks. How much stocks would you like to buy?", upToQtty), buttons);
                    return;

                case Stage.BuyStocksQtty:
                    if (number <= 0)
                    {
                        bot.SendMessage(user.Id, Terms.Get(18, user, "Invalid quantity value. Try again."));
                        return;
                    }

                    var totalPrice = asset.Price * number;
                    var availableCash = user.Person.Cash;
                    int availableQtty = availableCash / asset.Price;
                    bool isprofitable = asset.Price > 1000;

                    var defaultMsg = "{0} x {1} = {2}. You have only {3}. You can buy {4} stocks. So, what quantity of stocks do you want to buy?";
                    var message = Terms.Get(19, user, defaultMsg, number, asset.Price.AsCurrency(), totalPrice.AsCurrency(), availableCash.AsCurrency(), availableQtty);

                    if (totalPrice > availableCash)
                    {
                        bot.SendMessage(user.Id, message);
                        return;
                    }

                    asset.Qtty = number;
                    if (isprofitable)
                    {
                        user.Stage = Stage.BuyStocksCashFlow;
                        bot.SetButtons(user.Id, Terms.Get(12, user, "What is the cash flow?"), cashFlows);
                    }
                    else
                    {
                        CompleteTransaction();
                    }
                    return;

                case Stage.BuyStocksCashFlow:
                    asset.CashFlow = number;
                    CompleteTransaction();
                    return;
            }

            void CompleteTransaction()
            {
                asset.IsDraft = false;
                user.Person.Cash -= asset.Price * asset.Qtty;
                user.History.Add(ActionType.BuyStocks, asset.Id);

                AvailableAssets.Add(asset.Title, AssetType.Stock);
                AvailableAssets.Add(asset.Price, AssetType.StockPrice);

                SmallCircleButtons(bot, user, Terms.Get(13, user, "Done."));
            }
        }

        public static void BuyBoat(TelegramBotClient bot, User user)
        {
            const int firstPayment = 1_000;

            var boat = user.Person.Assets.Boat;

            if (boat != null)
            {
                bot.SendMessage(user.Id, Terms.Get(113, user, "You already have a boat."));
                Cancel(bot, user);
                return;
            }

            if (user.Person.Cash < firstPayment)
            {
                user.GetCredit(firstPayment);
                bot.SendMessage(user.Id, Terms.Get(88, user, "You've taken {0} from bank.", firstPayment.AsCurrency()));
            }

            boat = user.Person.Assets.Add(Terms.Get(116, user.Id, "Boat"), AssetType.Boat);

            boat.CashFlow = 340;
            boat.Price = 18_000;
            boat.Mortgage = 17_000;
            boat.IsDraft = false;

            user.Person.Cash -= firstPayment;
            user.History.Add(ActionType.BuyBoat, boat.Price);

            var message = Terms.Get(117, user.Id,
            "You've bot a boat for {0} in credit, first payment is {1}, monthly payment is {2}",
            boat.Price.AsCurrency(), firstPayment.AsCurrency(), boat.CashFlow.AsCurrency());
            bot.SendMessage(user.Id, message);

            Cancel(bot, user);
        }

        public static void StartCompany(TelegramBotClient bot, User user)
        {
            user.Stage = Stage.StartCompanyTitle;
            var businesses = AvailableAssets.Get(AssetType.SmallBusinessType).Append(Terms.Get(6, user, "Cancel"));

            bot.SetButtons(user.Id, Terms.Get(7, user, "Title:"), businesses);
        }

        public static void StartCompany(TelegramBotClient bot, User user, string value)
        {
            var title = value.Trim().ToUpper();
            var number = value.AsCurrency();
            var asset = user.Person.Assets.SmallBusinesses.FirstOrDefault(a => a.IsDraft) ??
                        user.Person.Assets.Add(title, AssetType.SmallBusinessType);
            var prices = AvailableAssets.Get(AssetType.SmallBusinessBuyPrice)
                .AsCurrency().Append(Terms.Get(6, user, "Cancel"));

            switch (user.Stage)
            {
                case Stage.StartCompanyTitle:
                    user.Stage = Stage.StartCompanyPrice;
                    bot.SetButtons(user.Id, Terms.Get(8, user, "What is the price?"), prices);
                    return;

                case Stage.StartCompanyPrice:
                    if (number <= 0)
                    {
                        bot.SetButtons(user.Id, Terms.Get(9, user, "Invalid price value. Try again."), prices);
                        return;
                    }

                    asset.Price = number;

                    if (user.Person.Cash < number)
                    {
                        var message = Terms.Get(23, user, "You don''t have {0}, but only {1}", number.AsCurrency(), user.Person.Cash.AsCurrency());

                        bot.SetButtons(user.Id, message, Terms.Get(34, user, "Get Credit"), Terms.Get(6, user, "Cancel"));
                        return;
                    }

                    CompleteTransaction();
                    return;

                case Stage.StartCompanyCredit:
                    var delta = asset.Price - user.Person.Cash;
                    var credit = (int)Math.Ceiling(delta / 1_000d) * 1_000;

                    user.GetCredit(credit);
                    CompleteTransaction();
                    return;
            }

            void CompleteTransaction()
            {
                user.Person.Cash -= asset.Price;

                asset.IsDraft = false;
                user.History.Add(ActionType.StartCompany, asset.Id);

                AvailableAssets.Add(asset.Title, AssetType.SmallBusinessType);
                AvailableAssets.Add(asset.Price, AssetType.SmallBusinessBuyPrice);

                bot.SendMessage(user.Id, Terms.Get(13, user, "Done."));
                Cancel(bot, user);
            }
        }

        public static void BuyCoins(TelegramBotClient bot, User user)
        {
            user.Stage = Stage.BuyCoinsTitle;
            var coins = AvailableAssets.Get(AssetType.CoinTitle).Append(Terms.Get(6, user, "Cancel"));

            bot.SetButtons(user.Id, Terms.Get(7, user, "Title:"), coins);
        }

        public static void BuyCoins(TelegramBotClient bot, User user, string value)
        {
            var title = value.Trim().ToUpper();
            var number = value.AsCurrency();
            var asset = user.Person.Assets.Coins.FirstOrDefault(a => a.IsDraft) ??
                        user.Person.Assets.Add(title, AssetType.Coin);
            var prices = AvailableAssets.Get(AssetType.CoinBuyPrice).AsCurrency().Append(Terms.Get(6, user, "Cancel"));
            var counts = AvailableAssets.Get(AssetType.CoinCount).OrderBy(x => x).Append(Terms.Get(6, user, "Cancel"));

            switch (user.Stage)
            {
                case Stage.BuyCoinsTitle:
                    user.Stage = Stage.BuyCoinsCount;
                    bot.SetButtons(user.Id, Terms.Get(21, user, "How much?"), counts);
                    return;

                case Stage.BuyCoinsCount:
                    if (number <= 0)
                    {
                        bot.SetButtons(user.Id, Terms.Get(18, user, "Invalid quantity value. Try again."), counts);
                        return;
                    }

                    user.Stage = Stage.BuyCoinsPrice;
                    asset.Qtty = number;
                    bot.SetButtons(user.Id, Terms.Get(8, user, "What is the price?"), prices);
                    return;

                case Stage.BuyCoinsPrice:
                    if (number <= 0)
                    {
                        bot.SetButtons(user.Id, Terms.Get(9, user, "Invalid price value. Try again."), prices);
                        return;
                    }

                    asset.Price = number;
                    var totalPrice = asset.Price * asset.Qtty;

                    if (user.Person.Cash < totalPrice)
                    {
                        var message = Terms.Get(23, user, "You don''t have {0}, but only {1}", totalPrice.AsCurrency(), user.Person.Cash.AsCurrency());

                        bot.SetButtons(user.Id, message, Terms.Get(34, user, "Get Credit"), Terms.Get(6, user, "Cancel"));
                        return;
                    }

                    CompleteTransaction();
                    return;

                case Stage.BuyCoinsCredit:
                    var delta = asset.Price * asset.Qtty - user.Person.Cash;
                    var credit = (int)Math.Ceiling(delta / 1_000d) * 1_000;

                    user.GetCredit(credit);
                    CompleteTransaction();
                    return;
            }

            void CompleteTransaction()
            {
                user.Person.Cash -= asset.Price * asset.Qtty;

                asset.IsDraft = false;
                user.History.Add(ActionType.BuyCoins, asset.Id);

                AvailableAssets.Add(asset.Title, AssetType.CoinTitle);
                AvailableAssets.Add(asset.Price, AssetType.CoinBuyPrice);
                AvailableAssets.Add(asset.Qtty, AssetType.CoinCount);

                bot.SendMessage(user.Id, Terms.Get(13, user, "Done."));
                Cancel(bot, user);
            }
        }
    }
}
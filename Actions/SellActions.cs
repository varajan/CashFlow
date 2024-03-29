﻿using CashFlowBot.Data;
using CashFlowBot.Extensions;
using CashFlowBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot;
using Terms = CashFlowBot.DataBase.Terms;

namespace CashFlowBot.Actions;

public class SellActions : BaseActions
{
    public static void SellLand(TelegramBotClient bot, User user)
    {
        var lands = user.Person.Assets.Lands;
        var cancel = Terms.Get(6, user, "Cancel");

        if (lands.Any())
        {
            var landsIds = new List<string>();
            var landsList = string.Empty;

            for (int i = 0; i < lands.Count; i++)
            {
                landsIds.Add($"#{i + 1}");
                landsList += $"{Environment.NewLine}*#{i + 1}* - {lands[i].Description}";
            }

            user.Stage = Stage.SellLandTitle;

            bot.SetButtons(user.Id, Terms.Get(99, user, "What Land do you want to sell?{0}{1}", Environment.NewLine, landsList), landsIds.Append(cancel));
            return;
        }

        SmallCircleButtons(bot, user, Terms.Get(100, user, "You have no Land."));
    }

    public static void SellLand(TelegramBotClient bot, User user, string value)
    {
        var lands = user.Person.Assets.Lands;
        var cancel = Terms.Get(6, user, "Cancel");
        var prices = AvailableAssets.GetAsCurrency(AssetType.LandSellPrice).Append(cancel);

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
                land.Sell(ActionType.SellLand, price);

                SmallCircleButtons(bot, user, Terms.Get(13, user, "Done."));
                return;
        }
    }

    public static void SellBusiness(TelegramBotClient bot, User user)
    {
        var cancel = Terms.Get(6, user, "Cancel");
        var businesses = user.Person.Assets.SmallBusinesses;
        businesses.AddRange(user.Person.Assets.Businesses);

        if (businesses.Any())
        {
            var businessesIds = new List<string>();
            var businessesList = string.Empty;

            for (int i = 0; i < businesses.Count; i++)
            {
                businessesIds.Add($"#{i + 1}");
                businessesList += $"{Environment.NewLine}*#{i + 1}* - {businesses[i].Description}";
            }

            businessesIds.Add(cancel);
            user.Stage = Stage.SellBusinessTitle;

            bot.SetButtons(user.Id, Terms.Get(78, user, "What Business do you want to sell?{0}{1}", Environment.NewLine, businessesList), businessesIds);
            return;
        }

        SmallCircleButtons(bot, user, Terms.Get(77, user, "You have no Business."));
    }

    public static void SellBusiness(TelegramBotClient bot, User user, string value)
    {
        var cancel = Terms.Get(6, user, "Cancel");
        var businesses = user.Person.Assets.SmallBusinesses;
        businesses.AddRange(user.Person.Assets.Businesses);

        var prices = AvailableAssets.GetAsCurrency(AssetType.BusinessSellPrice).Append(cancel);

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
                business.Sell(ActionType.SellBusiness, price);

                SmallCircleButtons(bot, user, Terms.Get(13, user, "Done."));
                return;
        }
    }

    public static void SellRealEstate(TelegramBotClient bot, User user)
    {
        var cancel = Terms.Get(6, user, "Cancel");
        var properties = user.Person.Assets.RealEstates;

        if (properties.Any())
        {
            var realEstateIds = new List<string>();
            var realEstateList = string.Empty;

            for (int i = 0; i < properties.Count; i++)
            {
                realEstateIds.Add($"#{i + 1}");
                realEstateList += $"{Environment.NewLine}*#{i + 1}* - {properties[i].Description}";
            }

            user.Stage = Stage.SellRealEstateTitle;

            bot.SetButtons(user.Id, Terms.Get(14, user, "What RealEstate do you want to sell?{0}{1}", Environment.NewLine, realEstateList), realEstateIds.Append(cancel));
            return;
        }

        SmallCircleButtons(bot, user, Terms.Get(15, user, "You have no properties."));
    }

    public static void SellRealEstate(TelegramBotClient bot, User user, string value)
    {
        var cancel = Terms.Get(6, user, "Cancel");
        var properties = user.Person.Assets.RealEstates;
        var prices = AvailableAssets.GetAsCurrency(AssetType.RealEstateSellPrice).Append(cancel);

        switch (user.Stage)
        {
            case Stage.SellRealEstateTitle:
                var index = value.Replace("#", "").ToInt();

                if (index < 1 || index > properties.Count)
                {
                    bot.SendMessage(user.Id, Terms.Get(16, user, "Invalid Real Estate number."));
                    return;
                }

                var property = properties[index - 1];
                var question = property.Title.GetApartmentsCount() == 1
                    ? Terms.Get(8, user, "What is the price?")
                    : Terms.Get(137, user, "You have *{0}* apartments. What is the price per one appartment?", property.Title.GetApartmentsCount());

                property.Title += "*";
                user.Stage = Stage.SellRealEstatePrice;

                bot.SetButtons(user.Id, question, prices);
                return;

            case Stage.SellRealEstatePrice:
                var realEstate = properties.First(x => x.Title.EndsWith("*"));
                var price = realEstate.Title.GetApartmentsCount() * value.AsCurrency();

                user.Person.Cash += price - realEstate.Mortgage;
                realEstate.Sell(ActionType.SellRealEstate, price);

                SmallCircleButtons(bot, user, Terms.Get(13, user, "Done."));
                return;
        }
    }

    public static void SellStocks(TelegramBotClient bot, User user)
    {
        var cancel = Terms.Get(6, user, "Cancel");
        var stocks = user.Person.Assets.Stocks.Select(x => x.Title).Distinct();

        if (stocks.Any())
        {
            user.Stage = Stage.SellStocksTitle;
            bot.SetButtons(user.Id, Terms.Get(27, user, "What stocks do you want to sell?"), stocks.Append(cancel));
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
        var cancel = Terms.Get(6, user, "Cancel");
        var stocks = user.Person.Assets.Stocks;
        var prices = AvailableAssets.GetAsCurrency(AssetType.StockPrice);

        switch (user.Stage)
        {
            case Stage.SellStocksTitle:
                var assets = stocks.Where(x => x.Title == title).ToList();

                if (!assets.Any())
                {
                    SmallCircleButtons(bot, user, Terms.Get(124, user, "Invalid stocks name."));
                    return;
                }

                user.Stage = Stage.SellStocksPrice;
                assets.ForEach(x => x.Title += "*");
                bot.SetButtons(user.Id, Terms.Get(8, user, "What is the price?"), prices.Append(cancel));
                return;

            case Stage.SellStocksPrice:
                var stocksToSell = stocks.Where(x => x.Title.EndsWith("*")).ToList();
                var qtty = stocksToSell.Sum(x => x.Qtty);

                if (number <= 0)
                {
                    bot.SetButtons(user.Id, Terms.Get(9, user, "Invalid price value. Try again."), prices.Append(cancel));
                    return;
                }

                user.Person.Cash += qtty * number;
                stocksToSell.ForEach(x => x.Sell(ActionType.SellStocks, number));

                SmallCircleButtons(bot, user, Terms.Get(13, user, "Done."));
                return;
        }
    }

    public static void SellCoins(TelegramBotClient bot, User user)
    {
        var cancel = Terms.Get(6, user, "Cancel");
        var coins = user.Person.Assets.Coins.Select(x => x.Title);

        if (coins.Any())
        {
            user.Stage = Stage.SellCoinsTitle;
            bot.SetButtons(user.Id, Terms.Get(122, user, "What coins do you want to sell?"), coins.Append(cancel));
        }
        else
        {
            SmallCircleButtons(bot, user, Terms.Get(121, user, "You have no coins."));
        }
    }

    public static void SellCoins(TelegramBotClient bot, User user, string value)
    {
        var title = value.Trim().ToUpper();
        var number = value.AsCurrency();
        var cancel = Terms.Get(6, user, "Cancel");
        var coin = user.Person.Assets.Coins.FirstOrDefault(x => x.Title.Equals(title, StringComparison.InvariantCultureIgnoreCase) || x.Title.EndsWith("*"));
        var prices = AvailableAssets.GetAsCurrency(AssetType.CoinSellPrice);

        switch (user.Stage)
        {
            case Stage.SellCoinsTitle:
                if (coin == null)
                {
                    SmallCircleButtons(bot, user, Terms.Get(123, user, "Invalid coins title."));
                    return;
                }

                user.Stage = Stage.SellCoinsPrice;
                coin.Title += "*";

                bot.SetButtons(user.Id, Terms.Get(8, user, "What is the price?"), prices.Append(cancel));
                return;

            case Stage.SellCoinsPrice:
                if (number <= 0)
                {
                    bot.SetButtons(user.Id, Terms.Get(9, user, "Invalid price value. Try again."), prices.Append(cancel));
                    return;
                }

                user.Person.Cash += coin.Qtty * number;
                coin.Sell(ActionType.SellCoins, number);

                SmallCircleButtons(bot, user, Terms.Get(13, user, "Done."));
                return;
        }
    }
}
using CashFlowBot.Data;
using CashFlowBot.Data.Consts;
using CashFlowBot.Data.DataBase;
using CashFlowBot.Data.Users;
using CashFlowBot.Extensions;
using CashFlowBot.Loggers;
using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CashFlowBot.Actions;

public class AdminActions
{
    private static ILogger logger = new FileLogger();
    private static IDataBase dataBase = new SQLiteDataBase(logger);
    private static IUsers Users => null;
    private static ITermsService Terms => new TermsService(dataBase);
    private static AvailableAssets AvailableAssets => new AvailableAssets(dataBase);

    public static void AdminMenu(TelegramBotClient bot, IUser user)
    {
        user.Stage = Stage.Admin;
        bot.SetButtons(user.Id, "Hi, Admin.",
            "Logs", "Bring Down", "Users", "Available Assets", "Cancel");
    }

    public static void ShowAvailableAssets(TelegramBotClient bot, IUser user, string value)
    {
        var all = value.Equals("All");
        var assetType = all ? AssetType.Boat : value.SubStringTo("-").Trim().ParseEnum<AssetType>();
        var assets = all
            ? Enum.GetValues(typeof(AssetType)).Cast<AssetType>().SelectMany(GetAssets)
            : GetAssets(assetType);

        user.Stage = Stage.AdminAvailableAssetsClear;
        bot.SetButtons(user.Id, string.Join(Environment.NewLine, assets),
            all ? "Clear ALL" : $"Clear {assetType}", "Back");

        List<string> GetAssets(AssetType type) =>
            AvailableAssets.Get(type)
                .Select(x => x.ToInt() == 0
                    ? $"*{type}*: '{x}'"
                    : $"*{type}*: '{x}' - '{x.ToInt().AsCurrency()}'")
                .ToList();
    }

    public static void ClearAvailableAssets(TelegramBotClient bot, IUser user, string value)
    {
        if (value.Equals("Clear ALL"))
        {
            AvailableAssets.ClearAll();
        }
        else
        {
            var type = value.Split(" ").Last().ParseEnum<AssetType>();
            AvailableAssets.Clear(type);
        }

        AdminMenu(bot, user);
    }

    public static void NotifyAdmins(TelegramBotClient bot, IUser user)
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

            //bot.SendTextMessageAsync(usr.Id, $"{user.Name} wants to become Admin.", replyMarkup: rkm, parseMode: ParseMode.Default);
        }
    }

    public static void ShowUsers(TelegramBotClient bot, IUser user)
    {
        var users = Users.AllUsers
            .OrderBy(x => x.LastActive)
            .Select(x => $"{(x.IsAdmin ? "••" : "•")}[{x.Id}] {x.Name} - {x.FirstLogin.AsString("yyyy.MM.dd")} - {x.LastActive.AsString()}").ToList();
        bot.SendMessage(user.Id, $"There are {users.Count} users.");
        bot.SendMessage(user.Id, string.Join(Environment.NewLine, users), ParseMode.None);
        //bot.SendMessage(user.Id, string.Join(Environment.NewLine, users), ParseMode.Default);
    }
}
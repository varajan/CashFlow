using System;
using System.Net;
using CashFlowBot.Data;
using CashFlowBot.Models;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace CashFlowBot
{
    public class CashFlowBot
    {
        private static readonly TelegramBotClient Bot = new("1991657067:AAGyDAK1xfqrfIEAFIKNsRjWOvy9owiKU40");

        public static void Main()
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            Bot.OnMessage       += BotOnMessageReceived;
            Bot.OnMessageEdited += BotOnMessageReceived;
            Bot.OnCallbackQuery += BotOnCallbackQueryReceived;
            Bot.OnReceiveError  += BotOnReceiveError;

            Bot.StartReceiving(Array.Empty<UpdateType>());

            Console.WriteLine("Starting Bot.");
            Console.ReadLine();

            Bot.StopReceiving();
        }

        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            try
            {
                var message = messageEventArgs.Message;
                var user = new User(message.Chat.Id);

                await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                if (message.Type != MessageType.Text) return;

                switch (message.Text.ToLower().Trim())
                {
                    case "/start":
                        Actions.Start(Bot, message.Chat.Id);
                        return;

                    case "/clear":
                        Actions.Clear(Bot, message.Chat.Id);
                        return;

                    case "/cancel":
                        Actions.Cancel(Bot, message.Chat.Id);
                        return;

                    // Actions
                    case "get money":
                        Actions.GetMoney(Bot, message.Chat.Id);
                        return;

                    case "show my data":
                        Actions.ShowData(Bot, message.Chat.Id);
                        return;

                    case "get credit":
                        Actions.GetCredit(Bot, message.Chat.Id);
                        return;

                    case "pay credit":
                        Actions.PayCredit(Bot, message.Chat.Id);
                        return;


                    // Buy actions
                    case "buy":
                        Actions.Buy(Bot, message.Chat.Id);
                        return;

                    case "stocks":
                        if (user.Stage == Stage.Buy)
                        {
                            Actions.BuyStocks(Bot, message.Chat.Id);
                        }

                        if (user.Stage == Stage.Sell)
                        {
                            //Actions.SellStocks(Bot, message.Chat.Id);
                        }
                        return;
                        // Buy actions

                        // Actions
                }

                switch (user.Stage)
                {
                    case Stage.GetProfession:
                        Actions.SetProfession(Bot, user.Id, message.Text.Trim().ToLower());
                        return;

                    case Stage.GetCredit:
                        Actions.GetCredit(Bot, user.Id, message.Text.Trim());
                        return;

                    case Stage.PayCredit:
                        Actions.PayCredit(Bot, user.Id, message.Text.Trim());
                        return;

                    case Stage.BuyStocksQtty:
                    case Stage.BuyStocksTitle:
                    case Stage.BuyStocksPrice:
                        Actions.BuyStocks(Bot, user.Id, message.Text.Trim());
                        return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            try
            {
                var callbackQuery = callbackQueryEventArgs.CallbackQuery;
                //var user = new User(callbackQuery.Message.Chat.Id);

                //await Bot.SendChatActionAsync(callbackQuery.Message.Chat.Id, ChatAction.Typing);

                //if (user.Stage == Stage.GetProfession)
                //{
                //    Actions.SetProfession(Bot, user.Id, callbackQuery.Data.Trim().ToLower());
                //    return;
                //}
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Console.WriteLine($"Received error: {receiveErrorEventArgs.ApiRequestException.ErrorCode} — {receiveErrorEventArgs.ApiRequestException.Message}");
        }
    }
}

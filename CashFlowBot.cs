using System;
using System.Linq;
using System.Net;
using CashFlowBot.DataBase;
using CashFlowBot.Extensions;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace CashFlowBot
{
    public class CashFlowBot
    {
        private static readonly TelegramBotClient Bot = new TelegramBotClient("1991657067:AAGyDAK1xfqrfIEAFIKNsRjWOvy9owiKU40");

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

                await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                if (message.Type != MessageType.Text) return;

                switch (message.Text.Split(' ').First())
                {
                    case "/clear":

                        Persons.Delete(message.Chat.Id);
                        Expenses.Delete(message.Chat.Id);

                        Bot.SendMessage(message.Chat.Id, "Done");
                        break;
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

                await Bot.SendChatActionAsync(callbackQuery.Message.Chat.Id, ChatAction.Typing);

                switch (callbackQuery.Data)
                {
                    //case "disableReports":
                    //    break;
                }
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

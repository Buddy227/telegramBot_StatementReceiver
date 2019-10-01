using BotTelegramFB;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace AwesomeBot
{

    class Program
    {
        private const string Address = "212.117.86.172:55455";
        static ITelegramBotClient botClient;
        public string CurAnswer { get; set; }
        public static Dictionary<int, RequestToAdmin> Forms { get; set; } = new Dictionary<int, RequestToAdmin>();

        static void Main()
        {
            WebProxy wp = new WebProxy(Address: Address, BypassOnLocal: true)
            {
                Credentials = new NetworkCredential("user1", "user1Password") // Авторизация в прокси
            }; // Подключение к прокси-серверу
            botClient = new TelegramBotClient("your_token_here", wp); // Подключение Бот-клиента по токену через прокси
            var me = botClient.GetMeAsync().Result;
            Console.WriteLine($"Hello, World! I am user {me.Id} and my name is {me.FirstName}.");


            botClient.OnCallbackQuery += BotOnCallbackQueryReceived;
            botClient.OnMessage += Bot_OnMessage;
            botClient.StartReceiving();
            Thread.Sleep(int.MaxValue);
            botClient.StopReceiving();
        }


        static void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs e) // метод - обработчик нажатия кнопок, которые появляются под сообщением.
        {
            var message = e.CallbackQuery;

            int id = message.From.Id;
            if (Forms.ContainsKey(id))
                Forms.Remove(id);
            Forms.Add(id, new RequestToAdmin()); //Создание новой формы        
            string curAnswer = Forms[id].StageText(); // Получение текста, который отправляется пользователю, в зависимости  от стадии заявки.

            botClient.SendTextMessageAsync(message.Message.Chat.Id, curAnswer); // Отправление сообщения 
        }


        static public async void Bot_OnMessage(object sender, MessageEventArgs e) // метод - обработчик получения «обычных» сообщений
        {
            int id = e.Message.From.Id;

            if (e.Message == null || e.Message.Type != MessageType.Text) return;

            var firstKeyboard = new InlineKeyboardMarkup(
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Оставить заявку!", "GO")
                        });


            if (e.Message.Text == "/start")
            {
                await botClient.SendTextMessageAsync(e.Message.Chat.Id,
                    "Добро пожаловать!\nЧтобы перезапустить бота - /start\nЧтобы оставить свою заявку нажми кнопку ниже",
                    replyMarkup: firstKeyboard);
                return;
            }
            else if (Forms.Count == 0)
                return;
            else if (Forms[id].Stage == 2)
            {
                Forms[id].Report.AppendLine(e.Message.Text);
                string curAnswer = Forms[id].StageText();
                await botClient.SendTextMessageAsync(e.Message.Chat.Id, curAnswer);
            }
            else if (Forms[id].Stage == 3)
            {
                Forms[id].Report.AppendLine(e.Message.Text);
                string curAnswer = Forms[id].StageText();
                await botClient.SendTextMessageAsync(e.Message.Chat.Id, curAnswer);
            }
            else if (Forms[id].Stage == 4)
            {
                Forms[id].Report.AppendLine(e.Message.Text);
                await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Вы успешно оформили заявку. Она отправлена администратору, ожидайте");
                await botClient.SendTextMessageAsync(e.Message.Chat.Id,
                    "Добро пожаловать!\nЧтобы перезапустить бота - /start\nЧтобы оставить свою заявку нажми кнопку ниже",
                    replyMarkup: firstKeyboard);


                var messagetoadmin = "Никнейм: " + e.Message.Chat.FirstName + " " + e.Message.Chat.LastName + "\nДата:" + e.Message.Date + "\nИмя, Место, Проблема:" + Forms[id].Report.ToString();

                Forms[id].Dispose();
                Forms.Remove(id);
                await botClient.SendTextMessageAsync(
                chatId: 1234567890, // add your administration's chatId
                text: messagetoadmin);
            }
            else
            {
                await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Не понимаю о чем ты");

            }
        }
    }
}
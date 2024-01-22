using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using static System.Net.Mime.MediaTypeNames;
using System.Data.SqlClient;

namespace TG_BOT
{

    class Program
    {
        public static ITelegramBotClient bot = new TelegramBotClient("TG-Tokien");
        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            //Подключить базу данных sql
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = "путь";
            builder.UserID = "*-*";
            builder.Password = "-*-";
            builder.InitialCatalog = "TelegramUser";
            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                connection.Open();
                Console.WriteLine("Соединение открыто");
                //Открыть таблицу TelegramUsers
                string sql = "SELECT * FROM TelegramUsers";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine("{0} {1} {2}", reader.GetInt32(0), reader.GetString(1), reader.GetString(2));
                        }
                    }
                }
                //Закрыть соединение
                connection.Close();
                Console.WriteLine("Соединение закрыто");
            }
            string[] lines = System.IO.File.ReadAllLines(@"Users.txt");
            bool flag = false;
            foreach (string line in lines)
            {
                if (line == update.Message.From.Id.ToString())
                {
                    flag = true;
                    break;
                }
            }
            if (flag == false)
            {
                await bot.SendTextMessageAsync(update.Message.Chat.Id, "Доступ запрещен");
                return;
            }
            var keyboard_categorii = new ReplyKeyboardMarkup(new[]
                {
                    new[]
                    {
                        new KeyboardButton("Meniu"),
                        new KeyboardButton("Categorii"),
                        new KeyboardButton("Nothing"),
                    },
                });
            var keyboard = new ReplyKeyboardMarkup(new[]
                {
                    new[]
                    {
                        new KeyboardButton("Comenzi"),
                        new KeyboardButton("timetable"),
                        new KeyboardButton("3"),
                    },
                    new[]
                    {
                        new KeyboardButton("4"),
                        new KeyboardButton("5"),
                        new KeyboardButton("6"),
                    },
                    new[]
                    {
                        new KeyboardButton("7"),
                        new KeyboardButton("Serching"),
                        new KeyboardButton("Meniu"),
                    },
                    new[]
                    {
                        new KeyboardButton("Help"),
                    },
                });
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                var message = update.Message;
                if (message.Text == "/start")
                {
                    await bot.SendTextMessageAsync(message.Chat.Id, "Привет, " + message.Chat.FirstName + "!", replyMarkup: keyboard);
                }

                await bot.SendTextMessageAsync(message.Chat.Id, "Поиск, введите ! перед сообщением", replyMarkup: keyboard);
                //если перед сообщением введен ! через пробел, то ищем в интернете
                if (message.Text.Contains("!"))
                {
                    //пропустить ! и пробел
                    string text = message.Text.Substring(2);
                    BalabobaGenerator balabobaGenerator = new BalabobaGenerator();
                    await bot.SendTextMessageAsync(message.Chat.Id, balabobaGenerator.Generate(text.ToString()).Result);
                }
                /*
                string[] lines1 = System.IO.File.ReadAllLines(@"Users.txt");
                foreach (string line in lines1)
                {
                    await bot.SendTextMessageAsync(Convert.ToInt32(line), "Я пошёл спать");
                }
                //зкрыть бота
                Environment.Exit(0);
                */
                //используем switch для обработки команд
                switch (message.Text)
                {
                    case "/test":
                        await bot.SendTextMessageAsync(message.Chat.Id, "Тестовое сообщение");
                        break;
                    case "/spam":
                        //написать сообщение несколько раз в чат
                        for (int i = 0; i < 10; i++)
                        {
                            await bot.SendTextMessageAsync(message.Chat.Id, "Спам");
                        }
                        break;
                    case "Help":
                        await bot.SendTextMessageAsync(message.Chat.Id, "Список команд: \n /start - начало работы \n Help - список команд \n /spam - спам \n /test - тестовое сообщение \n если перед сообщением введен ! через пробел то произойдёт поиск");
                        break;
                    case "Comenzi":
                        await bot.SendTextMessageAsync(message.Chat.Id, "Категория 1", replyMarkup: keyboard_categorii);
                        break;
                    case "Meniu":
                        await bot.SendTextMessageAsync(message.Chat.Id, "Меню", replyMarkup: keyboard);
                        break;
                    case "7":
                        
                        break;
                    case "timetable":
                        ITimeTable timeTable = new ZippyTimeTable();

                        List<DateTime> times = timeTable.BusTimes();
                        foreach (var time in times)
                        {
                            await bot.SendTextMessageAsync(message.Chat.Id, time.ToString());
                        }
                        break;
                }
            }
        }

        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Запущен бот " + bot.GetMeAsync().Result.FirstName);

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }, // receive all update types
            };
            bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );
            Console.ReadLine();
        }
    }
    internal class Data_time
    {
    }
    public class BusSendTelegram
    {
        public ITimeTable TimeTable { get; }

        public BusSendTelegram(ITimeTable timeTable)
        {
            TimeTable = timeTable;
        }
        public void SendTimeTable(int id)
        {
            TimeTable.BusTimes();
        }

    }
    public interface ITimeTable
    {
        List<DateTime> BusTimes();
    }

    public class ZippyTimeTable : ITimeTable
    {
        private readonly string _url = "https://zippybus.com/novopolotsk/route/bus/10/polotsk-novopolotsk-cherez-ekiman/1234/ekonomicheskiy-kolledzh-v-novopolotsk";
        private string _regexPattern = "<a role=\"button\" data-time=\".+?\" rel=\"nofollow\" href=\".+?\">(.+?)<sup>.?</sup></a>";

        public List<DateTime> BusTimes()
        {
            WebClient webClient = new WebClient();
            webClient.Encoding = Encoding.UTF8;

            string html = webClient.DownloadString(_url);

            MatchCollection matches = Regex.Matches(html, _regexPattern, RegexOptions.Multiline);

            List<DateTime> times = new List<DateTime>();
            foreach (Match time in matches)
            {
                DateTime dateTime = DateTime.Parse(time.Groups[1].Value);
                times.Add(dateTime);
            }
            return times;
        }
    }
    public class BalabobaGenerator
    {

        private HttpClient _client = new HttpClient();
        public async Task<string> Generate(string text)
        {
            // Make post request to https://zeapi.yandex.net
            // Raw body {"filter": 1, "into": 0, "query": "kkkkkkkkkkkkkkkkkkkkk"}
            // Content-type: Application/json
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://zeapi.yandex.net");

                var content = new StringContent($"{{\"filter\": 1, \"into\": 0, \"query\": \"{text}\"}}",
                                    Encoding.UTF8,
                                    "application/json");

                var result = await client.PostAsync("/lab/api/yalm/text3", content);
                string resultContent = await result.Content.ReadAsStringAsync();
                return JObject.Parse(resultContent)["text"].ToString();
            }
        }
    }
}
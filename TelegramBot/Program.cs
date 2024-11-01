using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var botToken = "YOUR_TELEGRAM_BOT_TOKEN";
            var cts = new CancellationTokenSource();
            var bot = new TelegramBot(botToken);


            bot.StartReceiving();

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();

        }
    }
}

using Microsoft.Extensions.Configuration;
using System.Data.Common;

namespace SITA
{
    static class Program
    {
        // Количество принимаемых подключений к серверу
        public static int MAXNUMCLIENTS;

        public static string? IP_TCP_CLIENT;
        public static int PORT_TCP_CLIENT;
        public const int PORT_LISTENER_TCP = 8080;

        static void Main(string[] args)
        {
            var directory = Directory.GetCurrentDirectory();
            var config = new ConfigurationBuilder()
                .SetBasePath(directory)
                .AddJsonFile($"appsettings.json", true, true).Build();

            PORT_TCP_CLIENT = Convert.ToInt32(config["portTCP"]);
            MAXNUMCLIENTS = Convert.ToInt32(config["maxCountClients"]);

            SITAConnection sitaConnection = new();
            sitaConnection.StartServer();
            Console.WriteLine();
        }
    }
}
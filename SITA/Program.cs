using Microsoft.Extensions.Configuration;
using System.Data.Common;

namespace SITA
{
    static class Program
    {
        // Количество принимаемых подключений к серверу
        public static int MAXNUMCLIENTS;

        public static string? ipClientTCP;
        public static int portClientTCP;
        public static string? TCPLIPClient;
        public static int TCPLportClient;
        public const int tcpPort = 8080;

        static void Main(string[] args)
        {
            var directory = Directory.GetCurrentDirectory();
            var config = new ConfigurationBuilder()
                .SetBasePath(directory)
                .AddJsonFile($"appsettings.json", true, true).Build();

            ipClientTCP = config["ipAddressTCP"];
            portClientTCP = Convert.ToInt32(config["portTCP"]);
            MAXNUMCLIENTS = Convert.ToInt32(config["maxCountClients"]);

            SITAConnection sitaConnection = new();
            sitaConnection.StartServer();
            Console.WriteLine();
        }
    }
}
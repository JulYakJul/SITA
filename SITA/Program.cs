using Microsoft.Extensions.Configuration;
using System.Data.Common;

namespace SITA
{
    static class Program
    {
        // Количество принимаемых подключений к серверу
        public static int MAX_COUNT_CLIENTS;
        public static int PORT_TCP_CLIENT;

        public static int SITA_TCP_LISTENER_PORT;
        public static string IP_TCP_CLIENT;

        static void Main(string[] args)
        {
            try
            {
                var directory = Directory.GetCurrentDirectory();
                var config = new ConfigurationBuilder()
                    .SetBasePath(directory)
                    .AddJsonFile($"appsettings.json", true, true).Build();

                PORT_TCP_CLIENT = Convert.ToInt32(Environment.GetEnvironmentVariable("PORT_TCP_CLIENT") ?? config["PORT_TCP_CLIENT"]);
                IP_TCP_CLIENT = Environment.GetEnvironmentVariable("IP_TCP_CLIENT") ?? config["IP_TCP_CLIENT"];
                MAX_COUNT_CLIENTS = Convert.ToInt32(Environment.GetEnvironmentVariable("MAX_COUNT_CLIENTS") ?? config["MAX_COUNT_CLIENTS"]);
                SITA_TCP_LISTENER_PORT = Convert.ToInt32(Environment.GetEnvironmentVariable("SITA_TCP_LISTENER_PORT") ?? config["SITA_TCP_LISTENER_PORT"]);

                SITAConnection sitaConnection = new();
                Thread threadTCP = new(() => sitaConnection.StartServer());

                threadTCP.Start();
                threadTCP.Join();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }
        }
    }
}
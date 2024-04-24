using SITA.MessageLogic.Models;
using SITA.MessageLogic;
using System.Net.Sockets;
using SITA.MessageLogic.Models.Enums;

namespace SITA
{
    static class Program
    {
        private static Task? tcpCM;
        private static readonly Random rnd = new();
        private static ByteBuffer buffer = new();

        /// <summary>
        /// Источник токенов для остановки запущенного экземпляра системы при перезапуске
        /// </summary>
        private static CancellationTokenSource _сancellationTokenSource { get; set; } = new();

        static void Main(string[] args)
        {
            try
            {
                _сancellationTokenSource.Cancel();
                _сancellationTokenSource = new();

                if (tcpCM != null)
                    tcpCM.Wait();

                TCPConnectionManager tcpConnectionManager = new(_сancellationTokenSource.Token);

                tcpConnectionManager.AddTCPServer(7991, HandleTCPListener91);
                tcpConnectionManager.AddTCPServer(7992, HandleTCPListener92);

                tcpCM = Task.Run(() => tcpConnectionManager.StartServer());

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }
        }

        static async Task HandleTCPListener91(TcpClient client)
        {
            while (!_сancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    // Поток данных, которые мы получаем с клиентов
                    NetworkStream networkStream = client.GetStream();

                    while (networkStream.DataAvailable)
                    {
                        ReadStream(client).ForEach(async message =>
                        {
                            await Console.Out.WriteLineAsync($"BISI->{message.ContentText}");
                            if (message == null && message.MessageType == MessageType.LOGIN_RQST)
                            {
                                await SendMessageAsync(client, new SITAMessage()
                                {
                                    MessageType = (rnd.Next(0, 2) % 2 == 0 ? MessageType.LOGIN_ACCEPT : MessageType.LOGIN_REJECT),
                                    ContentText = ""

                                }.GetByteData());
                                await Console.Out.WriteLineAsync("BSIS<-BSIS");
                            }
                            else
                            {
                                await SendMessageAsync(client, new SITAMessage()
                                {
                                    MessageType = MessageType.STATUS,
                                    ContentText = ""

                                }.GetByteData());
                                await Console.Out.WriteLineAsync("BSIS<-BSIS");
                            }

                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        static async Task HandleTCPListener92(TcpClient client)
        {
            while (!_сancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    // Поток данных, которые мы получаем с клиентов
                    NetworkStream networkStream = client.GetStream();

                    while (networkStream.DataAvailable)
                    {
                        ReadStream(client).ForEach(async message =>
                        {
                            await Console.Out.WriteLineAsync($"BISI->{message.ContentText}");
                            if (message == null && message.MessageType == MessageType.LOGIN_RQST)
                            {
                                await SendMessageAsync(client, new SITAMessage()
                                {
                                    MessageType = (rnd.Next(0, 2) % 2 == 0 ? MessageType.LOGIN_ACCEPT : MessageType.LOGIN_REJECT),
                                    ContentText = ""

                                }.GetByteData());
                                await Console.Out.WriteLineAsync("BSIS<-BSIS");
                            }
                            else
                            {
                                await SendMessageAsync(client, new SITAMessage()
                                {
                                    MessageType = MessageType.STATUS,
                                    ContentText = ""

                                }.GetByteData());
                                await Console.Out.WriteLineAsync("BSIS<-BSIS");
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        static async Task SendMessageAsync(TcpClient client, byte[] message)
        {
            NetworkStream ns = client.GetStream();
            await ns.WriteAsync(message);
        }

        static List<SITAMessage> ReadStream(TcpClient client)
        {
            NetworkStream networkStream = client.GetStream();
            return MessageParser.Parse(networkStream, buffer, client.Available);
        }
    }
}
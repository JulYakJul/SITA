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
        private static ByteBuffer buffer91 = new();
        private static ByteBuffer buffer92 = new();

        private static CancellationTokenSource _cancellationTokenSource7991 { get; set; } = new();
        private static CancellationTokenSource _cancellationTokenSource7992 { get; set; } = new();


        /// <summary>
        /// Источник токенов для остановки запущенного экземпляра системы при перезапуске
        /// </summary>
        private static CancellationTokenSource _сancellationTokenSource { get; set; } = new();

        static void Main(string[] args)
        {
            var a = SITAMessage.AppId;

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

                Console.WriteLine("Start");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }
            while (true) ;
        }

        static async Task HandleTCPListener91(TcpClient client)
        {
            _cancellationTokenSource7991.Cancel();
            Thread.Sleep(100);
            _cancellationTokenSource7991 = new();
            while (!_cancellationTokenSource7991.Token.IsCancellationRequested)
            {
                try
                {
                    // Поток данных, которые мы получаем с клиентов
                    NetworkStream networkStream = client.GetStream();

                    while (!_cancellationTokenSource7991.Token.IsCancellationRequested && networkStream.DataAvailable)
                    {
                        ReadStream(client, buffer91).ForEach(async message =>
                        {
                            await Console.Out.WriteLineAsync($"BSIS91->{message.MessageType.ToString()} {message.ContentText} " + DateTime.Now);
                            if (message != null && message.MessageType == MessageType.LOGIN_RQST)
                            {
                                var responce = new SITAMessage()
                                {
                                    MessageType = (rnd.Next(0, 3) % 2 == 0 ? MessageType.LOGIN_REJECT : MessageType.LOGIN_REJECT),
                                    ContentText = ""

                                };

                                await SendMessageAsync(client, responce.GetByteData());
                                await Console.Out.WriteLineAsync($"BSIS91<-{responce.MessageType.ToString()} {responce.ContentText} " + DateTime.Now);
                            }
                            else
                            {
                                var responce = new SITAMessage()
                                {
                                    MessageType = MessageType.STATUS,
                                    ContentText = ""

                                };

                                await SendMessageAsync(client, responce.GetByteData());
                                await Console.Out.WriteLineAsync($"BSIS91<-{responce.MessageType.ToString()} {responce.ContentText} " + DateTime.Now);
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
            _cancellationTokenSource7992.Cancel();
            Thread.Sleep(100);
            _cancellationTokenSource7992 = new();
            while (!_cancellationTokenSource7992.Token.IsCancellationRequested)
            {
                try
                {
                    // Поток данных, которые мы получаем с клиентов
                    NetworkStream networkStream = client.GetStream();

                    while (!_cancellationTokenSource7992.Token.IsCancellationRequested && networkStream.DataAvailable)
                    {
                        ReadStream(client, buffer92).ForEach(async message =>
                        {
                            await Console.Out.WriteLineAsync($"BSIS92->{message.MessageType.ToString()} {message.ContentText} " + DateTime.Now);
                            if (message != null && message.MessageType == MessageType.LOGIN_RQST)
                            {
                                var responce = new SITAMessage()
                                {
                                    MessageType = (rnd.Next(0, 2) % 2 == 0 ? MessageType.LOGIN_ACCEPT : MessageType.LOGIN_REJECT),
                                    ContentText = ""

                                };

                                await SendMessageAsync(client, responce.GetByteData());
                                await Console.Out.WriteLineAsync($"BSIS92<-{responce.MessageType.ToString()} {responce.ContentText} " + DateTime.Now);
                            }
                            else
                            {
                                var responce = new SITAMessage()
                                {
                                    MessageType = MessageType.STATUS,
                                    ContentText = ""

                                };

                                await SendMessageAsync(client, responce.GetByteData());
                                await Console.Out.WriteLineAsync($"BSIS92<-{responce.MessageType.ToString()} {responce.ContentText} " + DateTime.Now);
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

        static List<SITAMessage> ReadStream(TcpClient client, ByteBuffer byuffer)
        {
            NetworkStream networkStream = client.GetStream();
            return MessageParser.Parse(networkStream, byuffer, client.Available);
        }
    }
}
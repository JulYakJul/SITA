using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
//using FluentFTP;
//using FluentFTP.Helpers;

//using System.Net.WebClient;

namespace SITA
{
    internal class SITAConnection
    {
        // Высокоуровневая надстройка для прослушивающего сокета
        TcpListener server;

        TcpClient[] clients = new TcpClient[Program.MAXNUMCLIENTS];
        TcpClient TCPListenerClient = new();

        bool stopNetwork;

        // Счетчик подключенных клиентов
        int countClient = 0;

        public void StartServer()
        {
            // Предотвратим повторный запуск сервера
            if (server == null)
            {
                // Блок перехвата исключений на случай запуска одновременно
                // двух серверных приложений с одинаковым портом.
                try
                {
                    stopNetwork = false;
                    countClient = 0;

                    server = new TcpListener(IPAddress.Any, Program.tcpPort);
                    server.Start();

                    Thread acceptThread = new(AcceptClients);
                    acceptThread.Start();

                    Console.WriteLine("Сервер запущен");

                    string bsmFilePath = "путь_к_вашему_файлу.bsm";
                    SendBSMToTCPListener(bsmFilePath);

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Не удалось запустить сервер");
                    Console.WriteLine(ex.ToString());
                }
            }
        }



        // Принимаем запросы клиентов на подключение и
        // привязываем к каждому подключившемуся клиенту 
        // сокет (в данном случае объект класса TcpClient)
        // для обменом сообщений.
        public void AcceptClients()
        {
            while (true)
            {
                try
                {
                    this.clients[countClient] = server.AcceptTcpClient();
                    Thread readThread = new(ReceiveRun);
                    readThread.Start(countClient);
                    Console.WriteLine("Подключился клиент");
                    countClient++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Не удалось подключить клиента");
                    Console.WriteLine(ex.ToString());
                }

                if (countClient == Program.MAXNUMCLIENTS || stopNetwork == true)
                {
                    break;
                }
            }
        }

        // Генератор случайных чисел
        Random random = new Random();

        public void ReceiveRun(object num)
        {
            while (true)
            {
                try
                {
                    string stream = null;
                    NetworkStream networkStream = clients[(int)num].GetStream();

                    while (networkStream.DataAvailable == true)
                    {
                        byte[] buffer = new byte[clients[(int)num].Available];
                        networkStream.Read(buffer, 0, buffer.Length);
                        stream = Encoding.Default.GetString(buffer);
                        string[] messages = stream.Split(new[] { "\r\n\r\n", "\r\n\n" }, StringSplitOptions.RemoveEmptyEntries);

                        foreach (string line in messages)
                        {
                            if (line.Contains("LOGIN_REQUEST"))
                            {
                                bool authorize = random.Next(4) == 0;
                                string responseType = authorize ? "LOGIN_ACCEPT" : "LOGIN_REJECT";

                                LoginResponse loginResponse = new LoginResponse
                                {
                                    application_id = "LHR_BSI",
                                    version = "VERSION_2",
                                    type = responseType,
                                    message_id_number = 0,
                                    data_length = 0
                                };

                                string jsonResponse = JsonConvert.SerializeObject(loginResponse);
                                SendToClients(jsonResponse, TCPListenerClient);
                            }
                            else if (line.Contains("BPM"))
                            {
                                TCPListenerClient.Connect(IPAddress.Parse(Program.ipClientTCP), Program.portClientTCP);
                                // подтверждение получения BPM обратно клиенту.
                                SendToClients("BPM_ACK", TCPListenerClient);

                                // отправка BSM на сервер TCPListener, передавая путь к файлу
                                //SendBSMToTCPListener(line); // Предполагается, что line содержит путь к файлу
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                if (stopNetwork == true)
                {
                    break;
                }
            }
        }

        // Метод для отправки BSM на сервер TCPListener
        public void SendBSMToTCPListener(string filePath)
        {
            try
            {
                // Чтение содержимого файла в виде массива байтов
                byte[] fileBytes = File.ReadAllBytes(filePath);

                // Отправка массива байтов на сервер TCPListener
                NetworkStream ns = TCPListenerClient.GetStream();
                ns.Write(fileBytes, 0, fileBytes.Length);

                SendToClients("BSM_SENT", TCPListenerClient);

                Console.WriteLine($"Отправлен BSM размером {fileBytes.Length} байт");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отправке BSM: {ex.ToString()}");
            }
        }



        public void SendToClients(string text, TcpClient tcpClient)
        {
            // Подготовка и запуск асинхронной отправки сообщения.
            NetworkStream ns = tcpClient.GetStream();
            byte[] myReadBuffer = Encoding.Default.GetBytes(text);
            ns.BeginWrite(myReadBuffer, 0, myReadBuffer.Length, new AsyncCallback(AsyncSendCompleted), ns);
        }

        // Асинхронная отправка сообщения клиенту.
        public void AsyncSendCompleted(IAsyncResult ar)
        {
            NetworkStream ns = (NetworkStream)ar.AsyncState;
            ns.EndWrite(ar);
        }
    }
}

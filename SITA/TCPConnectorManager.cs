using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net;

namespace SITA
{
    public class TCPConnectionManager
    {

        private readonly ConcurrentDictionary<Task<TcpClient>, (int, TcpListener)> TcpListeners = new();

        private readonly ConcurrentDictionary<int, Func<TcpClient, Task>> Handlers = new();

        private readonly CancellationToken token;

        public TCPConnectionManager(CancellationToken cancellationToken)
        {
            token = cancellationToken;
        }

        public void AddTCPServer(int port, Func<TcpClient, Task> handler)
        {
            try
            {
                var tcpListener = new TcpListener(IPAddress.Any, port);
                tcpListener.Start();
                var task = tcpListener.AcceptTcpClientAsync();
                TcpListeners.TryAdd(task, (port, tcpListener));
                Handlers.TryAdd(port, handler);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task StartServer()
        {
            Task<TcpClient> tcpClientTask;

            var cancelationTask = Task.Run(() => CancelationTask());

            while ((tcpClientTask = await Task.WhenAny(TcpListeners.Keys.Append(cancelationTask))) != null)
            {
                try
                {
                    if (token.IsCancellationRequested)
                    {
                        foreach (var item in TcpListeners.Keys)
                        {
                            TcpListeners[item].Item2.Stop();
                        }
                        return;
                    }

                    var tcpListenerPortPair = TcpListeners[tcpClientTask];
                    var port = tcpListenerPortPair.Item1;
                    var tcpListener = tcpListenerPortPair.Item2;
                    Console.WriteLine($"Client connected on port {port}");

#pragma warning disable CS4014 // Так как этот вызов не ожидается, выполнение существующего метода продолжается до тех пор, пока вызов не будет завершен
                    Task.Run(() => Handlers[port](tcpClientTask.Result));
#pragma warning restore CS4014 // Так как этот вызов не ожидается, выполнение существующего метода продолжается до тех пор, пока вызов не будет завершен

                    TcpListeners.TryRemove(tcpClientTask, out _);

                    var task = tcpListener.AcceptTcpClientAsync(token);
                    TcpListeners.TryAdd(task.AsTask(), tcpListenerPortPair);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private TcpClient CancelationTask()
        {
            token.WaitHandle.WaitOne();
            return null;
        }
    }
}

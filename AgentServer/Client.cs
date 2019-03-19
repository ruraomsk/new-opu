using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using inout;
using loggers;

namespace AgentServer
{
    class Client
    {
        ServerOPU server;
        TcpClient client;
        NetworkStream stream;
        Thread thread;

        string endString = "\r\n\r\n";
        bool Connect = false;

        public Client(ServerOPU server, TcpClient client)
        {
            this.server = server;
            this.client = client;
        }

        public void Start()
        {
            stream = client.GetStream();
            thread = new Thread(this.Run);
            thread.Start();
            Connect = true;
        }

        public void Stop()
        {
            Connect = false;
            stream.Close();
            client.Close();
            thread.Abort();
            thread.Join();
            Log.Debug("Client", "Client stopped!");
        }

        private void Run()
        {
            byte[] buffer = new byte[1024];
            while (Connect)
            {
                string request = "";
                int count;

                try {
                    while ((count = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        request += Encoding.UTF8.GetString(buffer, 0, count);
                        if (request.IndexOf(endString) >= 0) break;
                    }
                }
                catch (Exception e) {
//                    Log.Info ("AgentServer Client", "Ошибка чтения из потока: " + e.ToString());
                    Log.Info ("AgentServer Client", "Ошибка чтения из потока: вероятно, удаленный хост принудительно разорвал соединения.");
                    Connect = false;
                }

                if (!Connect) break;
                if (request.Length == 0)
                {
                    Thread.Sleep(200);
                    continue;
                }
                  
                Parser parser = new Parser(request, server);
                byte[] result = Encoding.UTF8.GetBytes(parser.GetResponse()+endString);

                try {
                    stream.Write(result, 0, result.Length);
                }
                catch (Exception e) {
//                    Log.Info("AgentServer Client", "Ошибка записи в поток: " + e.ToString());
                    Log.Info("AgentServer Client", "Ошибка записи в поток: вероятно, удаленный хост принудительно разорвал соединения.");
                    Connect = false;
                }
            }
            Stop();
        }
    }
}

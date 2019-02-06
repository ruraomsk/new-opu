using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using inout;
using loggers;
namespace AgentServer
{
    public class AgServer
    {
        ServerOPU server;
        int port;
        TcpListener listner;
        Thread thread;
        List<Client> clients;
        bool Connect = true;

        public AgServer(ServerOPU server, int port)
        {
            this.server = server;
            this.port = port;
            clients = new List<Client>();
            thread = new Thread(this.Run);
            thread.Start();

        }
        public void Stop()
        {
            Connect = false;
            listner.Stop();
            foreach (Client client in clients)
            {
                client.Stop();
            }
            thread.Abort();
            thread.Join();
        }
        public void Run()
        {
            listner = new TcpListener(IPAddress.Any, port);
            listner.Start();
            while (Connect)
            {
                try
                {
                    TcpClient clientTcp = listner.AcceptTcpClient();
                    

                    Client client = new Client(server, clientTcp);
                    client.Start();             

                    clients.Add(client);

                    Log.Info("AgServer", " Добавлен клиент " + Convert.ToString(((System.Net.IPEndPoint)clientTcp.Client.RemoteEndPoint).Address));
                } catch (Exception ex)
                {
                    Log.Info("AgServer", " Ошибка " + ex.Message);
                    return;
                }
            }
        }
    }
}

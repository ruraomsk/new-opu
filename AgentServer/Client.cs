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
    class Client
    {
        ServerOPU server;
        TcpClient client;
        NetworkStream stream;
        Thread thread;
        string endString = "\r\n\r\n";
        bool Connect = true;

        public Client(ServerOPU server, TcpClient client)
        {
            this.server = server;
            this.client = client;
            stream = client.GetStream();
            thread = new Thread(this.Run);
            thread.Start();
        }
        public void Stop()
        {
            Connect = false;
            stream.Close();
            client.Close();
            thread.Abort();
            thread.Join();
        }
        public void Run()
        {
            byte[] buffer = new byte[1024];
            while (Connect)
            {
                string request = "";
                int count;
                while ((count = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    request += Encoding.UTF8.GetString(buffer, 0, count);
                    if (request.IndexOf(endString) >= 0) break;
                }
                if (!Connect) break;
                if (request.Length == 0)
                {
                    Thread.Sleep(200);
                    continue;
                }
                //Log.Debug("Client", request);
                    
                Parser parser = new Parser(request, server);
                byte[] result = Encoding.UTF8.GetBytes(parser.GetResponse()+endString);
                stream.Write(result,0,result.Length);

            }
        }
    }
}

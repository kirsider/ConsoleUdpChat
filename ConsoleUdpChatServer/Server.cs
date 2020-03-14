using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Net;

namespace ConsoleUdpChatServer
{
    class Server
    {
        const string localhost = "127.0.0.1";
        const int serverPort = 8000;

        private UdpClient udpServer;
        private Dictionary<int, string> clients;
        private Dictionary<string, int[]> groups;

        public Server()
        {
            clients = new Dictionary<int, string>();
            groups = new Dictionary<string, int[]>();
        }

        public void Start()
        {
            udpServer = new UdpClient(serverPort);
            udpServer.BeginReceive(new AsyncCallback(ReceiveCallback), null);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                lock (udpServer)
                {
                    IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);

                    if (udpServer.Client == null)
                    {
                        Console.WriteLine("no client");
                        return;
                    }

                    var bytes = udpServer.EndReceive(ar, ref endPoint);
                    var message = Encoding.Default.GetString(bytes);
                    Console.WriteLine(message);

                    string[] tokens = message.Split(':');

                    if (tokens[0] == "/clients")
                    {
                        SendMessage(ClientsList(), endPoint.Port);    
                    } 
                    else if (tokens[0] == "/check")
                    {
                        if (!clients.ContainsKey(int.Parse(tokens[1])))
                        {
                            SendMessage("/noclient", endPoint.Port);
                        }
                    }
                    else if (tokens[0] == "/create")
                    {
                        if (groups.ContainsKey(tokens[1]))
                        {
                            SendMessage("Such group is already exists.", endPoint.Port);
                        } 
                        else
                        {
                            int[] ports = new int[tokens.Length - 2];
                            for (int i = 2; i < tokens.Length; i++)
                            {
                                ports[i - 2] = int.Parse(tokens[i]);
                            }
                            groups[tokens[1]] = ports;
                            SendMessage($"Group was created: {tokens[1]}", endPoint.Port);
                        } 
                    }
                    else if (tokens[0] == "/grouplist")
                    {
                        StringBuilder sb = new StringBuilder();
                        int port = endPoint.Port;
                        int cnt = 0;
                        foreach (var group in groups)
                        {
                            if (group.Value.Contains(port))
                            {
                                cnt++;
                                sb.Append($"{cnt}.{group.Key}\n");
                            }
                        }
                        SendMessage(sb.ToString(), port);
                    }
                    else if (tokens[0] == "/group")
                    {
                        if (groups.ContainsKey(tokens[1]) && groups[tokens[1]].Contains(endPoint.Port))
                        {
                            foreach (var port in groups[tokens[1]])
                            {
                                if (port != endPoint.Port)
                                {
                                    SendMessage($"({tokens[1]})[{clients[endPoint.Port]}]:{tokens[2]}", port);
                                }
                            }
                        }
                        else
                        {
                            SendMessage("No such group.", endPoint.Port);
                        }
                    }
                    else if (tokens[0] == "/checkgroup")
                    {
                        if (!groups.ContainsKey(tokens[1]))
                        {
                            SendMessage("/nogroup", endPoint.Port);
                        }
                    }
                    else if (tokens[0] == "/send")
                    {
                        if (clients.ContainsKey(int.Parse(tokens[1])))
                            SendMessage($"[{clients[endPoint.Port]}]:{tokens[2]}", int.Parse(tokens[1]));
                    }
                    else if (tokens[0] == "/exit")
                    {
                        clients.Remove(endPoint.Port);
                    }
                    else
                    {
                        clients.Add(endPoint.Port, message);
                        SendMessage("OK", endPoint.Port);
                    }
                    
                    udpServer.BeginReceive(ReceiveCallback, null);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void SendMessage(string message, int port)
        {
            var bytes = Encoding.Default.GetBytes(message);
            var endPoint = new IPEndPoint(IPAddress.Loopback, port);
            
            udpServer.Send(bytes, message.Length, endPoint);
        }

        private string ClientsList()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in clients)
            {
                sb.Append(item.Value + ":" + item.Key + "\n");
            }
            return sb.ToString();
        }
    }
}

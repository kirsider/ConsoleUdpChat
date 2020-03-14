using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleUdpChat
{
    class Client
    {
        const string localhost = "127.0.0.1";
        const int serverPort = 8000;

        private bool isOnlineClient;
        private bool isThereGroup;

        private UdpClient udpClient;

        public int Port { get; set; }
        public string Name { get; set; }

        public Client(int port, string name)
        {
            Port = port;
            Name = name;

            udpClient = new UdpClient(Port);
        }

        public void Start()
        {
            udpClient.Connect(localhost, serverPort);
            SendMessageOnServer($"{Name}");
            udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), null);
        }

        public void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                lock (udpClient)
                {
                    isOnlineClient = true;
                    isThereGroup = true;
                    IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, 0);
                    byte[] bytes = udpClient.EndReceive(ar, ref endpoint);
                    string message = Encoding.Default.GetString(bytes);

                    if (message == "/noclient")
                        isOnlineClient = false;
                    else if (message == "/nogroup")
                        isThereGroup = false;
                    else
                        Console.WriteLine(message);

                    udpClient.BeginReceive(ReceiveCallback, null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void SendMessageOnServer(string message)
        {
            var bytes = Encoding.Default.GetBytes(message);
            udpClient.Send(bytes, message.Length);
        }

        public void GetOnlineClients() => SendMessageOnServer("/clients");

        public void GetGroupList() => SendMessageOnServer("/grouplist");

        public void InitChat(int receivePort)
        {
            ValidateClient(receivePort);
            Thread.Sleep(100);
            if (isOnlineClient)
            {
                Console.WriteLine("Можете отправлять сообщения.");
                while (true)
                {
                    string message = Console.ReadLine();
                    if (message.Equals("/menu"))
                    {
                        break;
                    }
                    SendMessageOnServer($"/send:{receivePort}:{message}");
                }
            }
            else
            {
                Console.WriteLine("Этот клиент сейчас недоступен.");
            }
        }

        private void ValidateClient(int port) => SendMessageOnServer($"/check:{port}");

        private void ValidateGroup(string groupName) => SendMessageOnServer($"/checkgroup:{groupName}");

        public void CreateGroup(string groupName)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"/create:{groupName}");
            GetOnlineClients();
            Thread.Sleep(100);
            Console.Write("\nВведите количество участников группы: ");
            int num = int.Parse(Console.ReadLine());

            for (int i = 0; i < num; i++)
            {
                Console.Write("Введите порт нового участника: ");
                int port = int.Parse(Console.ReadLine());
                sb.Append($":{port}");
            }

            SendMessageOnServer(sb.ToString());
        }
        
        public void InitGroupChat(string groupName)
        {
            ValidateGroup(groupName);
            Thread.Sleep(100);
            if (isThereGroup)
            {
                Console.WriteLine("Можете отправлять сообщения.");
                while (true)
                {
                    string message = Console.ReadLine();
                    if (message.Equals("/menu"))
                    {
                        break;
                    }
                    SendMessageOnServer($"/group:{groupName}:{message}");
                }
            }
            else
            {
                Console.WriteLine("Такой группы нет.");
            }
        }

        public void Close()
        {
            SendMessageOnServer("/exit");
            udpClient.Close();
        }
    }
}

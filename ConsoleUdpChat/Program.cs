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
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Введите свое имя: ");
            string clientName = Console.ReadLine();
            Console.Write("Введите ваш порт: ");
            int clientPort = int.Parse(Console.ReadLine());
           
            try
            {
                Client client = new Client(clientPort, clientName);
                client.Start();

                while (true)
                {
                    Thread.Sleep(400);
                    ShowMenu();
                    Console.Write("Выбор: ");
                    int choice = int.Parse(Console.ReadLine());

                    switch (choice)
                    {
                        case 1:
                            client.GetOnlineClients();
                            break;
                        case 2:
                            Console.Write("Введите порт собеседника: ");
                            int receivePort = int.Parse(Console.ReadLine());
                            client.InitChat(receivePort);
                            break;
                        case 3:
                            Console.Write("Введите название группы: ");
                            string groupName = Console.ReadLine();
                            client.CreateGroup(groupName);
                            break;
                        case 4:
                            client.GetGroupList();
                            break;
                        case 5:
                            Console.Write("Введите название группы: ");
                            string group = Console.ReadLine();
                            client.InitGroupChat(group);
                            break;
                        case 6:
                            ShowHelp();
                            break;
                        case 0:
                            break;
                        default:
                            Console.WriteLine("Нет такой опции. Повторите ввод.");
                            break;
                    }

                    if (choice == 0)
                        break;
                }

                Console.WriteLine("Для завершения работы нажмите любую клавишу...");
                Console.ReadKey();
                client.Close();
            }
            catch (Exception)
            {
                Console.WriteLine("Сервер сейчас не доступен. Попробуйте подключиться позже.");
                Console.ReadKey();
            }
            
        }

        private static void ShowMenu()
        {
            Console.WriteLine();
            Console.WriteLine("МЕНЮ");
            Console.WriteLine("1. Список пользователей"); 
            Console.WriteLine("2. Отправить сообщение клиенту"); 
            Console.WriteLine("3. Создать группу"); 
            Console.WriteLine("4. Мои группы"); 
            Console.WriteLine("5. Групповой чат");
            Console.WriteLine("6. Помощь");
            Console.WriteLine("0. Выйти"); 
            Console.WriteLine();
        }

        private static void ShowHelp()
        {
            Console.WriteLine("ПОМОЩЬ");
            Console.WriteLine("1. Чтобы выйти из чата в меню, отправьте /menu.");
        }
    }
}

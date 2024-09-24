using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace BasicSocketServer
{
    class Program
    {
        private static Socket listener;
        private static bool isRunning = true;
        private static List<Socket> connectedClients = new List<Socket>();

        static void Main(string[] args)
        {
            Console.WriteLine("기본 TCP 서버");
            StartListening();
        }

        public static void StartListening()
        {
            listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7000);
                listener.Bind(localEndPoint);
                listener.Listen(10);

                Console.WriteLine("연결 대기 중...");

                Thread commandThread = new Thread(CommandListener);
                commandThread.Start();

                while (isRunning)
                {
                    Socket handler = listener.Accept();
                    Console.WriteLine("클라이언트 연결됨...");
                    connectedClients.Add(handler);

                    Thread clientThread = new Thread(HandleClient);
                    clientThread.Start(handler);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                listener.Close();
            }

            Console.WriteLine("\n계속하려면 ENTER를 누르세요...");
            Console.Read();
        }

        private static void HandleClient(object obj)
        {
            Socket clientSocket = (Socket)obj;
            byte[] bytes = new byte[8192];

            try
            {
                while (true)
                {
                    int bytesRec = clientSocket.Receive(bytes);
                    string data = Encoding.UTF8.GetString(bytes, 0, bytesRec);
                    Console.WriteLine("수신된 텍스트 : {0}", data);

                    if (data.Equals("Quit"))
                    {
                        Console.WriteLine("클라이언트 연결 종료됨");
                        break;
                    }

                    foreach (Socket client in connectedClients)
                    {
                        if (client.Connected)
                        {
                            client.Send(bytes, 0, bytesRec, SocketFlags.None);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                clientSocket.Close();
            }
        }

        private static void CommandListener()
        {
            while (isRunning)
            {
                string command = Console.ReadLine();

                if (command.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    isRunning = false;
                    Console.WriteLine("서버를 종료합니다...");
                    listener.Close();
                }
                else
                {
                    Console.WriteLine("알 수 없는 명령어입니다. 'exit'를 입력하면 서버가 종료됩니다.");
                }
            }
        }
    }
}

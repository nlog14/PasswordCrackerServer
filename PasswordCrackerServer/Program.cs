using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PasswordCrackerServer
{
    class Program
    {
        static List<string> passwords = ReadPasswordFile(); 

        static void Main(string[] args)
        {
            Console.WriteLine("This is the server");

            TcpListener listener = new TcpListener(IPAddress.Any, 10000);

            listener.Start();

            while (true)
            {
                TcpClient socket = listener.AcceptTcpClient();

                Task.Run(() => HandleClient(socket));
            }
            //listener.Stop();
        }

        public static void HandleClient(TcpClient socket)
        {
            Console.WriteLine("Client connected");
            NetworkStream ns = socket.GetStream();

            StreamReader reader = new StreamReader(ns);
            StreamWriter writer = new StreamWriter(ns);

            string ClientRequestPassword = reader.ReadLine();

            if (ClientRequestPassword == "password")
            {
                Console.WriteLine("Password requested");
                SendPassword(socket);

                Console.WriteLine("Password sent");
            }

            string ClientRequestDictionary = reader.ReadLine();

            if (ClientRequestDictionary == "dictionary")
            {
                Console.WriteLine("Dictionary requested");

                string path = "D:/ComputerScience_ED/2ndYearCS/4thSemester/ITSecurity";

                socket.Client.SendFile(path + "\\" + "webster-dictionary.txt");

                Console.WriteLine("Dictionary sent");
            }
            writer.Flush();
            socket.Close();
        }

        public static void SendPassword(TcpClient socket)
        {
            NetworkStream ns = socket.GetStream();
            StreamWriter writer = new StreamWriter(ns);

            //var passwords = ReadPasswordFile();
            writer.WriteLine(passwords[0]);
            passwords.RemoveAt(0);

            if (passwords.Count == 0)
            {
                Console.WriteLine("List is empty. No more passwords available");
            }
            writer.Flush();
        }

        public static List<string> ReadPasswordFile()
        {
            List<string> result = new List<string>();
            foreach (var password in File.ReadLines("D:/ComputerScience_ED/2ndYearCS/4thSemester/ITSecurity" + "\\" + "passwords.txt"))
            {
                result.Add(password);
            }
            return result;
        }
    }
}

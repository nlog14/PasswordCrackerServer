using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PasswordCrackerServer
{
    class Program
    {
        private static List<string> _encryptedPasswords = new();
        private static List<string> _decryptedPasswords = new();

        static void Main(string[] args)
        {
            Console.WriteLine("This is the server");

            _encryptedPasswords = File.ReadLines("passwords.txt").ToList();

            TcpListener listener = new TcpListener(IPAddress.Any, 10000);

            listener.Start();

            while (true)
            {
                TcpClient socket = listener.AcceptTcpClient();

                Task.Run(() => HandleClient(socket));
            }

            listener.Stop();
        }

        private static void HandleClient(TcpClient socket)
        {
            try
            {
                Console.WriteLine("Client connected");
                NetworkStream ns = socket.GetStream();

                StreamReader reader = new StreamReader(ns);
                StreamWriter writer = new StreamWriter(ns);
                bool connected = true;
                while (connected)
                {
                    string request = reader.ReadLine();

                    switch (request)
                    {
                        case "dictionary":
                            Console.WriteLine("dictionary requested");
                            socket.Client.SendFile(@"webster-dictionary.txt");
                            Console.WriteLine("Dictionary sent");
                            break;
                        case "password":
                            Console.WriteLine("Password requested");
                            SendPassword(socket);
                            break;
                        case "report":
                            ReceiveReport(reader, writer);
                            break;
                        default:
                            connected = false;
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Disconnected");
            }

            socket.Close();
        }

        private static void SendPassword(TcpClient socket)
        {
            NetworkStream ns = socket.GetStream();
            StreamWriter writer = new StreamWriter(ns);

            if (_encryptedPasswords.Any())
            {
                writer.WriteLine(_encryptedPasswords[0]);
                _encryptedPasswords.RemoveAt(0);
                writer.Flush();
            }
            else
            {
                writer.WriteLine("NONE");
                writer.Flush();
            }

        }

        /// <summary>
        /// Receives a cracked password from the client.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="writer"></param>
        private static void ReceiveReport(StreamReader reader, StreamWriter writer)
        {
            writer.WriteLine("OK");
            writer.Flush();
            var password = reader.ReadLine();
            if (!string.IsNullOrWhiteSpace(password))
            {
                _decryptedPasswords.Add(password);
                WritePasswordToFile(password);
            }
        }

        /// <summary>
        /// Writes a received cracked password to a text file.
        /// </summary>
        /// <param name="password"></param>
        private static void WritePasswordToFile(string password)
        {
            string filePath = "decryptedPasswords.txt";

            if (!File.Exists(filePath))
            {
                using (StreamWriter sw = File.CreateText(filePath))
                {
                    sw.WriteLine("--- DECRYPTED USER PASSWORDS ---");
                    sw.WriteLine(password);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filePath))
                {
                    sw.WriteLine(password);
                }
            }
        }
    }
}

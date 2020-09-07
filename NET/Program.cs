using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Server
{
    public class Program
    {
        static int port;
        static string ip;

        static IPEndPoint ipPoint;
        static Socket listenSocket;
        static FileStream file;
        public static FileDetails fileDetails;
        static void Main(string[] args)
        {
            Console.Write("Введите IP сервера:");
            IPAddress iPAddress = IPAddress.Parse(Console.ReadLine());
            Console.Write("Введите порт:");
            port = int.Parse(Console.ReadLine());
            ipPoint = new IPEndPoint(iPAddress, port);
            listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Socket current = null;
            try
            {
                listenSocket.Bind(ipPoint);
                listenSocket.Listen(1);
                Console.WriteLine("Сервер запущен...");
                current = listenSocket.Accept();
                Console.WriteLine($"Клиент подключен: {current.RemoteEndPoint.ToString()}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }
            while (true)
            {
                try
                {
                    int bufferSize = 8192;
                    byte[] buffer = new byte[bufferSize];
                    Console.WriteLine("Введите путь файла:");
                    string path = @Console.ReadLine();
                    file = new FileStream(path, FileMode.Open, FileAccess.Read);

                    SendFileInfo(current);

                    Thread.Sleep(2000);

                    SendFile(current);
                }
                catch (FileNotFoundException e) { Console.WriteLine(e.Message); }
                catch (Exception e) { Console.WriteLine(e.Message); break; }

            }
        }
        static void SendFileInfo(Socket current)
        {
            fileDetails = new FileDetails();
            string fullFileName = file.Name;
            int lastIndex = fullFileName.LastIndexOf(@"\");
            string type = fullFileName.Remove(0, lastIndex + 1);
            fileDetails.FileType = type;
            fileDetails.Length = file.Length;

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(FileDetails));
            MemoryStream memoryStream = new MemoryStream();
            xmlSerializer.Serialize(memoryStream, fileDetails);
            memoryStream.Position = 0;
            byte[] buffer = new byte[memoryStream.Length];
            memoryStream.Read(buffer, 0, buffer.Length);
            memoryStream.Close();
            current.Send(buffer);
            Console.WriteLine("Данные о файле отправлены");
        }
        static void SendFile(Socket current)
        {
            int bufferSize = 8192;
            byte[] buffer;
            do
            {
                int currentSize = fileDetails.Length >(long) bufferSize ? bufferSize : (int) fileDetails.Length;
                buffer = new byte[currentSize];
                file.Read(buffer, 0, currentSize);
                current.Send(buffer);
                fileDetails.Length -= currentSize;

            } while (fileDetails.Length > 0);
            file.Close();
            Console.WriteLine("Файл отправлен");
            Thread.Sleep(1000);
        }

    }

    [Serializable]
    public class FileDetails
    {
        public string FileType { get; set; }
        public long Length { get; set; }
        
    }
}

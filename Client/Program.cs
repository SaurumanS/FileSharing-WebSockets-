using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Client
{
    class Program
    {
        static int port; // порт сервера
        static string address; // адрес сервера
        static IPEndPoint ipPoint;
        static Socket listenSocket;
        static FileStream file;
        static FileDetails fileDetails;
        static string FilesPath;
        static void Main(string[] args)
        {
            Console.Write("Введите IP сервера:");
            IPAddress iPAddress = IPAddress.Parse(Console.ReadLine());
            Console.Write("Введите порт:");
            port = int.Parse(Console.ReadLine());
            try
            {
                listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                ipPoint = new IPEndPoint(iPAddress, port);
                listenSocket.Connect(ipPoint);
                Console.WriteLine($"Подключение к серверу прошло успешно: {ipPoint.Address.ToString()}");
                Console.WriteLine("Введите дирректорию для сохранения файлов");
                FilesPath = @Console.ReadLine();
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            while (true)
            {
                try
                {
                    GetFileDetails();
                    GetFile();
                }catch(Exception e) { Console.WriteLine(e.Message); }
            }
        }
        static void GetFileDetails()
        {
            Console.WriteLine("Ожидание нового отправления...");
            byte[] buffer=new byte[8192];
            int size = listenSocket.Receive(buffer);

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(FileDetails));
            MemoryStream memoryStream = new MemoryStream();
            memoryStream.Write(buffer, 0, size);
            memoryStream.Position = 0;
            fileDetails = (FileDetails)xmlSerializer.Deserialize(memoryStream);
            memoryStream.Close();
            Console.WriteLine($"Происходит передача файла {fileDetails.FileType} , который имеет вес {fileDetails.Length} Byte");

        }
        static void GetFile()
        {
            file = new FileStream(FilesPath + fileDetails.FileType,FileMode.Create,FileAccess.ReadWrite,FileShare.ReadWrite);
            int bufferSize = 8192;
            byte[] buffer;
            do
            {
                buffer = new byte[bufferSize];
                int currentSize = listenSocket.Receive(buffer);
                file.Write(buffer, 0, currentSize);
                fileDetails.Length -= currentSize;

            } while (fileDetails.Length > 0);
            file.Close();
            Console.WriteLine("Файл полностью передан");
        }
    }

    [Serializable]
    public class FileDetails
    {
        public string FileType { get; set; }
        
        public long Length { get; set; }

    }
}

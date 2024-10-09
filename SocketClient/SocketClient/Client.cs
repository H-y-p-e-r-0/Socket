using System.Net;
using System.Net.Sockets;
using System.Text;

class Client
{
    static IPEndPoint ipEndPoint = null!;
    static TcpClient tcpClient = new TcpClient();

    static void Main()
    {
        byte whileWaiter = 0;
        bool success = false;
        ConsoleWriteline("[+] -- Hoşgeldiniz ! --", ConsoleColor.White);
        while (true)
        {
            try
            {
                ConsoleWrite("[+] Lütfen bağlanmak istediğiniz sunucu ip adresini giriniz: ", ConsoleColor.White);
                string ip = Console.ReadLine()!;
                ConsoleWrite("[+] Lütfen bağlanmak istediğiniz sunucu ip adresinin portunu giriniz: ", ConsoleColor.White);
                string port = Console.ReadLine()!;
                ipEndPoint = new IPEndPoint(IPAddress.Parse(ip), int.Parse(port));
                ConsoleWriteline("[?] Sunucuya bağlanılıyor...", ConsoleColor.Cyan);

                while (whileWaiter < 3)
                {
                    whileWaiter++;
                    try
                    {
                        tcpClient.Connect(ipEndPoint);
                        _ = Task.Run(ServerHandler);    // Sunucu dinlemesini arka plana at
                        ClientMessager();
                        success = true;
                        break;
                    }
                    catch (Exception)
                    {
                        ConsoleWriteline("[-] Sunucuya bağlanılamadı. Tekrar denemeye 3 sn...", ConsoleColor.Red);
                        Thread.Sleep(3000);
                    }
                }

                if (success) break;
                ConsoleWriteline("[-] Sunucuya bağlanılamadı, Kapalı olabilir.", ConsoleColor.Red);
                Thread.Sleep(2000);
                whileWaiter = 0;
            }
            catch (Exception)
            {
                ConsoleWriteline("[-] Hatalı bilgi girildi. Lütfen tekrar deneyin. Örnek ip: 127.0.0.1 Örnek port: 8080", ConsoleColor.Red);
            }
        }
    }

    static void ClientMessager()
    {
        ConsoleWriteline("[+] Sunucuya bağlanıldı , Sohbet aktif.", ConsoleColor.Green);

        NetworkStream stream = tcpClient.GetStream();
        try
        {
            while (true)
            {
                ConsoleWrite("[txt]> ", ConsoleColor.White);
                stream.Write(Encoding.UTF8.GetBytes(Console.ReadLine()!));
            }
        }
        catch (Exception)
        {

            ConsoleWriteline("[-] Sunucuya bağlantı kesildi. Tekrar denemeye 3 sn...", ConsoleColor.Red);
            Thread.Sleep(3000);
            Main();
        }
    }


    static async Task ServerHandler()
    {
        NetworkStream stream = tcpClient.GetStream();
        byte[] buffer = new byte[1024];
        while (true)
        {
            await stream.ReadAsync(buffer, 0, buffer.Length);
            string message = Encoding.UTF8.GetString(buffer);
            ConsoleWriteline("[//] \n" + "[*@]> " + message, ConsoleColor.Blue);
            ConsoleWrite("[txt]> ", ConsoleColor.White);
            Array.Clear(buffer, 0, buffer.Length);
        }
    }

    static void ConsoleWriteline(string message, ConsoleColor color = default)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    static void ConsoleWrite(string message, ConsoleColor color = default)
    {
        Console.ForegroundColor = color;
        Console.Write(message);
        Console.ResetColor();
    }
}
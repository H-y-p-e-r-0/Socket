using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Server
{
    static IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
    static TcpListener tcpListener = new TcpListener(ipAddress, 31310);
    static List<TcpClient> clients = new List<TcpClient>();


    static bool isRunning = false;
    static void Main()
    {
        while (true)
        {
            ConsoleWriteline("[+] -- Hoşgeldiniz --", ConsoleColor.Blue);
            ConsoleWrite("[>] Bir IP adresi girin: ", ConsoleColor.White);
            string ip = Console.ReadLine()!;
            ConsoleWrite("[>] IP adresinin dinlenecek PORT numarasını girin: ", ConsoleColor.White);
            string port = Console.ReadLine()!;

            if (IPAddress.TryParse(ip, out IPAddress? _) && int.TryParse(port, out int _))
            {
                try
                {
                    tcpListener.Start();
                    isRunning = true;
                    ConsoleWriteline($"[>] Sunucu başlatıldı. ({ipAddress})", ConsoleColor.Green);
                    AcceptClients();
                    break;
                }
                catch (Exception)
                {
                    ConsoleWriteline("[!] Sunucu başlatılamadı , Hedef ulaşılamıyor olabilir. Lütfen tekrar deneyin.", ConsoleColor.Red);
                }
            }
            else
            {
                ConsoleWriteline("[!] Hatalı IP adresi veya PORT numarası, Lütfen tekrar deneyin. Örnek IP : 127.0.0.1 , Örnek PORT : 8080.", ConsoleColor.Red);
            }
        }

        try
        {
            ConsoleWriteline("[?] Sunucu şu anda aktif , Kapatmak için herhangi bir tuşa basın.", ConsoleColor.Cyan);
            Console.ReadLine();
            isRunning = false;
            tcpListener.Stop();
            ConsoleWriteline("[?>] Sunucu durduruldu, Kapanıyor...", ConsoleColor.DarkCyan);
            Thread.Sleep(1200);
        }
        catch (Exception)
        {
            Environment.Exit(0);
        }

    }

    static async void AcceptClients()
    {
        while (isRunning)
        {
            try
            {
                ConsoleWriteline("[>] İstemci bekleniyor...", ConsoleColor.Yellow);
                TcpClient client = await tcpListener.AcceptTcpClientAsync();
                _ = Task.Run(() => HandleClient(client));
                ConsoleWriteline($"[>] İstemci bağlandı. ({client.Client.RemoteEndPoint})", ConsoleColor.DarkGreen);
            }
            catch (Exception ex)
            {
                ConsoleWriteline("[!] İstemci bağlanamadı." + ex, ConsoleColor.Red);
            }
        }
    }

    static async Task HandleClient(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];
        int byteCount;
        clients.Add(client);
        try
        {
            while ((byteCount = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                // İstemciden gelen mesajı al
                string receivedMessage = Encoding.UTF8.GetString(buffer, 0, byteCount);
                ConsoleWriteline($"[@({client.Client.RemoteEndPoint})]> {receivedMessage}", ConsoleColor.Magenta);

                await ClientMessageRPC(receivedMessage, client);

            }
        }
        catch (Exception)
        {
            ConsoleWriteline($"[!] İstemci ile iletişim kurulamadı.", ConsoleColor.Red);

            // İstemciyi listeden kaldır
            clients.Remove(client);
        }
        finally
        {
            // İstemci bağlantısını kapat
            client.Close();
            ConsoleWriteline("[!?>] İstemci bağlantısı zorunlu olarak sonlandırıldı.", ConsoleColor.DarkRed);
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

    static async Task ClientMessageRPC(string message, TcpClient senderClient)
    {
        NetworkStream stream;
        byte[] buffer;
        foreach (TcpClient client in clients)
        {
            if (client == senderClient) continue;

            stream = client.GetStream();
            buffer = Encoding.UTF8.GetBytes(message);
            await stream.WriteAsync(buffer, 0, buffer.Length);
            Array.Clear(buffer, 0, buffer.Length);
        }
    }
}
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ClientTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string serverIP = "127.0.0.1";
            int port = 5000;
            TcpClient client = null;

            Console.WriteLine("--- Client TCP pornit ---");

            // 1. Buclă de așteptare până când serverul este pornit
            while (client == null || !client.Connected)
            {
                try
                {
                    Console.WriteLine($"Se încearcă conectarea la {serverIP}:{port}...");
                    client = new TcpClient();
                    client.Connect(serverIP, port);
                }
                catch (SocketException)
                {
                    Console.WriteLine("Serverul nu este pornit încă. Reîncerc în 2 secunde...");
                    Thread.Sleep(2000); // Așteaptă 2 secunde înainte de următoarea încercare
                }
            }

            Console.WriteLine("\n✅ Conectat cu succes la server!");

            try
            {
                NetworkStream stream = client.GetStream();

                while (true)
                {
                    Console.Write("\nIntrodu mesaj (sau 'exit'): ");
                    string message = Console.ReadLine();
                    if (string.IsNullOrEmpty(message) || message.ToLower() == "exit") break;

                    byte[] dataToSend = Encoding.UTF8.GetBytes(message);
                    stream.Write(dataToSend, 0, dataToSend.Length);

                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                    {
                        Console.WriteLine("Serverul a închis conexiunea.");
                        break;
                    }

                    string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"[SERVER]: {response}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nEroare în timpul comunicării: {ex.Message}");
            }
            finally
            {
                client.Close();
                Console.WriteLine("Conexiune închisă. Apasă orice tastă pentru a ieși...");
                Console.ReadKey();
            }
        }
    }
}
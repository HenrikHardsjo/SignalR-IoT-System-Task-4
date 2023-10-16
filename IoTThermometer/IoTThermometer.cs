using Microsoft.AspNetCore.SignalR.Client;
using System.Security.Cryptography;
using System.Text;

namespace IoTThermometer
{
    internal class Program
    {
        // Endpoint för SignalR-hubben i API-anslutningen.
        private const string HUB_URL = "https://localhost:7294/thermometerHub";

        // SignalR-anslutning
        private static HubConnection hubConnection;

        private static readonly Random random = new Random();

        private static async Task Main(string[] args)
        {
            Console.WriteLine("##### IoT Thermometer #####");

            // Sätter upp en SignalR-anslutning till API:t.
            hubConnection = new HubConnectionBuilder()
                .WithUrl(HUB_URL)
                .Build();

            // Om anslutningen stängs av någon anledning, försök ansluta igen efter 3 sekunder.
            hubConnection.Closed += async (error) =>
            {
                await Task.Delay(3000);
                await hubConnection.StartAsync();
            };

            await ConnectToServer();

            // Genererar och skickar kontinuerligt temperaturdata.
            while (true)
            {
                var temperature = GenerateRandomTemperature();
                Console.WriteLine($"Genererad temperatur: {temperature}°C");
                await SendTemperature(temperature);
                Thread.Sleep(5000); // Skickar var femte sekund.
            }
        }

        // Metod för att upprätta en anslutning till servern.
        private static async Task ConnectToServer()
        {
            while (true)
            {
                try
                {
                    await hubConnection.StartAsync();
                    Console.WriteLine("Ansluten till servern.");
                    break;
                }
                catch
                {
                    Console.WriteLine("Anslutning misslyckades. Försöker igen om 5 sekunder...");
                    await Task.Delay(5000);
                }
            }
        }

        // Genererar en slumpmässigt temperatur mellan 0°C och 35°C.
        private static double GenerateRandomTemperature()
        {
            return Math.Round(random.NextDouble() * 35, 2);
        }

        // Metod för att skicka den genererade temperaturen till API:t med SignalR.
        private static async Task SendTemperature(double temperature)
        {
            try
            {
                // Konvertera temperatur till en sträng och sedan till en byte array för att kryptera.
                var encryptedTemperature = EncryptData(Encoding.UTF8.GetBytes(temperature.ToString()));

                await hubConnection.SendAsync("SendTemperature", encryptedTemperature);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fel vid skick av temperatur: {ex.Message}");
            }
        }

        // Metod för att kryptera temperaturdata med DPAPI.
        private static byte[] EncryptData(byte[] data)
        {
            try
            {
                return ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Encryption failed: {ex.Message}");
                return null;
            }
        }
    }
}
using Microsoft.AspNetCore.SignalR.Client;

namespace ClientApp
{
    internal class Program
    {
        // Endpoint för DatabaseHub i Database.
        private static readonly string hubUrl = "https://localhost:7294/thermometerHub";

        private static HubConnection hubConnection;

        private static async Task Main(string[] args)
        {
            // Skapar en ny SignalR-anslutning.
            hubConnection = new HubConnectionBuilder()
                .WithUrl(hubUrl)
                .Build();

            Console.WriteLine("##### ClientApp #####");

            // Försöker ansluta till databas.
            await ConnectToServer();
            Console.WriteLine("Ansluten till Database.");

            while (true) // Huvudloop som väntar på användarens kommandon.
            {
                Console.WriteLine("Skriv '1' för att hämta temperaturer eller 'exit' för att avsluta.");
                var input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        await FetchTemperatures();
                        break;

                    case "exit":
                        return;

                    default:
                        Console.WriteLine("Fel-inmatning. Försök igen!");
                        break;
                }
            }
        }

        // Metod som hämtar temperaturdata från databas.
        private static async Task FetchTemperatures()
        {
            try
            {
                Console.WriteLine("Begär temperaturdata...");
                var temperatures = await hubConnection.InvokeAsync<List<double>>("GetTemperaturesFromDatabase");

                foreach (var temp in temperatures)
                {
                    Console.WriteLine($"Temperatur: {temp}°C");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fel vid hämtning av temperaturer: {ex.Message}");
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
                    break;
                }
                catch
                {
                    Console.WriteLine("Anslutning misslyckades. Försöker igen om 5 sekunder...");
                    await Task.Delay(5000);
                }
            }
        }
    }
}
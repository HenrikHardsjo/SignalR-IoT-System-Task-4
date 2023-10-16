using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Lägger till SignalR-tjänster till applikationen.
builder.Services.AddSignalR();

//Lite klumpig lösning, men ser till att ##### ApiConnection ##### printas ut i cmd-rutan, så att det är lättare att hålla koll.
var app = builder.Build();

bool hasPrintedStartupMessage = false;

app.Use(async (context, next) =>
{
    if (!hasPrintedStartupMessage)
    {
        Console.WriteLine("##### ApiConnection #####");
        hasPrintedStartupMessage = true;
    }

    await next.Invoke();
});

app.UseHttpsRedirection();

//Detta för CSP-krav.
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Content-Security-Policy", "default-src 'none'");
    await next();
});

app.UseRouting();
app.UseAuthorization();

// Kartlägger SignalR-hubben till en specifik endpoint.
app.MapHub<ThermometerHub>("/thermometerHub");

app.Run();

public class ThermometerHub : Hub
{
    // Skapar en statisk anslutning till DatabaseServern.
    private static HubConnection databaseServerConnection;

    // Statisk konstruktor som initieras när klassen först används.
    static ThermometerHub()
    {
        databaseServerConnection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7239/databaseHub") // Anger URL för Database's SignalR-hub.
            .Build();

        // Startar anslutningen till Database.
        databaseServerConnection.StartAsync().Wait();
    }

    // Metod som tar emot temperaturen från IoTThermometer och skickar den vidare till Database.
    public async Task SendTemperature(byte[] encryptedTemperature)
    {
        //Denna if-sats kontrollerar att den mottagna enkrypterade datan är ok.
        if (encryptedTemperature == null || encryptedTemperature.Length == 0)
        {
            Console.WriteLine("Ogiltig eller tom krypterad data mottagen.");
            return;
        }

        double temperature = double.Parse(Encoding.UTF8.GetString(DecryptData(encryptedTemperature)));

        //Denna if-sats kontrollerar att temperatur är inom rätt spann.
        if (temperature <= 0 || temperature >= 35)
        {
            Console.WriteLine("Ogiltigt temperaturvärde mottaget.");
            return;
        }

        Console.WriteLine($"Mottagen temperatur: {temperature}°C");

        // Skicka temperaturen till DatabaseServer.
        await databaseServerConnection.SendAsync("StoreTemperature", temperature);
    }

    // Metod som hämtar temperaturdata från databas.
    public async Task<List<double>> GetTemperaturesFromDatabase()
    {
        return await databaseServerConnection.InvokeAsync<List<double>>("GetAllTemperatures");
    }

    // Metod för att dekryptera temperaturdata med DPAPI.
    private static byte[] DecryptData(byte[] encryptedData)
    {
        try
        {
            return ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.CurrentUser);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Decryption failed: {ex.Message}");
            return null;
        }
    }
}
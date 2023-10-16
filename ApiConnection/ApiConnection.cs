using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// L�gger till SignalR-tj�nster till applikationen.
builder.Services.AddSignalR();

//Lite klumpig l�sning, men ser till att ##### ApiConnection ##### printas ut i cmd-rutan, s� att det �r l�ttare att h�lla koll.
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

//Detta f�r CSP-krav.
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Content-Security-Policy", "default-src 'none'");
    await next();
});

app.UseRouting();
app.UseAuthorization();

// Kartl�gger SignalR-hubben till en specifik endpoint.
app.MapHub<ThermometerHub>("/thermometerHub");

app.Run();

public class ThermometerHub : Hub
{
    // Skapar en statisk anslutning till DatabaseServern.
    private static HubConnection databaseServerConnection;

    // Statisk konstruktor som initieras n�r klassen f�rst anv�nds.
    static ThermometerHub()
    {
        databaseServerConnection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7239/databaseHub") // Anger URL f�r Database's SignalR-hub.
            .Build();

        // Startar anslutningen till Database.
        databaseServerConnection.StartAsync().Wait();
    }

    // Metod som tar emot temperaturen fr�n IoTThermometer och skickar den vidare till Database.
    public async Task SendTemperature(byte[] encryptedTemperature)
    {
        //Denna if-sats kontrollerar att den mottagna enkrypterade datan �r ok.
        if (encryptedTemperature == null || encryptedTemperature.Length == 0)
        {
            Console.WriteLine("Ogiltig eller tom krypterad data mottagen.");
            return;
        }

        double temperature = double.Parse(Encoding.UTF8.GetString(DecryptData(encryptedTemperature)));

        //Denna if-sats kontrollerar att temperatur �r inom r�tt spann.
        if (temperature <= 0 || temperature >= 35)
        {
            Console.WriteLine("Ogiltigt temperaturv�rde mottaget.");
            return;
        }

        Console.WriteLine($"Mottagen temperatur: {temperature}�C");

        // Skicka temperaturen till DatabaseServer.
        await databaseServerConnection.SendAsync("StoreTemperature", temperature);
    }

    // Metod som h�mtar temperaturdata fr�n databas.
    public async Task<List<double>> GetTemperaturesFromDatabase()
    {
        return await databaseServerConnection.InvokeAsync<List<double>>("GetAllTemperatures");
    }

    // Metod f�r att dekryptera temperaturdata med DPAPI.
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